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

        public static DataView GetCSVData(string path)
        {
            DataTable dataTable = new DataTable();
            TextFieldParser parser = new TextFieldParser(path);

            parser.SetDelimiters(",");

            if(!parser.EndOfData)
            {

            }


            // Change before done.
            return null;
        }
    }
}

