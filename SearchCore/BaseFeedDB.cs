//   Flows Libraries -- Flows Common Classes and Methods
//
//   Copyright (C) 2003-2023 Eric Knight
//   This software is distributed under the GNU Public v3 License
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DatabaseAdapters;

namespace Proliferation.Flows.SearchCore
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
