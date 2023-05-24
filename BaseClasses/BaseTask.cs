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

using Proliferation.Fatum;
using System.Collections;
using System.Data;
using DatabaseAdapters;
using Proliferation.LanguageAdapters;
using Proliferation.Flows.SearchCore;

namespace Proliferation.Flows
{
    public class BaseTask
    {
        public string DateAdded = "";
        public string Name = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string OwnerID = "";
        public DateTime Occurence;
        public string ProcessorID = "";
        public Boolean Sunday = false;
        public Boolean Monday = false;
        public Boolean Tuesday = false;
        public Boolean Wednesday = false;
        public Boolean Thursday = false;
        public Boolean Friday = false;
        public Boolean Saturday = false;
        public Boolean EndOfMonth = false;
        public Boolean Enabled = false;
        public string Description = "";
        public string Origin = "";
        public string InstanceID = "";
        public int hour = 0;
        public int minute = 0;
        public IntLanguage runtime = null;
        public Tree query = null;

        public DateTime lastrun = new DateTime(0);
        public ArrayList forwarderLinks = null;

        ~BaseTask()
        {
            DateAdded = null;
            Name = null;
            UniqueID = null;
            GroupID = null;
            OwnerID = null;
            ProcessorID = null;
            Origin = null;
            Description = null;
            if (runtime!=null)
            {
                runtime.dispose();
                runtime = null;
            }
            if (query!=null)
            {
                query.Dispose();
                query = null;
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Tasks](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[Occurence] INTEGER NULL, " +
                    "[ProcessorID] TEXT NULL, " +
                    "[Sunday] TEXT NULL, " +
                    "[Monday] TEXT NULL, " +
                    "[Tuesday] TEXT NULL, " +
                    "[Wednesday] TEXT NULL, " +
                    "[Thursday] TEXT NULL, " +
                    "[Friday] TEXT NULL, " +
                    "[Saturday] TEXT NULL, " +
                    "[EndOfMonth] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[InstanceID] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[Hour] INTEGER NULL, " +
                    "[Minute] INTEGER NULL, " +
                    "[Query] TEXT NULL, " +
                    "[Enabled] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Tasks](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Name] NVARCHAR(100) NULL, " +
                    "[Occurence] VARCHAR(22) NULL, " +
                    "[ProcessorID] VARCHAR(33) NULL, " +
                    "[Sunday] VARCHAR(10) NULL, " +
                    "[Monday] VARCHAR(10) NULL, " +
                    "[Tuesday] VARCHAR(10) NULL, " +
                    "[Wednesday] VARCHAR(10) NULL, " +
                    "[Thursday] VARCHAR(10) NULL, " +
                    "[Friday] VARCHAR(10) NULL, " +
                    "[Saturday] VARCHAR(10) NULL, " +
                    "[EndOfMonth] VARCHAR(10) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[InstanceID] VARCHAR(33) NULL, " +
                    "[Hour] INTEGER NULL, " +
                    "[Minute] INTEGER NULL, " +
                    "[Description] NVARCHAR(512) NULL, " +
                    "[Query] TEXT NULL, " +
                    "[Enabled] VARCHAR(10) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basetasks ON Tasks([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basetasks ON Tasks([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void removeTaskByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Tasks] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void updateTask(IntDatabase managementDB, BaseTask task)
        {
            if (task.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Name", task.Name.ToString());
                if (task.Occurence!=null)
                {
                    data.AddElement("Occurence", task.Occurence.Ticks.ToString());
                }
                else
                {
                    data.AddElement("Occurence", "0");
                }
                
                data.AddElement("ProcessorID", task.ProcessorID.ToString());
                data.AddElement("Sunday", task.Sunday.ToString());
                data.AddElement("Monday", task.Monday.ToString());
                data.AddElement("Tuesday", task.Tuesday.ToString());
                data.AddElement("Wednesday", task.Wednesday.ToString());
                data.AddElement("Thursday", task.Thursday.ToString());
                data.AddElement("Friday", task.Friday.ToString());
                data.AddElement("Saturday", task.Saturday.ToString());
                data.AddElement("EndOfMonth", task.EndOfMonth.ToString());
                data.AddElement("Enabled", task.EndOfMonth.ToString());
                data.AddElement("OwnerID", task.OwnerID);
                data.AddElement("GroupID", task.GroupID);
                data.AddElement("Origin", task.Origin);
                data.AddElement("Description", task.Description);
                data.AddElement("Hour", task.hour.ToString());
                data.AddElement("Minute", task.minute.ToString());
                data.AddElement("InstanceID", task.InstanceID);
                data.AddElement("_Hour", "integer");
                data.AddElement("_Minute", "integer");
                if (task.query == null)
                {
                    data.AddElement("Query", TreeDataAccess.WriteTreeToXmlString(new Tree(), "Query"));
                }
                else
                {
                    data.AddElement("Query", TreeDataAccess.WriteTreeToXmlString(task.query, "Query"));
                }
                data.AddElement("*@UniqueID", task.UniqueID);
                managementDB.UpdateTree("[Tasks]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewTask = new Tree();
                NewTask.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewTask.AddElement("Name", task.Name);
                NewTask.AddElement("Occurence", task.Occurence.Ticks.ToString());
                NewTask.AddElement("ProcessorID", task.ProcessorID.ToString());
                NewTask.AddElement("Sunday", task.Sunday.ToString());
                NewTask.AddElement("Monday", task.Monday.ToString());
                NewTask.AddElement("Tuesday", task.Tuesday.ToString());
                NewTask.AddElement("Wednesday", task.Wednesday.ToString());
                NewTask.AddElement("Thursday", task.Thursday.ToString());
                NewTask.AddElement("Friday", task.Friday.ToString());
                NewTask.AddElement("Saturday", task.Saturday.ToString());
                NewTask.AddElement("EndOfMonth", task.EndOfMonth.ToString());
                NewTask.AddElement("Enabled", task.Enabled.ToString());
                task.UniqueID = "T" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewTask.AddElement("UniqueID", task.UniqueID);
                NewTask.AddElement("OwnerID", task.OwnerID);
                NewTask.AddElement("GroupID", task.GroupID);
                NewTask.AddElement("Origin", task.Origin);
                NewTask.AddElement("InstanceID", task.InstanceID);
                NewTask.AddElement("Hour", task.hour.ToString());
                NewTask.AddElement("Minute", task.minute.ToString());
                NewTask.AddElement("_Hour", "integer");
                NewTask.AddElement("_Minute", "integer");
                NewTask.AddElement("Description", task.Description);
                if (task.query==null)
                {
                    NewTask.AddElement("Query", TreeDataAccess.WriteTreeToXmlString(new Tree(), "Query"));
                }
                else
                {
                    NewTask.AddElement("Query", TreeDataAccess.WriteTreeToXmlString(task.query, "Query"));
                }
                
                managementDB.InsertTree("[Tasks]", NewTask);

                NewTask.Dispose();
            }
        }

        static public ArrayList loadTasksForInstance(IntDatabase managementDB, string instanceid)
        {
            DataTable processors;
            String query = "select * from [Tasks] where InstanceID=@instanceid or InstanceID='All';";
            Tree data = new Tree();
            data.AddElement("@instanceid", instanceid);
            processors = managementDB.ExecuteDynamic(query, data);
            data.Dispose();

            ArrayList tmpTasks = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseTask newTask = new BaseTask();
                newTask.DateAdded = row["DateAdded"].ToString();
                newTask.Name = row["Name"].ToString();
                newTask.ProcessorID = row["ProcessorID"].ToString();
                newTask.OwnerID = row["OwnerID"].ToString();
                newTask.UniqueID = row["UniqueID"].ToString();
                newTask.GroupID = row["GroupID"].ToString();
                newTask.Origin = row["Origin"].ToString();
                newTask.Description = row["Description"].ToString();
                newTask.InstanceID = row["InstanceID"].ToString();
                newTask.forwarderLinks = ForwarderLink.loadLinksByFlowID(managementDB, newTask.UniqueID);
                long ticks = 0;
                long.TryParse(row["Occurence"].ToString(), out ticks);
                newTask.Occurence = new DateTime(ticks);

                if (row["Sunday"].ToString().ToLower() == "true")
                {
                    newTask.Sunday = true;
                }
                else
                {
                    newTask.Sunday = false;
                }

                if (row["Monday"].ToString().ToLower() == "true")
                {
                    newTask.Monday = true;
                }
                else
                {
                    newTask.Monday = false;
                }

                if (row["Tuesday"].ToString().ToLower() == "true")
                {
                    newTask.Tuesday = true;
                }
                else
                {
                    newTask.Tuesday = false;
                }

                if (row["Wednesday"].ToString().ToLower() == "true")
                {
                    newTask.Wednesday = true;
                }
                else
                {
                    newTask.Wednesday = false;
                }

                if (row["Thursday"].ToString().ToLower() == "true")
                {
                    newTask.Thursday = true;
                }
                else
                {
                    newTask.Thursday = false;
                }

                if (row["Friday"].ToString().ToLower() == "true")
                {
                    newTask.Friday = true;
                }
                else
                {
                    newTask.Friday = false;
                }

                if (row["Saturday"].ToString().ToLower() == "true")
                {
                    newTask.Saturday = true;
                }
                else
                {
                    newTask.Saturday = false;
                }

                if (row["EndOfMonth"].ToString().ToLower() == "true")
                {
                    newTask.EndOfMonth = true;
                }
                else
                {
                    newTask.EndOfMonth = false;
                }

                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newTask.Enabled = true;
                }
                else
                {
                    newTask.Enabled = false;
                }

                newTask.hour = Convert.ToInt32(row["Hour"]);
                newTask.minute = Convert.ToInt32(row["Minute"]);

                string QueryText = row["Query"].ToString();
                if (QueryText == "")
                {
                    newTask.query = new Tree();

                }
                else
                {
                    newTask.query = XMLTree.ReadXmlFromString(QueryText);
                }
                newTask.lastrun = DateTime.Now;
                tmpTasks.Add(newTask);
            }
            return tmpTasks;
        }

        public static BaseTask locateTask(ArrayList taskList, string ID)
        {
            BaseTask result = null;
            foreach (BaseTask current in taskList)
            {
                if (ID == current.UniqueID)
                {
                    result = current;
                }
            }
            return result;
        }

        static public string getXML(BaseTask current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("_DateAdded", "BIGINT");
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("ForwarderID", current.ProcessorID);
            tmp.AddElement("Occurence", current.Occurence.Ticks.ToString());
            tmp.AddElement("Sunday", current.Sunday.ToString());
            tmp.AddElement("Monday", current.Monday.ToString());
            tmp.AddElement("Tuesday", current.Tuesday.ToString());
            tmp.AddElement("Wednesday", current.Wednesday.ToString());
            tmp.AddElement("Thursday", current.Thursday.ToString());
            tmp.AddElement("Friday", current.Friday.ToString());
            tmp.AddElement("Saturday", current.Saturday.ToString());
            tmp.AddElement("EndOfMonth", current.EndOfMonth.ToString());
            tmp.AddElement("Enabled", current.Enabled.ToString());
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("Hour", current.hour.ToString());
            tmp.AddElement("Minute", current.minute.ToString());
            tmp.AddElement("InstanceID", current.InstanceID);
            tmp.AddElement("Description", current.Description);
            tmp.AddNode(current.query.Duplicate(), "Query");
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseTask");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public DataTable getTasks(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Tasks];";
            return managementDB.Execute(query);
        }

        static public Boolean executeReady(BaseTask task)
        {
            Boolean result = false;
            
            if (task.Occurence.Ticks > 0)
            {
                if ((DateTime.Now.Ticks - task.lastrun.Ticks) > task.Occurence.Ticks)
                {
                    result = true;
                }
            }

            Boolean isPossiblyTriggeredToday = false;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    if (task.Monday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
                case DayOfWeek.Tuesday:
                    if (task.Tuesday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
                case DayOfWeek.Wednesday:
                    if (task.Wednesday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
                case DayOfWeek.Thursday:
                    if (task.Thursday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
                case DayOfWeek.Friday:
                    if (task.Friday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
                case DayOfWeek.Saturday:
                    if (task.Saturday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
                case DayOfWeek.Sunday:
                    if (task.Sunday)
                    {
                        isPossiblyTriggeredToday = true;
                    }
                    break;
            }

            if (isPossiblyTriggeredToday)
            {
                if (task.hour == DateTime.Now.Hour)
                {
                    if (task.minute == DateTime.Now.Minute)
                    {
                        if (task.lastrun.Hour != DateTime.Now.Hour || task.lastrun.Minute != DateTime.Now.Minute)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        static public BaseTask loadTaskByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseTask result = null;

            String query = "select * from [Tasks] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseTask newTask = new BaseTask();
                newTask.Name = row["Name"].ToString();
                newTask.DateAdded = row["DateAdded"].ToString();
                newTask.OwnerID = row["OwnerID"].ToString();
                newTask.UniqueID = row["UniqueID"].ToString();
                newTask.Description = row["Description"].ToString();
                newTask.OwnerID = row["OwnerID"].ToString();
                newTask.GroupID = row["GroupID"].ToString();
                newTask.ProcessorID = row["ProcessorID"].ToString();
                newTask.Origin = row["Origin"].ToString();
                if (row["Monday"].ToString().ToLower()=="true")
                {
                    newTask.Monday = true;
                }
                else
                {
                    newTask.Monday = false;
                }
                if (row["Tuesday"].ToString().ToLower() == "true")
                {
                    newTask.Tuesday = true;
                }
                else
                {
                    newTask.Tuesday= false;
                }
                if (row["Wednesday"].ToString().ToLower() == "true")
                {
                    newTask.Wednesday = true;
                }
                else
                {
                    newTask.Wednesday = false;
                }
                if (row["Thursday"].ToString().ToLower() == "true")
                {
                    newTask.Thursday = true;
                }
                else
                {
                    newTask.Thursday = false;
                }
                if (row["Friday"].ToString().ToLower() == "true")
                {
                    newTask.Friday = true;
                }
                else
                {
                    newTask.Friday = false;
                }
                if (row["Saturday"].ToString().ToLower() == "true")
                {
                    newTask.Saturday = true;
                }
                else
                {
                    newTask.Saturday = false;
                }
                if (row["Sunday"].ToString().ToLower() == "true")
                {
                    newTask.Sunday = true;
                }
                else
                {
                    newTask.Sunday = false;
                }
                if (row["EndOfMonth"].ToString().ToLower() == "true")
                {
                    newTask.EndOfMonth = true;
                }
                else
                {
                    newTask.EndOfMonth = false;
                }
                newTask.Occurence = new DateTime(Convert.ToInt64(row["Occurence"]));
                newTask.hour = Convert.ToInt32(row["hour"]);
                newTask.minute = Convert.ToInt32(row["minute"]);

                string QueryText = row["Query"].ToString();
                if (QueryText == "")
                {
                    newTask.query = new Tree();
                }
                else
                {
                    newTask.query = XMLTree.ReadXmlFromString(QueryText);
                }

                newTask.forwarderLinks = ForwarderLink.loadLinksByFlowID(managementDB, uniqueid);
                result = newTask;
            }
            return result;
        }

        public static Tree performTaskQuery(CollectionState State, BaseTask task)
        {
            BaseSearch Search = new BaseSearch();
            Tree allDocuments = new Tree();

            Tree Query = task.query.Duplicate();
            long StartTime = 0;
            long EndTime = DateTime.Now.Ticks;
            switch (Query.GetElement("Lookback").ToLower())
            {
                case "past minute":
                    StartTime = DateTime.Now.AddMinutes(-1).Ticks;
                    break;
                case "past 2 minutes":
                    StartTime = DateTime.Now.AddMinutes(-2).Ticks;
                    break;
                case "past 5 minutes":
                    StartTime = DateTime.Now.AddMinutes(-5).Ticks;
                    break;
                case "past 10 minutes":
                    StartTime = DateTime.Now.AddMinutes(-10).Ticks;
                    break;
                case "past 30 minutes":
                    StartTime = DateTime.Now.AddMinutes(-30).Ticks;
                    break;
                case "past hour":
                    StartTime = DateTime.Now.AddHours(-1).Ticks;
                    break;
                case "past 4 hours":
                    StartTime = DateTime.Now.AddHours(-4).Ticks;
                    break;
                case "past day":
                    StartTime = DateTime.Now.AddDays(-1).Ticks;
                    break;
                case "past week":
                    StartTime = DateTime.Now.AddDays(-7).Ticks;
                    break;
                case "all available":
                    StartTime = 0;
                    break;
            }

            Query.AddElement("StartTime", StartTime.ToString());
            Query.AddElement("EndTime", EndTime.ToString());

            SearchRequest Request = new SearchRequest();
            Request.Query = Query;
            BaseQueryHost.performQuery(State.searchSystem, Request, int.MaxValue);
            BaseSearch.redux((Tree)Request.Result.tree[0], int.MaxValue);

            Query.Dispose();
            return allDocuments;
        }
    }
}
