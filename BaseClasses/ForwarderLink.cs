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


using System.Data;
using System.Collections;
using DatabaseAdapters;
using Proliferation.Fatum;
using System.IO;

namespace Proliferation.Flows
{
    public class ForwarderLink
    {
        public string DateAdded = "";
        public string ForwarderType = "";   // Alarm or Document
        public string FlowID = "";
        public string ForwarderID = "";
        public string UniqueID = "";
        public string Origin = "";

        public BaseFlow Flow = null;
        public BaseForwarder Forwarder = null;

        ~ForwarderLink()
        {
            DateAdded = null;
            ForwarderType = null;
            FlowID = null;
            ForwarderID = null; ;
            UniqueID = null;
            Flow = null;
            Forwarder = null;
            Origin = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [ForwarderLinks](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[ForwarderType] TEXT NULL, " +
                    "[FlowID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[ForwarderID] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [ForwarderLinks](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[ForwarderType] VARCHAR(20) NULL, " +
                    "[FlowID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[ForwarderID] VARCHAR(33) NULL );";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseforwarderlinks ON ForwarderLinks([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baseforwarderlinksflow ON ForwarderLinks([FlowID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseforwarderlinks ON ForwarderLinks([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baseforwarderlinksflow ON ForwarderLinks([FlowID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }


        static public void removeForwarderLinkByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [ForwarderLinks] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void updateForwarderLink(CollectionState State, ForwarderLink link)
        {
            updateForwarderLink(State.managementDB, link);
        }

        static public void updateForwarderLink(IntDatabase managementDB, ForwarderLink link)
        {
            if (link.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("ForwarderType", link.ForwarderType);
                data.AddElement("FlowID", link.FlowID);
                data.AddElement("ForwarderID", link.ForwarderID);
                data.AddElement("Origin", link.Origin);
                data.AddElement("*@UniqueID", link.UniqueID);
                managementDB.UpdateTree("[ForwarderLinks]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [ForwarderLinks] ([DateAdded], [ForwarderType], [FlowID], [UniqueID], [ForwarderID]) VALUES (@DateAdded, @ForwarderType, @FlowID, @UniqueID, @ForwarderID);";
                    
                Tree NewForwarderLink = new Tree();
                NewForwarderLink.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewForwarderLink.AddElement("@ForwarderType", link.ForwarderType);
                NewForwarderLink.AddElement("@FlowID", link.FlowID);
                link.UniqueID = "E" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewForwarderLink.AddElement("@UniqueID", link.UniqueID);
                NewForwarderLink.AddElement("@Origin", link.Origin);
                NewForwarderLink.AddElement("@ForwarderID", link.ForwarderID);
                managementDB.ExecuteDynamic(sql, NewForwarderLink);
                NewForwarderLink.Dispose();
            }
        }

        static public void addForwarderLink(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [ForwarderLinks] ([DateAdded], [ForwarderType], [FlowID], [UniqueID], [ForwarderID]) VALUES (@DateAdded, @ForwarderType, @FlowID, @UniqueID, @ForwarderID);";

            Tree NewForwarderLink = new Tree();
            NewForwarderLink.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewForwarderLink.AddElement("@ForwarderType", description.GetElement("ForwarderType"));
            NewForwarderLink.AddElement("@FlowID", description.GetElement("FlowID"));
            NewForwarderLink.AddElement("@UniqueID", description.GetElement("UniqueID"));
            NewForwarderLink.AddElement("@Origin", description.GetElement("Origin"));
            NewForwarderLink.AddElement("@ForwarderID", description.GetElement("ForwarderID"));
            managementDB.ExecuteDynamic(sql, NewForwarderLink);
            NewForwarderLink.Dispose();
        }

        static public string getXML(ForwarderLink current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "ForwarderLink");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(ForwarderLink current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("ForwarderType", current.ForwarderType);
            tmp.AddElement("FlowID", current.FlowID);
            tmp.AddElement("ForwarderID", current.ForwarderID);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("Origin", current.Origin);
            return tmp;
        }

        static public ArrayList loadLinks(CollectionState State)
        {
            return loadLinks(State.managementDB);
        }

        static public ArrayList loadLinks(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [ForwarderLinks];";
            processors = managementDB.Execute(query);

            ArrayList tmpForwarders = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                ForwarderLink newRule = new ForwarderLink();
                newRule.DateAdded = row["DateAdded"].ToString();
                newRule.ForwarderType = row["ForwarderType"].ToString();
                newRule.ForwarderID = row["ForwarderID"].ToString();
                newRule.UniqueID = row["UniqueID"].ToString();
                newRule.Origin = row["Origin"].ToString();
                newRule.FlowID = row["FlowID"].ToString();
                tmpForwarders.Add(newRule);
            }
            return tmpForwarders;
        }

        static public ArrayList loadLinksByFlowID(IntDatabase managementDB, string flowid)
        {
            DataTable forwarderlinks;
            String query = "select * from [ForwarderLinks] where FlowID=@FlowID;";
            Tree parms = new Tree();
            parms.AddElement("@FlowID", flowid);
            forwarderlinks = managementDB.ExecuteDynamic(query,parms);
            parms.Dispose();

            ArrayList tmpForwarders = new ArrayList();

            foreach (DataRow row in forwarderlinks.Rows)
            {
                ForwarderLink newForwarderLink = new ForwarderLink();
                newForwarderLink.DateAdded = row["DateAdded"].ToString();
                newForwarderLink.ForwarderType = row["ForwarderType"].ToString();
                newForwarderLink.ForwarderID = row["ForwarderID"].ToString();
                newForwarderLink.UniqueID = row["UniqueID"].ToString();
                newForwarderLink.Origin = row["Origin"].ToString();
                newForwarderLink.FlowID = flowid;
                tmpForwarders.Add(newForwarderLink);
            }
            return tmpForwarders;
        }

        static public ForwarderLink loadLinkByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable forwarderlinks;
            String query = "select * from [ForwarderLinks] where UniqueID=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            forwarderlinks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            if (forwarderlinks.Rows.Count>0)
            {
                DataRow row = forwarderlinks.Rows[0];
                ForwarderLink newForwarderLink = new ForwarderLink();
                newForwarderLink.DateAdded = row["DateAdded"].ToString();
                newForwarderLink.ForwarderType = row["ForwarderType"].ToString();
                newForwarderLink.ForwarderID = row["ForwarderID"].ToString();
                newForwarderLink.UniqueID = row["UniqueID"].ToString();
                newForwarderLink.Origin = row["Origin"].ToString();
                newForwarderLink.FlowID = row["FlowID"].ToString();
                return newForwarderLink;
            }
            else
            {
                return null;
            }
        }

        static public void removeLinksByForwarderID(IntDatabase managementDB, string ForwarderID)
        {
            String query = "delete from [ForwarderLinks] where ForwarderID=@forwarderid;";
            Tree parms = new Tree();
            parms.AddElement("@forwarderid", ForwarderID);
            managementDB.ExecuteDynamic(query, parms);
        }

        static public void removeLinksByTargetID(IntDatabase managementDB, string FlowID)
        {
            String query = "delete from [ForwarderLinks] where FlowID=@flowid;";
            Tree parms = new Tree();
            parms.AddElement("@flowid", FlowID);
            managementDB.ExecuteDynamic(query, parms);
        }
    }
}
