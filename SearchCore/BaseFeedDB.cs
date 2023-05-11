using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DatabaseAdapters;

namespace PhlozLib.SearchCore
{
    public class BaseFlowDB
    {
        public string DatabaseDirectory = "";
        public string DatabaseName = "";
        public int year = 0;
        public int month = 0;
        public int day = 0;
        public string FlowName = "";
        public string FlowID = "";
        public string IndexDirectory = "";
        public SQLiteDatabase Database = null;
        
        public BaseFlowDB(FileInfo fi)
        {
            string datedir = fi.Directory.Name;
            DateTime dirdate = Convert.ToDateTime(datedir);
            year = dirdate.Year;
            month = dirdate.Month;
            day = dirdate.Day;
            FlowName = fi.Name;
            DatabaseDirectory = fi.DirectoryName;
            DatabaseName = fi.Name;
            FlowID = fi.Name.Substring(fi.Name.Length - 38, 33);
            IndexDirectory = fi.Directory.FullName + "\\" + FlowName.Replace(fi.Extension, "");
            try
            {
                Database = new SQLiteDatabase(DatabaseDirectory + "\\" + DatabaseName);
            }
            catch (Exception xyz)
            {
                // Must be the currently opened database
                Database = null;
            }
        }

        public void Close()
        {
            Database.Close();
        }
    }
}
