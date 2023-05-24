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

using System.Collections;

namespace Proliferation.Flows.SearchCore
{
    public class SearchManager
    {
        fatumconfig fatumConfig = null;
        CollectionState State = null;
        public ArrayList Databases = new ArrayList();

        public SearchManager(fatumconfig FC, CollectionState S)
        {
            fatumConfig = FC;
            State = S;
        }

        private ArrayList DirSearch(DateTime cutoffDate, string sDir)
        {
            ArrayList notCollected = new ArrayList();

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    try {
                        DirectoryInfo dinfo = new DirectoryInfo(d);
                        try
                        {
                            DateTime dirDate = Convert.ToDateTime(dinfo.Name);
                            if (dirDate.CompareTo(cutoffDate) >= 0)
                            {
                                foreach (string f in Directory.GetFiles(d))
                                {
                                    try
                                    {
                                        FileInfo info = new FileInfo(f);

                                        if (info.Extension == ".s3db")
                                        {
                                            try
                                            {
                                                string flowid = info.Name.Substring(0, 33);
                                                Boolean load = false;

                                                foreach (BaseSource currentSource in State.Sources)
                                                {
                                                    foreach (BaseService currentService in currentSource.Services)
                                                    {
                                                        foreach (BaseFlow currentFlow in currentService.Flows)
                                                        {
                                                            if (string.Compare(currentFlow.UniqueID, flowid) == 0)
                                                            {
                                                                load = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (load)
                                                {
                                                    BaseFlowDB flowDB = new BaseFlowDB(info);
                                                    flowDB.day = dirDate.Day;
                                                    flowDB.month = dirDate.Month;
                                                    flowDB.year = dirDate.Year;
                                                    Databases.Add(flowDB);
                                                }
                                            }
                                            catch (Exception junkindirectory)
                                            {

                                            } 
                                        }
                                    }
                                    catch (Exception xyz)
                                    {
                                        System.Console.Out.WriteLine("Error loading databases: " + xyz.Message + ", " + xyz.StackTrace);
                                    }
                                }
                            }
                            else
                            {
                                notCollected.Add(d);
                            }
                        }
                        catch (Exception)
                        {
                            // Someone put a directory in here that isn't a timestamp, ignore it.
                        }
                    }
                    catch (Exception xyz)
                    {
                        System.Console.Out.WriteLine("Directory name is invalid for date conversion.");
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return notCollected;
        }

        public ArrayList loadDatabases(DateTime cutoffDate)
        {
            string directory = State.config.GetProperty("DocumentDatabaseDirectory");
            return DirSearch(cutoffDate, directory);
        }

        public void updateDatabases()
        {
            string directory = State.config.GetProperty("DocumentDatabaseDirectory");
            DirSearchUpdate(directory);
        }

        public void closeDatabases()
        {
            foreach (BaseFlowDB current in Databases)
            {
                current.Close();
            }
            Databases.Clear();
        }

        private void DirSearchUpdate(string sDir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    try
                    {
                        DirectoryInfo dinfo = new DirectoryInfo(d);
                        try
                        {
                            DateTime dirDate = Convert.ToDateTime(dinfo.Name);
                            foreach (string f in Directory.GetFiles(d))
                            {
                                try
                                {
                                    FileInfo info = new FileInfo(f);

                                    if (info.Extension == ".s3db")
                                    {
                                        try
                                        {
                                            string flowid = info.Name.Substring(0, 33);
                                            Boolean load = true;
                                            foreach (BaseFlowDB current in Databases)
                                            {
                                                if (string.Compare(current.DatabaseName, info.Name) == 0)
                                                {
                                                    if (string.Compare(current.DatabaseDirectory, info.DirectoryName) == 0)
                                                    {
                                                        load = false;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (load)
                                            {
                                                BaseFlowDB flowDB = new BaseFlowDB(info);
                                                flowDB.day = dirDate.Day;
                                                flowDB.month = dirDate.Month;
                                                flowDB.year = dirDate.Year;
                                                Databases.Add(flowDB);
                                            }
                                        }
                                        catch (Exception junkindirectory)
                                        {
                                            System.Console.Out.WriteLine("Error loading databases: " + junkindirectory.Message + ", " + junkindirectory.StackTrace);
                                        }

                                    }
                                }
                                catch (Exception xyz)
                                {
                                    System.Console.Out.WriteLine("Error loading databases: " + xyz.Message + ", " + xyz.StackTrace);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Someone put a directory in here that isn't a timestamp, ignore it.
                        }
                    }
                    catch (Exception xyz)
                    {
                        System.Console.Out.WriteLine("Directory name is invalid for date conversion.");
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
