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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
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
                data.AddElement("ForwarderName", forwarder.forwarderName.ToString());
                data.AddElement("ForwarderType", forwarder.forwarderType.ToString());
                data.AddElement("OwnerID", forwarder.OwnerID);
                data.AddElement("GroupID", forwarder.GroupID);
                data.AddElement("CredentialID", forwarder.CredentialID);
                data.AddElement("ParameterID", forwarder.ParameterID);
                data.AddElement("Enabled", forwarder.Enabled);
                data.AddElement("Origin", forwarder.Origin);
                data.AddElement("Description", forwarder.Description);
                data.AddElement("ControlState", forwarder.ControlState);
                data.AddElement("*@UniqueID", forwarder.UniqueID);
                managementDB.UpdateTree("[Forwarders]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Forwarders] ([DateAdded], [UniqueID], [ForwarderName], [ForwarderType], [CredentialID], [ParameterID], [OwnerID], [GroupID], [Enabled], [Description], [Origin], [ControlState]) VALUES " +
                    "(@DateAdded, @UniqueID, @ForwarderName, @ForwarderType, @CredentialID, @ParameterID, @OwnerID, @GroupID, @Enabled, @Description, @Origin, @ControlState);";

                Tree NewFilter = new Tree();
                NewFilter.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewFilter.AddElement("@ForwarderName", forwarder.forwarderName);
                NewFilter.AddElement("@ForwarderType", forwarder.forwarderType);
                forwarder.UniqueID = "D" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewFilter.AddElement("@UniqueID", forwarder.UniqueID);
                NewFilter.AddElement("@OwnerID", forwarder.OwnerID);
                NewFilter.AddElement("@GroupID", forwarder.GroupID);
                NewFilter.AddElement("@CredentialID", forwarder.CredentialID);
                NewFilter.AddElement("@ParameterID", forwarder.ParameterID);
                NewFilter.AddElement("@Enabled", forwarder.Enabled);
                NewFilter.AddElement("@Origin", forwarder.Origin);
                NewFilter.AddElement("@ControlState", forwarder.ControlState);
                NewFilter.AddElement("@Description", forwarder.Description);
                managementDB.ExecuteDynamic(sql, NewFilter);
                NewFilter.Dispose();
            }
        }

        static public void addForwarder(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [Forwarders] ([DateAdded], [UniqueID], [ForwarderName], [ForwarderType], [CredentialID], [ParameterID], [OwnerID], [GroupID], [Enabled], [Description], [Origin], [ControlState]) VALUES " +
                "(@DateAdded, @UniqueID, @ForwarderName, @ForwarderType, @CredentialID, @ParameterID, @OwnerID, @GroupID, @Enabled, @Description, @Origin, @ControlState);";

            Tree NewFilter = new Tree();
            NewFilter.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewFilter.AddElement("@ForwarderName", description.GetElement("ForwarderName"));
            NewFilter.AddElement("@ForwarderType", description.GetElement("ForwarderType"));
            NewFilter.AddElement("@UniqueID", description.GetElement("UniqueID"));
            NewFilter.AddElement("@OwnerID", description.GetElement("OwnerID"));
            NewFilter.AddElement("@GroupID", description.GetElement("GroupID"));
            NewFilter.AddElement("@CredentialID", description.GetElement("CredentialID"));
            NewFilter.AddElement("@ParameterID", description.GetElement("ParameterID"));
            NewFilter.AddElement("@Enabled", description.GetElement("Enabled"));
            NewFilter.AddElement("@Origin", description.GetElement("Origin"));
            NewFilter.AddElement("@ControlName", description.GetElement("ControlName"));
            NewFilter.AddElement("@Description", description.GetElement("Description"));
            managementDB.ExecuteDynamic(sql, NewFilter);
            NewFilter.Dispose();
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
            TreeDataAccess.WriteXML(outs, tmp, "BaseForwarder");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseForwarder current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("forwarderName", current.forwarderName);
            tmp.AddElement("forwarderType", current.forwarderType);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("CredentialID", current.CredentialID);
            tmp.AddElement("ParameterID", current.ParameterID);
            tmp.AddElement("Enabled", current.Enabled);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("ControlState", current.ControlState);
            tmp.AddElement("Description", current.Description);
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
            data.AddElement("@UniqueID", forwarderid);
            forwarders = managementDB.ExecuteDynamic(query,data);
            data.Dispose();

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
