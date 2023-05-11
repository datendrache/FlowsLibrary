//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using DatabaseAdapters;

namespace PhlozLib
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
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
                data.addElement("ProcessName", processor.ProcessName);
                data.addElement("ProcessCode", processor.ProcessCode);
                data.addElement("Language", processor.Language);
                data.addElement("OwnerID", processor.OwnerID);
                data.addElement("GroupID", processor.GroupID);
                data.addElement("Enabled", processor.Enabled);
                data.addElement("Description", processor.Description);
                data.addElement("Origin", processor.Origin);
                data.addElement("*@UniqueID", processor.UniqueID);
                managementDB.UpdateTree("[Processors]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewProcessor = new Tree();
                NewProcessor.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewProcessor.addElement("ProcessName", processor.ProcessName);
                NewProcessor.addElement("ProcessCode", processor.ProcessCode);
                NewProcessor.addElement("Language", processor.Language);
                NewProcessor.addElement("Enabled", processor.Enabled);
                processor.UniqueID= "P" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewProcessor.addElement("UniqueID", processor.UniqueID);
                NewProcessor.addElement("OwnerID", processor.OwnerID);
                NewProcessor.addElement("GroupID", processor.GroupID);
                NewProcessor.addElement("Description", processor.Description);
                NewProcessor.addElement("Origin", processor.Origin);
                managementDB.InsertTree("Processors", NewProcessor);
                NewProcessor.dispose();
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

            result.addElement("DateAdded",DateAdded);
            result.addElement("ProcessName",ProcessName);
            result.addElement("Language", Language);
            result.addElement("ProcessCode", FatumLib.toSafeString(ProcessCode));
            result.addElement("Enabled", Enabled.ToString());
            result.addElement("UniqueID", UniqueID);
            result.addElement("GroupID", GroupID);
            result.addElement("OwnerID", OwnerID);
            result.addElement("Description", Description);
            result.addElement("Origin", Origin);
            return result;
        }

        public void fromTree(Tree settings, string NewOwner)
        {
            DateAdded = settings.getElement("DateAdded");
            ProcessName = settings.getElement("ProcessName");
            ProcessCode = FatumLib.fromSafeString(settings.getElement("ProcessCode"));
            Language = settings.getElement("Language");
            Origin = settings.getElement("Origin");
            GroupID = settings.getElement("GroupID");
            UniqueID = "P" + System.Guid.NewGuid().ToString().Replace("-", ""); 
            OwnerID = NewOwner;
            Description = settings.getElement("Description");

            if (settings.getElement("Enabled") =="true")
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

            tmp.addElement("DateAdded", current.DateAdded.ToString());
            tmp.addElement("ProcessName", current.ProcessName);
            tmp.addElement("ProcessCode", current.ProcessCode);
            tmp.addElement("Language", current.Language);
            tmp.addElement("Enabled", current.Enabled);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("Description", current.Description);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseProcessor");
            tmp.dispose();
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
            data.addElement("@UniqueID", uid);
            processors = managementDB.ExecuteDynamic(query,data);
            data.dispose();

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
            delete.addElement("@UniqueID", uid);
            managementDB.DeleteTree("Processors",delete,"UniqueID=@UniqueID");
            delete.dispose();
        }
    }
}
