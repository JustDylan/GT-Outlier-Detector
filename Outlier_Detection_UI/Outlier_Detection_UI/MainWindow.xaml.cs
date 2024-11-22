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
            string pythonDll = System.IO.Path.GetFullPath("../../../../python_interpreter/python311.dll");
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            Runtime.PythonDLL = pythonDll;


            // experimental code which runs python code that adds 3 to c# integer, then displays this number in graph
            int integer = 21;
            object output = RunPython(
@"integer=integer+3;
result=integer
", integer, "integer", "result");
            WpfPlot1.Plot.Add.Text(output.ToString(), 0, 0);

            //PythonEngine.RunSimpleString(@"print(""hello world from python!"")");
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
