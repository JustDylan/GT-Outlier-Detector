using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Python.Runtime;
using OpenTK.Audio.OpenAL;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.Data;
using ScottPlot.Plottables;
using System.Data.Common;

namespace Outlier_Detection_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // python string that takes input_data
        string trainCode = @"
#import os
#os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3' 

import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

# autoencoder model
class AnomalyDetector(Model):
  def __init__(self, latent_dim, dim):
    super(AnomalyDetector, self).__init__()
    self.encoder = tf.keras.Sequential([
      layers.Dense(10, activation=""relu""),
      #layers.Dense(4, activation=""relu""),
      layers.Dense(latent_dim, activation=""relu"")])

    self.decoder = tf.keras.Sequential([
      #layers.Dense(4, activation=""relu""),
      layers.Dense(10, activation=""relu""),
      layers.Dense(dim, activation=""sigmoid"")])

  def call(self, x):
    encoded = self.encoder(x)
    decoded = self.decoder(encoded)
    return decoded

data = np.array(input_data, dtype=""float32"")

# train autoencoder
autoencoder = AnomalyDetector(7, 13)
autoencoder.compile(optimizer='adam', loss='mae')
autoencoder.fit(data, data,
          epochs=160,
          batch_size=32,
          validation_data=(data, data),
          shuffle=True,
          verbose=0)

# save trained model to file
autoencoder.export(""autoencoder"")
#autoencoder.save(""autoencoder.keras"")
test = ""done""
";

        // python string that takes input_data and returns it as result
        string testCode = @"
import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

print(tf.version.VERSION)

# average squared difference between float arrays
def vDiff(arr1, arr2):
	length = min(len(arr1), len(arr2))
	return sum([((arr1[i] - arr2[i])**2)/length for i in range(0, length)], 0)
		
data = np.array(input_data, dtype=""float32"")

# load autoencoder from model trained by train_autoencoder
autoencoder = tf.saved_model.load(""autoencoder"")
#autoencoder = tf.keras.models.load_model(""autoencoder.keras"")

decoded_data = autoencoder.serve(data)

decoded_error = [vDiff(decoded_data[i], data[i]) for i in range(0, len(decoded_data))]
result = []
for i in decoded_error:
	result.append(float(i))

test = ""done""
";

        Scatter dataPlot;

        public MainWindow()
        {
            InitializeComponent();
          
            // temporary path to python dll just to get things working
            string pythonDll = System.IO.Path.GetFullPath(@"..\..\..\..\..\python_interpreter\python311.dll");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            Runtime.PythonDLL = pythonDll;

            // add empty plot for data
            dataPlot = WpfPlot1.Plot.Add.Scatter(new int[] { }, new float[] { });
            dataPlot.LineStyle.IsVisible = false;
        }

        /*
        pycode: python code to run
        returnedVariableName: name of variable inside python script to return the value of.
        */
        private object RunPython(string pycode, string returnedVariableName)
        {
            return RunPython(pycode, null, "", returnedVariableName);
        }

        /*
        pycode: python code to run
        parameter: value of parameter to inject into python code
        parameterName: variable name that parameter value will appear as in python code
        returnedVariableName: name of variable inside python script to return the value of.
        */
        private object RunPython(string pycode, object parameter, string parameterName, string returnedVariableName)
        {
            object returnedVariable = new object();
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                using (PyModule scope = Py.CreateScope())
                {
                    if(parameter != null)
                        scope.Set(parameterName, parameter.ToPython());
                    scope.Exec(pycode);
                    returnedVariable = scope.Get<object>(returnedVariableName);
                }
            }
            return returnedVariable;
        }

        // Temporary Needs to be changed later
        private void Model_Click(object sender, RoutedEventArgs e)
        {
            // Implement properly later

        }

        private void Train_Click(object sender, RoutedEventArgs e)
        {
            var csv = getCurrentCSV();

            if(csv != null)
                RunPython("input_data = " + getCSVString(csv) + "\n" + trainCode, "test");
        }

        // Method to open csv file and process to data grid
        private void CSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            // Change to open in specific folder later *****
            openFile.Filter = "Csv Files| *.csv";

            if (openFile.ShowDialog() == true)
            {
                var csv = ReadCSVFile.GetCSVData(openFile.FileName);

                // print csv file to DataGrid
                CSVData.ItemsSource = csv;
            }
        }

        // returns the csv in the window's datagrid
        private DataView getCurrentCSV()
        {
            return (System.Data.DataView)CSVData.ItemsSource;
        }

        private string getCSVString(DataView csv)
        {
            string csvString = "[";
            // convert csv to python variable
            // exclude first row and first two columns
            for (int r = 1; r < csv.Count; ++r)
            {
                object[] row = csv[r].Row.ItemArray;

                // add row to string
                csvString += "[";
                for (int c = 2; c < row.Length; ++c)
                {
                    csvString += c + 1 < row.Length ? row[c].ToString() + "," : row[c].ToString();
                }
                csvString += r + 1 < csv.Count ? "]," : "]";
            }

            csvString += "]";

            return csvString;
        }

        // UNDER CONSTRUCTION
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            // Change later to have user select model or python script they want to use
            //string pyPath = System.IO.Path.GetFullPath("../../../../python_scripts/Test_For_Csharp");
            //string pyCode = System.IO.File.ReadAllText(pyPath);
            object output;
            //try
            {
                DataView csv = getCurrentCSV();

                // exit if csv is not set
                if (csv == null)
                {
                    return;
                }

                // run python with csv file added
                output = RunPython("input_data = " + getCSVString(csv) + "\n" + testCode, "result");

                // Convert Python object to string
                string result = output.ToString();

                //
                string[] theNumbers = result.Trim('[', ']').Split(',');

                float[] arr = theNumbers.Select(n => float.Parse(n)).ToArray();

                // get idx column
                List<int> idxColumn = new List<int>();
                for (int i = 0; i < arr.Length; ++i)
                {
                    idxColumn.Add(int.Parse(csv[i][0].ToString()));
                }

                WpfPlot1.Plot.Remove(dataPlot);
                dataPlot = WpfPlot1.Plot.Add.Scatter(idxColumn.ToArray(), arr);
                dataPlot.LineStyle.IsVisible = false;

                WpfPlot1.Plot.Axes.AutoScale();
                WpfPlot1.Refresh();
            }
            /*catch (Exception ex)
            {
                Console.WriteLine("Error: RunPython failure.");
                Console.WriteLine("Exception Message: " + ex.Message);
            }*/

           
        }
    }
}