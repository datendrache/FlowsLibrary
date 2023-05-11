//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using DatabaseAdapters;
using FatumCore;
using System.IO;

namespace PhlozLib
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
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
                data.addElement("ForwarderType", link.ForwarderType);
                data.addElement("FlowID", link.FlowID);
                data.addElement("ForwarderID", link.ForwarderID);
                data.addElement("Origin", link.Origin);
                data.addElement("*@UniqueID", link.UniqueID);
                managementDB.UpdateTree("[ForwarderLinks]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [ForwarderLinks] ([DateAdded], [ForwarderType], [FlowID], [UniqueID], [ForwarderID]) VALUES (@DateAdded, @ForwarderType, @FlowID, @UniqueID, @ForwarderID);";
                    
                Tree NewForwarderLink = new Tree();
                NewForwarderLink.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewForwarderLink.addElement("@ForwarderType", link.ForwarderType);
                NewForwarderLink.addElement("@FlowID", link.FlowID);
                link.UniqueID = "E" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewForwarderLink.addElement("@UniqueID", link.UniqueID);
                NewForwarderLink.addElement("@Origin", link.Origin);
                NewForwarderLink.addElement("@ForwarderID", link.ForwarderID);
                managementDB.ExecuteDynamic(sql, NewForwarderLink);
                NewForwarderLink.dispose();
            }
        }

        static public void addForwarderLink(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [ForwarderLinks] ([DateAdded], [ForwarderType], [FlowID], [UniqueID], [ForwarderID]) VALUES (@DateAdded, @ForwarderType, @FlowID, @UniqueID, @ForwarderID);";

            Tree NewForwarderLink = new Tree();
            NewForwarderLink.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewForwarderLink.addElement("@ForwarderType", description.getElement("ForwarderType"));
            NewForwarderLink.addElement("@FlowID", description.getElement("FlowID"));
            NewForwarderLink.addElement("@UniqueID", description.getElement("UniqueID"));
            NewForwarderLink.addElement("@Origin", description.getElement("Origin"));
            NewForwarderLink.addElement("@ForwarderID", description.getElement("ForwarderID"));
            managementDB.ExecuteDynamic(sql, NewForwarderLink);
            NewForwarderLink.dispose();
        }

        static public string getXML(ForwarderLink current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "ForwarderLink");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(ForwarderLink current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("ForwarderType", current.ForwarderType);
            tmp.addElement("FlowID", current.FlowID);
            tmp.addElement("ForwarderID", current.ForwarderID);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("Origin", current.Origin);
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
            parms.addElement("@FlowID", flowid);
            forwarderlinks = managementDB.ExecuteDynamic(query,parms);
            parms.dispose();

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
            parms.addElement("@uid", uniqueid);
            forwarderlinks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
            parms.addElement("@forwarderid", ForwarderID);
            managementDB.ExecuteDynamic(query, parms);
        }

        static public void removeLinksByTargetID(IntDatabase managementDB, string FlowID)
        {
            String query = "delete from [ForwarderLinks] where FlowID=@flowid;";
            Tree parms = new Tree();
            parms.addElement("@flowid", FlowID);
            managementDB.ExecuteDynamic(query, parms);
        }
    }
}
