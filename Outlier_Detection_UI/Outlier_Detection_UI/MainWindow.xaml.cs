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

namespace Outlier_Detection_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // For testing delete later
            Loaded += (s, e) =>
            {
                double[] dataX = { 1, 2, 3, 4, 5 };
                double[] dataY = { 1, 4, 9, 16, 25 };
                WpfPlot1.Plot.Add.Scatter(dataX, dataY);
                WpfPlot1.Refresh();
            };

            // temporary path to python dll just to get things working
            string pythonDll = System.IO.Path.GetFullPath(@"..\..\..\..\..\python_interpreter\python311.dll");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            Runtime.PythonDLL = pythonDll;


            string pycode = @"
import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3' 

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

            string pycode2 = @"import os
test = os.getcwd()";
            System.Diagnostics.Debug.WriteLine("\n\n\n\n\n");
            System.Diagnostics.Debug.WriteLine(RunPython(pycode, "test"));

            // experimental code which runs python code that adds 3 to c# integer, then displays this number in graph
            int integer = 21;
            object output = RunPython(
@"integer=integer+3
result=integer
", integer, "integer", "result");
            WpfPlot1.Plot.Add.Text(output.ToString(), 0, 0);
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
                    scope.Set(parameterName, parameter.ToPython());
                    scope.Exec(pycode);
                    returnedVariable = scope.Get<object>(returnedVariableName);
                }
            }
            return returnedVariable;
        }

        // Method to open csv file and process to data grid
        private void CSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            // Change to open in specific folder later *****
            openFile.Filter = "Csv Files| *.csv";

            if(openFile.ShowDialog() == true) 
            {
                var csv = ReadCSVFile.GetCSVData(openFile.FileName);

                // print csv file to DataGrid
                CSVData.ItemsSource = csv;
            }
        }
    }
}
