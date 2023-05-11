using System;
using System.Collections.Generic;
using FatumCore;
using System.Collections;
using System.Data;
using System.IO;
using DatabaseAdapters;
using PhlozLanguages;
using System.ServiceModel.Configuration;
using Lucene.Net.Search;
using System.Threading;
using PhlozLib.SearchCore;
using Fatum.FatumCore;

namespace PhlozLib
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
                query.dispose();
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
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updateTask(IntDatabase managementDB, BaseTask task)
        {
            if (task.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Name", task.Name.ToString());
                if (task.Occurence!=null)
                {
                    data.addElement("Occurence", task.Occurence.Ticks.ToString());
                }
                else
                {
                    data.addElement("Occurence", "0");
                }
                
                data.addElement("ProcessorID", task.ProcessorID.ToString());
                data.addElement("Sunday", task.Sunday.ToString());
                data.addElement("Monday", task.Monday.ToString());
                data.addElement("Tuesday", task.Tuesday.ToString());
                data.addElement("Wednesday", task.Wednesday.ToString());
                data.addElement("Thursday", task.Thursday.ToString());
                data.addElement("Friday", task.Friday.ToString());
                data.addElement("Saturday", task.Saturday.ToString());
                data.addElement("EndOfMonth", task.EndOfMonth.ToString());
                data.addElement("Enabled", task.EndOfMonth.ToString());
                data.addElement("OwnerID", task.OwnerID);
                data.addElement("GroupID", task.GroupID);
                data.addElement("Origin", task.Origin);
                data.addElement("Description", task.Description);
                data.addElement("Hour", task.hour.ToString());
                data.addElement("Minute", task.minute.ToString());
                data.addElement("InstanceID", task.InstanceID);
                data.addElement("_Hour", "integer");
                data.addElement("_Minute", "integer");
                if (task.query == null)
                {
                    data.addElement("Query", TreeDataAccess.writeTreeToXMLString(new Tree(), "Query"));
                }
                else
                {
                    data.addElement("Query", TreeDataAccess.writeTreeToXMLString(task.query, "Query"));
                }
                data.addElement("*@UniqueID", task.UniqueID);
                managementDB.UpdateTree("[Tasks]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewTask = new Tree();
                NewTask.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewTask.addElement("Name", task.Name);
                NewTask.addElement("Occurence", task.Occurence.Ticks.ToString());
                NewTask.addElement("ProcessorID", task.ProcessorID.ToString());
                NewTask.addElement("Sunday", task.Sunday.ToString());
                NewTask.addElement("Monday", task.Monday.ToString());
                NewTask.addElement("Tuesday", task.Tuesday.ToString());
                NewTask.addElement("Wednesday", task.Wednesday.ToString());
                NewTask.addElement("Thursday", task.Thursday.ToString());
                NewTask.addElement("Friday", task.Friday.ToString());
                NewTask.addElement("Saturday", task.Saturday.ToString());
                NewTask.addElement("EndOfMonth", task.EndOfMonth.ToString());
                NewTask.addElement("Enabled", task.Enabled.ToString());
                task.UniqueID = "T" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewTask.addElement("UniqueID", task.UniqueID);
                NewTask.addElement("OwnerID", task.OwnerID);
                NewTask.addElement("GroupID", task.GroupID);
                NewTask.addElement("Origin", task.Origin);
                NewTask.addElement("InstanceID", task.InstanceID);
                NewTask.addElement("Hour", task.hour.ToString());
                NewTask.addElement("Minute", task.minute.ToString());
                NewTask.addElement("_Hour", "integer");
                NewTask.addElement("_Minute", "integer");
                NewTask.addElement("Description", task.Description);
                if (task.query==null)
                {
                    NewTask.addElement("Query", TreeDataAccess.writeTreeToXMLString(new Tree(), "Query"));
                }
                else
                {
                    NewTask.addElement("Query", TreeDataAccess.writeTreeToXMLString(task.query, "Query"));
                }
                
                managementDB.InsertTree("[Tasks]", NewTask);

                NewTask.dispose();
            }
        }

        static public ArrayList loadTasksForInstance(IntDatabase managementDB, string instanceid)
        {
            DataTable processors;
            String query = "select * from [Tasks] where InstanceID=@instanceid or InstanceID='All';";
            Tree data = new Tree();
            data.addElement("@instanceid", instanceid);
            processors = managementDB.ExecuteDynamic(query, data);
            data.dispose();

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
                    newTask.query = XMLTree.readXMLFromString(QueryText);
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

            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("_DateAdded", "BIGINT");
            tmp.addElement("Name", current.Name);
            tmp.addElement("ForwarderID", current.ProcessorID);
            tmp.addElement("Occurence", current.Occurence.Ticks.ToString());
            tmp.addElement("Sunday", current.Sunday.ToString());
            tmp.addElement("Monday", current.Monday.ToString());
            tmp.addElement("Tuesday", current.Tuesday.ToString());
            tmp.addElement("Wednesday", current.Wednesday.ToString());
            tmp.addElement("Thursday", current.Thursday.ToString());
            tmp.addElement("Friday", current.Friday.ToString());
            tmp.addElement("Saturday", current.Saturday.ToString());
            tmp.addElement("EndOfMonth", current.EndOfMonth.ToString());
            tmp.addElement("Enabled", current.Enabled.ToString());
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("Hour", current.hour.ToString());
            tmp.addElement("Minute", current.minute.ToString());
            tmp.addElement("InstanceID", current.InstanceID);
            tmp.addElement("Description", current.Description);
            tmp.addNode(current.query.Duplicate(), "Query");
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseTask");
            tmp.dispose();
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
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
                    newTask.query = XMLTree.readXMLFromString(QueryText);
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
            switch (Query.getElement("Lookback").ToLower())
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

            Query.addElement("StartTime", StartTime.ToString());
            Query.addElement("EndTime", EndTime.ToString());

            SearchRequest Request = new SearchRequest();
            Request.Query = Query;
            BaseQueryHost.performQuery(State.searchSystem, Request, int.MaxValue);
            BaseSearch.redux((Tree)Request.Result.tree[0], int.MaxValue);

            Query.dispose();
            return allDocuments;
        }
    }
}
