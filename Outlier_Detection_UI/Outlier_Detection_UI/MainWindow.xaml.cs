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

namespace Outlier_Detection_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string trainCode = @"
#import os
#os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3' 

import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

import csv

DATA_PATH = ""resources\\DataSet1_Normalized.csv""

# autoencoder model
class AnomalyDetector(Model):
  def __init__(self, latent_dim, dim):
    super(AnomalyDetector, self).__init__()
    self.encoder = tf.keras.Sequential([
      layers.Dense(7, activation=""relu""),
      #layers.Dense(4, activation=""relu""),
      layers.Dense(latent_dim, activation=""relu"")])

    self.decoder = tf.keras.Sequential([
      #layers.Dense(4, activation=""relu""),
      layers.Dense(7, activation=""relu""),
      layers.Dense(dim, activation=""sigmoid"")])

  def call(self, x):
    encoded = self.encoder(x)
    decoded = self.decoder(encoded)
    return decoded

# read data from csv only including columns X1 through X13
data = {}
with open(DATA_PATH) as csvfile:
	csvreader = csv.reader(csvfile, delimiter="","")
	raw_data = list(csvreader)
	
	# remove header row
	raw_data = raw_data[1:]
	
	# remove first 2 columns and parse floats
	for i in range(0, len(raw_data)):
		raw_data[i] = raw_data[i][2:]
		raw_data[i] = [float(elem) for elem in raw_data[i]]
		
	data = np.array(raw_data, dtype=""float32"")

# train autoencoder
autoencoder = AnomalyDetector(2, 13)
autoencoder.compile(optimizer='adam', loss='mae')
autoencoder.fit(data, data,
          epochs=10,
          batch_size=32,
          validation_data=(data, data),
          shuffle=True,
          verbose=0)

# save trained model to file
autoencoder.export(""autoencoder"")
#autoencoder.save(""autoencoder.keras"")
test = ""done""
";

        string testCode = @"
import numpy as np
import tensorflow as tf

from tensorflow.keras import layers, losses
from tensorflow.keras.models import Model

import csv

DATA_PATH = ""resources\\DataSet1_Normalized.csv""

print(tf.version.VERSION)

# average squared difference between float arrays
def vDiff(arr1, arr2):
	length = min(len(arr1), len(arr2))
	return sum([((arr1[i] - arr2[i])**2)/length for i in range(0, length)], 0)


# read data from csv only including columns X1 through X13
data = {}
with open(DATA_PATH) as csvfile:
	csvreader = csv.reader(csvfile, delimiter="","")
	raw_data = list(csvreader)
	
	# remove header row
	raw_data = raw_data[1:]
	
	# remove first 2 columns and parse floats
	for i in range(0, len(raw_data)):
		raw_data[i] = raw_data[i][2:]
		raw_data[i] = [float(elem) for elem in raw_data[i]]
		
	data = np.array(raw_data, dtype=""float32"")

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

        public MainWindow()
        {
            InitializeComponent();
          
            // temporary path to python dll just to get things working
            string pythonDll = System.IO.Path.GetFullPath(@"..\..\..\..\..\python_interpreter\python311.dll");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            Runtime.PythonDLL = pythonDll;

            //RunPython(trainCode, "test");
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
            // Implement Later
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

                output = RunPython(testCode, "result");

                // Convert Python object to string
                string result = output.ToString();

                //
                string[] theNumbers = result.Trim('[', ']').Split(',');

                double[] arr = theNumbers.Select(n => double.Parse(n)).ToArray();

                // get idx column
                List<int> idxColumn = new List<int>();
                for (int i = 0; i < arr.Length; ++i)
                {
                    idxColumn.Add(int.Parse(csv[i][0].ToString()));
                }

                ScottPlot.Plottables.Scatter plot = WpfPlot1.Plot.Add.Scatter(idxColumn.ToArray(), arr);
                plot.LineStyle.IsVisible = false;

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