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
    public class BaseForwarder
    {
        public string DateAdded = "";
        public string forwarderName = "";
        public string forwarderType = "";
        public string OwnerID = "";
        public string UniqueID = "";
        public string CredentialID = "";
        public string ParameterID = "";
        public string ControlState = "";
        public string GroupID = "";
        public string Enabled = "";
        public string Description = "";
        public string Origin = "";

        public Boolean FailedToInit = false;
        public Object ForwarderState = null;
        public BaseParameter Parameters = null;
        public BaseCredential Credential = null;

        ~BaseForwarder()
        {
            DateAdded = null;
            forwarderName = null;
            forwarderType = null;
            OwnerID = null;
            UniqueID = null;
            CredentialID = null;
            ParameterID = null;
            GroupID = null;
            Enabled = null;
            Description = null;
            ForwarderState = null;
            Parameters = null;
            Credential = null;
            ControlState = null;
            Origin = null;
        }

        static public void removeForwarderByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Forwarders] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Forwarders](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[ForwarderName] TEXT NULL, " +
                    "[ForwarderType] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Enabled] TEXT NULL, " +
                    "[CredentialID] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[ControlState] TEXT NULL, " +
                    "[Description] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Forwarders](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[ForwarderName] NVARCHAR(100) NULL, " +
                    "[ForwarderType] VARCHAR(100) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Enabled] VARCHAR(10) NULL, " +
                    "[CredentialID] VARCHAR(33) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[ControlState] VARCHAR(20) NULL, " +
                    "[Description] NVARCHAR(512) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseforwarders ON Forwarders([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseforwarders ON Forwarders([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void updateForwarder(IntDatabase managementDB, BaseForwarder forwarder)
        {
            if (forwarder.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("ForwarderName", forwarder.forwarderName.ToString());
                data.addElement("ForwarderType", forwarder.forwarderType.ToString());
                data.addElement("OwnerID", forwarder.OwnerID);
                data.addElement("GroupID", forwarder.GroupID);
                data.addElement("CredentialID", forwarder.CredentialID);
                data.addElement("ParameterID", forwarder.ParameterID);
                data.addElement("Enabled", forwarder.Enabled);
                data.addElement("Origin", forwarder.Origin);
                data.addElement("Description", forwarder.Description);
                data.addElement("ControlState", forwarder.ControlState);
                data.addElement("*@UniqueID", forwarder.UniqueID);
                managementDB.UpdateTree("[Forwarders]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Forwarders] ([DateAdded], [UniqueID], [ForwarderName], [ForwarderType], [CredentialID], [ParameterID], [OwnerID], [GroupID], [Enabled], [Description], [Origin], [ControlState]) VALUES " +
                    "(@DateAdded, @UniqueID, @ForwarderName, @ForwarderType, @CredentialID, @ParameterID, @OwnerID, @GroupID, @Enabled, @Description, @Origin, @ControlState);";

                Tree NewFilter = new Tree();
                NewFilter.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewFilter.addElement("@ForwarderName", forwarder.forwarderName);
                NewFilter.addElement("@ForwarderType", forwarder.forwarderType);
                forwarder.UniqueID = "D" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewFilter.addElement("@UniqueID", forwarder.UniqueID);
                NewFilter.addElement("@OwnerID", forwarder.OwnerID);
                NewFilter.addElement("@GroupID", forwarder.GroupID);
                NewFilter.addElement("@CredentialID", forwarder.CredentialID);
                NewFilter.addElement("@ParameterID", forwarder.ParameterID);
                NewFilter.addElement("@Enabled", forwarder.Enabled);
                NewFilter.addElement("@Origin", forwarder.Origin);
                NewFilter.addElement("@ControlState", forwarder.ControlState);
                NewFilter.addElement("@Description", forwarder.Description);
                managementDB.ExecuteDynamic(sql, NewFilter);
                NewFilter.dispose();
            }
        }

        static public void addForwarder(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [Forwarders] ([DateAdded], [UniqueID], [ForwarderName], [ForwarderType], [CredentialID], [ParameterID], [OwnerID], [GroupID], [Enabled], [Description], [Origin], [ControlState]) VALUES " +
                "(@DateAdded, @UniqueID, @ForwarderName, @ForwarderType, @CredentialID, @ParameterID, @OwnerID, @GroupID, @Enabled, @Description, @Origin, @ControlState);";

            Tree NewFilter = new Tree();
            NewFilter.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewFilter.addElement("@ForwarderName", description.getElement("ForwarderName"));
            NewFilter.addElement("@ForwarderType", description.getElement("ForwarderType"));
            NewFilter.addElement("@UniqueID", description.getElement("UniqueID"));
            NewFilter.addElement("@OwnerID", description.getElement("OwnerID"));
            NewFilter.addElement("@GroupID", description.getElement("GroupID"));
            NewFilter.addElement("@CredentialID", description.getElement("CredentialID"));
            NewFilter.addElement("@ParameterID", description.getElement("ParameterID"));
            NewFilter.addElement("@Enabled", description.getElement("Enabled"));
            NewFilter.addElement("@Origin", description.getElement("Origin"));
            NewFilter.addElement("@ControlName", description.getElement("ControlName"));
            NewFilter.addElement("@Description", description.getElement("Description"));
            managementDB.ExecuteDynamic(sql, NewFilter);
            NewFilter.dispose();
        }

        static public ArrayList loadForwarders(CollectionState State)
        {
            return loadForwarders(State.managementDB);
        }

        static public ArrayList loadForwarders(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Forwarders];";
            processors = managementDB.Execute(query);

            ArrayList tmpForwarders = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseForwarder newForwarder = new BaseForwarder();
                newForwarder.DateAdded = row["DateAdded"].ToString();
                newForwarder.forwarderName = row["ForwarderName"].ToString();
                newForwarder.forwarderType = row["ForwarderType"].ToString();
                newForwarder.UniqueID = row["UniqueID"].ToString();
                newForwarder.CredentialID = row["CredentialID"].ToString();
                newForwarder.ParameterID = row["ParameterID"].ToString();
                newForwarder.OwnerID = row["OwnerID"].ToString();
                newForwarder.GroupID = row["GroupID"].ToString();
                newForwarder.Origin = row["Origin"].ToString();
                newForwarder.ControlState = row["ControlState"].ToString();
                newForwarder.Description = row["Description"].ToString();

                if (newForwarder.CredentialID!="")
                {
                    newForwarder.Credential = BaseCredential.loadCredentialByUniqueID(managementDB, newForwarder.CredentialID);
                }

                if (newForwarder.ParameterID!="")
                {
                    newForwarder.Parameters = BaseParameter.loadParameterByUniqueID(managementDB, newForwarder.ParameterID);
                }

                tmpForwarders.Add(newForwarder);
            }
            return tmpForwarders;
        }

        public static BaseForwarder locateForwarder(ArrayList forwarders, string ID)
        {
            BaseForwarder result = null;
            foreach (BaseForwarder current in forwarders)
            {
                if (ID == current.UniqueID)
                {
                    result = current;
                }
            }
            return result;
        }

        static public string getID(string ForwarderName, ArrayList ForwarderList)
        {
            string result = "-1";

            for (int i = 0; i < ForwarderList.Count; i++)
            {
                BaseForwarder current = (BaseForwarder)ForwarderList[i];
                if (current.forwarderName == ForwarderName)
                {
                    result = current.UniqueID;
                    i = ForwarderList.Count;
                }
            }
            return result;
        }

        static public string getXML(BaseForwarder current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseForwarder");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseForwarder current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("forwarderName", current.forwarderName);
            tmp.addElement("forwarderType", current.forwarderType);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("CredentialID", current.CredentialID);
            tmp.addElement("ParameterID", current.ParameterID);
            tmp.addElement("Enabled", current.Enabled);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("ControlState", current.ControlState);
            tmp.addElement("Description", current.Description);
            return tmp;
        }

        public static DataTable getForwarderList(IntDatabase managementDB)
        {
            string SQL = "select * from [Forwarders]";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static BaseForwarder loadForwarderByUniqueID(IntDatabase managementDB, string forwarderid)
        {
            DataTable forwarders;
            String query = "select * from [Forwarders] where UniqueID=@UniqueID;";
            Tree data = new Tree();
            data.addElement("@UniqueID", forwarderid);
            forwarders = managementDB.ExecuteDynamic(query,data);
            data.dispose();

            if (forwarders.Rows.Count>0)
            {
                DataRow row = forwarders.Rows[0];
                BaseForwarder newForwarder = new BaseForwarder();
                newForwarder.DateAdded = row["DateAdded"].ToString();
                newForwarder.forwarderName = row["ForwarderName"].ToString();
                newForwarder.forwarderType = row["ForwarderType"].ToString();
                newForwarder.UniqueID = row["UniqueID"].ToString();
                newForwarder.CredentialID = row["CredentialID"].ToString();
                newForwarder.ParameterID = row["ParameterID"].ToString();
                newForwarder.OwnerID = row["OwnerID"].ToString();
                newForwarder.GroupID = row["GroupID"].ToString();
                newForwarder.Origin = row["Origin"].ToString();
                newForwarder.Description = row["Description"].ToString();
                newForwarder.Enabled = row["Enabled"].ToString();
                newForwarder.ControlState = row["ControlState"].ToString();

                if (newForwarder.CredentialID != "")
                {
                    newForwarder.Credential = BaseCredential.loadCredentialByUniqueID(managementDB, newForwarder.CredentialID);
                }

                if (newForwarder.ParameterID != "")
                {
                    newForwarder.Parameters = BaseParameter.loadParameterByUniqueID(managementDB, newForwarder.ParameterID);
                }

                return newForwarder;
            }

            return null;
        }
    }
}
