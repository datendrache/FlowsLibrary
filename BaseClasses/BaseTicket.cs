//   Phloz
//   Copyright (C) 2003-2019 Eric Knight


using System;
using FatumCore;
using DatabaseAdapters;
using System.Data.Entity;
using System.Data;

namespace PhlozLib
{
    public class BaseTicket
    {
        public string UniqueID = "";
        public string ThreadID = "";
        public string OwnerID = "";
        public string InstanceID = "";
        public string Platform = "";
        public string OperatingSystem = "";
        public string Created = "";
        public string Closed = "";
        public string Resolution = "";
        public string Priority = "";
        public string AssigneeID = "";

        static public void updateTicket(IntDatabase managementDB, BaseTicket ticket)
        {
            if (ticket.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("ThreadID", ticket.ThreadID);
                data.addElement("InstanceID", ticket.InstanceID);
                data.addElement("Platform", ticket.Platform);
                data.addElement("OperatingSystem", ticket.OperatingSystem);
                data.addElement("Closed", ticket.Closed);
                data.addElement("Resolution", ticket.Resolution);
                data.addElement("Priority", ticket.Priority);
                data.addElement("AssigneeID", ticket.AssigneeID);
                data.addElement("*@UniqueID", ticket.UniqueID);
                managementDB.UpdateTree("[Tickets]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewTicket = new Tree();
                NewTicket.addElement("OwnerID", ticket.OwnerID);
                NewTicket.addElement("ThreadID", ticket.ThreadID);
                NewTicket.addElement("InstanceID", DateTime.Now.Ticks.ToString());
                NewTicket.addElement("Platform", ticket.Platform);
                NewTicket.addElement("OperatingSystem", ticket.OperatingSystem);
                NewTicket.addElement("Closed", "0");
                NewTicket.addElement("Resolution", ticket.Resolution);
                NewTicket.addElement("Priority", ticket.Priority);
                NewTicket.addElement("AssigneeID", ticket.AssigneeID);
                NewTicket.addElement("Created", DateTime.Now.Ticks.ToString());
                ticket.UniqueID = "3" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewTicket.addElement("UniqueID", ticket.UniqueID);
                managementDB.InsertTree("Tickets", NewTicket);
                NewTicket.dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Tickets](" +
                    "[UniqueID] TEXT NOT NULL," +
                    "[ThreadID] TEXT NOT NULL," +
                    "[OwnerID] TEXT NULL," +
                    "[InstanceID] TEXT NULL," +
                    "[Platform] TEXT NULL," +
                    "[OperatingSystem] TEXT NULL," +
                    "[Created] TEXT NULL," +
                    "[Closed] TEXT NULL," +
                    "[Resolution] TEXT NULL," +
                    "[Priority] TEXT  NULL," +
                    "[AssigneeID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Tickets](" +
                    "[UniqueID] [varchar] (33) NOT NULL," +
                    "[ThreadID] [varchar] (33) NOT NULL," +
                    "[OwnerID] [varchar] (33) NULL," +
                    "[InstanceID] [varchar] (33) NULL," +
                    "[Platform] [varchar] (100) NULL," +
                    "[OperatingSystem] [varchar] (100) NULL," +
                    "[Created] [varchar] (50) NULL," +
                    "[Closed] [varchar] (50) NULL," +
                    "[Resolution] [varchar] (100) NULL," +
                    "[Priority] [varchar] (10) NULL," +
                    "[AssigneeID] [varchar] (33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        public static DataTable getTicketsByAccountID(IntDatabase managementDB, string accountid)
        {
            string SQL = "select t.*, mt.[Subject] from [Tickets] as t join [MessageThreads] as mt on t.ThreadID=mt.UniqueID where t.[OwnerID]=@accountid;";
            Tree parms = new Tree();
            parms.addElement("@accountid", accountid);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.dispose();
            return dt;
        }

        public static DataTable getTicketsAsAdministrator(IntDatabase managementDB)
        {
            string SQL = "select t.*, mt.[Subject] from [Tickets] as t join [MessageThreads] as mt on t.ThreadID=mt.UniqueID where t.[Resolution]='Open';";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        static public BaseTicket loadTicketByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            BaseTicket result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Tickets] where [UniqueID]=@uid;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select * from [Tickets] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseTicket newGroup = new BaseTicket();
                newGroup.Created = row["Created"].ToString();
                newGroup.AssigneeID = row["AssigneeID"].ToString();
                newGroup.Closed = row["Closed"].ToString();
                newGroup.InstanceID = row["InstanceID"].ToString();
                newGroup.UniqueID = row["UniqueID"].ToString();
                newGroup.OperatingSystem = row["OperatingSystem"].ToString();
                newGroup.Platform = row["Platform"].ToString();
                newGroup.Priority = row["Priority"].ToString();
                newGroup.ThreadID = row["ThreadID"].ToString();
                newGroup.Resolution = row["Resolution"].ToString();
                newGroup.OwnerID = row["OwnerID"].ToString();
                result = newGroup;
            }
            return result;
        }
    }
}
