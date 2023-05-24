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

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using DatabaseAdapters;
using Proliferation.Fatum;
using System.IO;

namespace Proliferation.Flows
{
    public class ProcessLink
    {
        public string DateAdded = "";
        public string ProcessID = "";
        public string FlowID = "";
        public string UniqueID = "";
        public string Origin = "";
        public BaseRule Rule = null;

        ~ProcessLink()
        {
            DateAdded = null;
            ProcessID = null;
            FlowID = null;
            UniqueID = null;
            Rule = null;
            Origin = null;
        }

        static public ArrayList loadLinks(CollectionState State)
        {
            return loadLinks(State.managementDB);
        }
        static public ArrayList loadLinks(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Links];";
            processors = managementDB.Execute(query);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                ProcessLink newLink = new ProcessLink();
                newLink.DateAdded = row["DateAdded"].ToString();
                newLink.ProcessID = row["ProcessID"].ToString();
                newLink.FlowID = row["FlowID"].ToString();
                newLink.UniqueID = row["UniqueID"].ToString();
                newLink.Origin = row["Origin"].ToString();
                tmpProcessors.Add(newLink);
            }

            return tmpProcessors;
        }

        static public void updateLink(ProcessLink link, CollectionState State)
        {
            updateLink(State.managementDB, link);
        }

        static public void updateLink(IntDatabase managementDB, ProcessLink link)
        {
            if (link.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("ProcessID", link.ProcessID);
                data.AddElement("FlowID", link.FlowID);
                data.AddElement("Origin", link.Origin);
                data.AddElement("*@UniqueID", link.UniqueID);
                managementDB.UpdateTree("[Links]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "INSERT INTO [Links] ([DateAdded], [ProcessID], [UniqueID], [FlowID], [Origin]) VALUES (@DateAdded, @ProcessID, @UniqueID, @FlowID, @Origin);";
                
                Tree NewProcessLink = new Tree();
                NewProcessLink.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
                NewProcessLink.AddElement("@ProcessID", link.ProcessID);
                link.UniqueID = "L" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewProcessLink.AddElement("@UniqueID", link.UniqueID);
                NewProcessLink.AddElement("@FlowID", link.FlowID);
                NewProcessLink.AddElement("@Origin", link.Origin);
                managementDB.ExecuteDynamic(sql, NewProcessLink);
                NewProcessLink.Dispose();
            }
        }

        static public void addProcessLink(IntDatabase managementDB, Tree description)
        {
            string sql = "INSERT INTO [Links] ([DateAdded], [ProcessID], [UniqueID], [FlowID], [Origin]) VALUES (@DateAdded, @ProcessID, @UniqueID, @FlowID, @Origin);";

            Tree NewProcessLink = new Tree();
            NewProcessLink.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewProcessLink.AddElement("@ProcessID", description.GetElement("ProcessID"));
            NewProcessLink.AddElement("@UniqueID", description.GetElement("UniqueID"));
            NewProcessLink.AddElement("@FlowID", description.GetElement("FlowID"));
            NewProcessLink.AddElement("@Origin", description.GetElement("Origin"));
            managementDB.ExecuteDynamic(sql, NewProcessLink);
            NewProcessLink.Dispose();
        }

        static public void removeProcessLinkByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [ProcessLink] where [UniqueID]=@uniqueid;";
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
                    configDB = "CREATE TABLE [Links](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[ProcessID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[FlowID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Links](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[ProcessID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[FlowID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseprocesslink ON Links([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baseprocesslinkflows ON Links([FlowID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseprocesslink ON Links([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baseprocesslinkflows ON Links([FlowID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public ArrayList loadLinksByProcessorID(IntDatabase managementDB, string processorid)
        {
            DataTable processors;
            String query = "select * from [Links] where ProcessID=@ProcessorID;";
            Tree data = new Tree();
            data.AddElement("@ProcessorID", processorid);
            processors = managementDB.ExecuteDynamic(query, data);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                ProcessLink newRule = new ProcessLink();
                newRule.DateAdded = row["DateAdded"].ToString();
                newRule.ProcessID = row["ProcessID"].ToString();
                newRule.FlowID = row["FlowID"].ToString();
                newRule.UniqueID = row["UniqueID"].ToString();
                newRule.Origin = row["Origin"].ToString();
                tmpProcessors.Add(newRule);
            }

            return tmpProcessors;
        }

        static public ArrayList loadLinksByFlowID(IntDatabase managementDB, string flowid)
        {
            DataTable processors;
            String query = "select * from [Links] where FlowID=@FlowID;";
            Tree data = new Tree();
            data.AddElement("@FlowID", flowid);
            processors = managementDB.ExecuteDynamic(query, data);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                ProcessLink newProcessLink = new ProcessLink();
                newProcessLink.DateAdded = row["DateAdded"].ToString();
                newProcessLink.ProcessID = row["ProcessID"].ToString();
                newProcessLink.FlowID = row["FlowID"].ToString();
                newProcessLink.UniqueID = row["UniqueID"].ToString();
                newProcessLink.Origin = row["Origin"].ToString();
                tmpProcessors.Add(newProcessLink);
            }

            return tmpProcessors;
        }

        static public ProcessLink loadLinkByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            String query = "select * from [Links] where [UniqueID]=@uid;";
            Tree data = new Tree();
            data.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, data);

            DataRow row = processors.Rows[0];

            ProcessLink newRule = new ProcessLink();
            newRule.DateAdded = row["DateAdded"].ToString();
            newRule.ProcessID = row["ProcessID"].ToString();
            newRule.FlowID = row["FlowID"].ToString();
            newRule.UniqueID = row["UniqueID"].ToString();
            newRule.Origin = row["Origin"].ToString();

            return newRule;
        }

        static public void removeLinksByProcessorID(IntDatabase managementDB, string ProcessorID)
        {
            String query = "delete from [Links] where ProcessID=@processorid;";
            Tree parms = new Tree();
            parms.AddElement("@processorid", ProcessorID);
            managementDB.ExecuteDynamic(query, parms);
        }

        static public void removeLinksByFlowID(IntDatabase managementDB, string FlowID)
        {
            String query = "delete from [Links] where FlowID=@flowid;";
            Tree parms = new Tree();
            parms.AddElement("@flowid", FlowID);
            managementDB.ExecuteDynamic(query, parms);
        }

        static public string getXML(ProcessLink current)
        {
            string result = "";
            Tree tmp = getTree(current);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "ProcessLink");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(ProcessLink current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("ProcessID", current.ProcessID);
            tmp.AddElement("FlowID", current.FlowID);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("Origin", current.Origin);
            return tmp;
        }
    }
}
