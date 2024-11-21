using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace Outlier_Detection_UI
{
    public class ReadCSVFile
    {
        public ReadCSVFile()
        {
        }

        // Reads in csv files rows and columns to populate datagrid
        public static DataView GetCSVData(string path)
        {
            DataTable dataTable = new DataTable();
            TextFieldParser parser = new TextFieldParser(path);

            parser.SetDelimiters(",");

            if(!parser.EndOfData)
            {
                var columns = parser.ReadFields();
                foreach (var col in columns) 
                {
                    dataTable.Columns.Add(col);
                }
            }

            while (!parser.EndOfData)
            {
                var row = parser.ReadFields();

                dataTable.Rows.Add(row);
            }

            // Change before done.
            return dataTable.DefaultView;
        }
    }
}

