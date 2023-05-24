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
using System.Data;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseProcessor
    {
        public string DateAdded = "";
        public string ProcessName = "";
        public string ProcessCode = "";
        public string Enabled = "";
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Language = "";
        public string Description = "";
        public string Origin = "";

        ~BaseProcessor()
        {
            DateAdded = null;
            ProcessName = null;
            ProcessCode = null;
            Enabled = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            Language = null;
            Description = null;
            Origin = null;
        }

        static public ArrayList loadProcessors(CollectionState State)
        {
            return loadProcessors(State.managementDB);
        }

        static public void removeProcessorsByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Processors] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public ArrayList loadProcessors(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Processors];";
            processors = managementDB.Execute(query);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseProcessor newProcessor = new BaseProcessor();
                newProcessor.DateAdded = row["DateAdded"].ToString();
                newProcessor.ProcessName = row["ProcessName"].ToString();
                newProcessor.ProcessCode = row["ProcessCode"].ToString();
                newProcessor.UniqueID = row["UniqueID"].ToString();
                newProcessor.Language = row["Language"].ToString();
                newProcessor.OwnerID = row["OwnerID"].ToString();
                newProcessor.GroupID = row["GroupID"].ToString();
                newProcessor.Description = row["description"].ToString();
                newProcessor.Origin = row["Origin"].ToString();

                newProcessor.Enabled = row["Enabled"].ToString();
                tmpProcessors.Add(newProcessor);
            }

            return tmpProcessors;
        }

        static public void updateProcessor(BaseProcessor rule, CollectionState State)
        {
            updateProcessor(State.managementDB, rule);
        }

        static public void updateProcessor(IntDatabase managementDB, BaseProcessor processor)
        {
            if (processor.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("ProcessName", processor.ProcessName);
                data.AddElement("ProcessCode", processor.ProcessCode);
                data.AddElement("Language", processor.Language);
                data.AddElement("OwnerID", processor.OwnerID);
                data.AddElement("GroupID", processor.GroupID);
                data.AddElement("Enabled", processor.Enabled);
                data.AddElement("Description", processor.Description);
                data.AddElement("Origin", processor.Origin);
                data.AddElement("*@UniqueID", processor.UniqueID);
                managementDB.UpdateTree("[Processors]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewProcessor = new Tree();
                NewProcessor.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewProcessor.AddElement("ProcessName", processor.ProcessName);
                NewProcessor.AddElement("ProcessCode", processor.ProcessCode);
                NewProcessor.AddElement("Language", processor.Language);
                NewProcessor.AddElement("Enabled", processor.Enabled);
                processor.UniqueID= "P" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewProcessor.AddElement("UniqueID", processor.UniqueID);
                NewProcessor.AddElement("OwnerID", processor.OwnerID);
                NewProcessor.AddElement("GroupID", processor.GroupID);
                NewProcessor.AddElement("Description", processor.Description);
                NewProcessor.AddElement("Origin", processor.Origin);
                managementDB.InsertTree("Processors", NewProcessor);
                NewProcessor.Dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Processors](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[ProcessName] TEXT NULL, " +
                    "[ProcessCode] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Language] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[Enabled] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Processors](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[ProcessName] NVARCHAR(100) NULL, " +
                    "[ProcessCode] TEXT NULL, " +
                    "[Language] NVARCHAR(20) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33), " +
                    "[Origin] VARCHAR(33), " +
                    "[Description] NVARCHAR(MAX), " +
                    "[Enabled] TEXT NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseprocessors ON Processors([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseprocessors ON Processors([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getID(string ProcessName, ArrayList ProcessList)
        {
            string result = "-1";

            for (int i = 0; i < ProcessList.Count; i++)
            {
                BaseProcessor current = (BaseProcessor)ProcessList[i];
                if (current.ProcessName == ProcessName)
                {
                    result = current.UniqueID;
                    i = ProcessList.Count;
                }
            }

            return result;
        }

        static public string getName(string ProcessID, ArrayList ProcessList)
        {
            string result = "-1";

            for (int i = 0; i < ProcessList.Count; i++)
            {
                BaseProcessor current = (BaseProcessor)ProcessList[i];
                if (current.UniqueID == ProcessID)
                {
                    result = current.ProcessName;
                    i = ProcessList.Count;
                }
            }

            return result;
        }

        static public BaseProcessor findProcessor(string ProcessID, ArrayList ProcessList)
        {
            BaseProcessor result = null;

            for (int i = 0; i < ProcessList.Count; i++)
            {
                BaseProcessor current = (BaseProcessor)ProcessList[i];
                if (current.UniqueID == ProcessID)
                {
                    result = current;
                    i = ProcessList.Count;
                }
            }

            return result;
        }

        public Tree toTree()
        {
            Tree result = new Tree();

            result.AddElement("DateAdded",DateAdded);
            result.AddElement("ProcessName",ProcessName);
            result.AddElement("Language", Language);
            result.AddElement("ProcessCode", FatumLib.ToSafeString(ProcessCode));
            result.AddElement("Enabled", Enabled.ToString());
            result.AddElement("UniqueID", UniqueID);
            result.AddElement("GroupID", GroupID);
            result.AddElement("OwnerID", OwnerID);
            result.AddElement("Description", Description);
            result.AddElement("Origin", Origin);
            return result;
        }

        public void fromTree(Tree settings, string NewOwner)
        {
            DateAdded = settings.GetElement("DateAdded");
            ProcessName = settings.GetElement("ProcessName");
            ProcessCode = FatumLib.FromSafeString(settings.GetElement("ProcessCode"));
            Language = settings.GetElement("Language");
            Origin = settings.GetElement("Origin");
            GroupID = settings.GetElement("GroupID");
            UniqueID = "P" + System.Guid.NewGuid().ToString().Replace("-", ""); 
            OwnerID = NewOwner;
            Description = settings.GetElement("Description");

            if (settings.GetElement("Enabled") =="true")
            {
                Enabled = "true";
            }
            else
            {
                Enabled = "false";
            }
        }

        static public string getXML(BaseProcessor current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("DateAdded", current.DateAdded.ToString());
            tmp.AddElement("ProcessName", current.ProcessName);
            tmp.AddElement("ProcessCode", current.ProcessCode);
            tmp.AddElement("Language", current.Language);
            tmp.AddElement("Enabled", current.Enabled);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("Description", current.Description);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseProcessor");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public DataTable getProcessorList(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Processors];";
            services = managementDB.Execute(squery);
            return services;
        }

        static public BaseProcessor getProcessorByUniqueID(IntDatabase managementDB, string uid)
        {
            DataTable processors;
            String query = "select * from [Processors] where UniqueID=@UniqueID;";
            Tree data = new Tree();
            data.AddElement("@UniqueID", uid);
            processors = managementDB.ExecuteDynamic(query,data);
            data.Dispose();

            if (processors.Rows.Count>0)
            {
                DataRow row = processors.Rows[0];
                BaseProcessor newProcessor = new BaseProcessor();
                newProcessor.DateAdded = row["DateAdded"].ToString();
                newProcessor.ProcessName = row["ProcessName"].ToString();
                newProcessor.ProcessCode = row["ProcessCode"].ToString();
                newProcessor.Language = row["Language"].ToString();
                newProcessor.UniqueID = row["UniqueID"].ToString();
                newProcessor.OwnerID = row["OwnerID"].ToString();
                newProcessor.GroupID = row["GroupID"].ToString();
                newProcessor.Origin = row["Origin"].ToString();
                newProcessor.Enabled = row["Enabled"].ToString();
                newProcessor.Description = row["Description"].ToString();

                return newProcessor;
            }
            else
            {
                return null;
            }
        }

        static public void deleteProcessor(IntDatabase managementDB, string uid)
        {
            Tree delete = new Tree();
            delete.AddElement("@UniqueID", uid);
            managementDB.DeleteTree("Processors",delete,"UniqueID=@UniqueID");
            delete.Dispose();
        }
    }
}
