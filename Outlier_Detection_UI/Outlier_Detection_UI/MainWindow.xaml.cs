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
using System.Globalization;

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
          
            // temporary path to python dll just to get things working
            string pythonDll = System.IO.Path.GetFullPath("../../../../python_interpreter/python311.dll");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            Runtime.PythonDLL = pythonDll;
        }

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


        // UNDER CONSTRUCTION
        private void Run_Click(object sender, RoutedEventArgs e)
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

            // Change later to have user select model or python script they want to use
            string pyPath = System.IO.Path.GetFullPath("../../../../python_scripts/Test_For_Csharp");
            string pyCode = System.IO.File.ReadAllText(pyPath);
            object output;
            try
            {

                output = RunPython(pyCode, null, "", "testInteger");

                // Convert Python object to string
                string result = output.ToString();

                //
                string[] theNumbers = result.Trim('[', ']').Split(',');

                int[] intArray = theNumbers.Select(n => int.Parse(n)).ToArray();
                int index = 1;
                foreach (int var in intArray)
                {
                    WpfPlot1.Plot.Add.Scatter(index, var);
                    index++;
                }

                WpfPlot1.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: RunPython failure.");
                Console.WriteLine("Exception Message: " + ex.Message);
            }

           
        }
    }
}