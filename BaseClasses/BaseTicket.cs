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
using DatabaseAdapters;
using System.Data;

namespace Proliferation.Flows
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
                data.AddElement("ThreadID", ticket.ThreadID);
                data.AddElement("InstanceID", ticket.InstanceID);
                data.AddElement("Platform", ticket.Platform);
                data.AddElement("OperatingSystem", ticket.OperatingSystem);
                data.AddElement("Closed", ticket.Closed);
                data.AddElement("Resolution", ticket.Resolution);
                data.AddElement("Priority", ticket.Priority);
                data.AddElement("AssigneeID", ticket.AssigneeID);
                data.AddElement("*@UniqueID", ticket.UniqueID);
                managementDB.UpdateTree("[Tickets]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewTicket = new Tree();
                NewTicket.AddElement("OwnerID", ticket.OwnerID);
                NewTicket.AddElement("ThreadID", ticket.ThreadID);
                NewTicket.AddElement("InstanceID", DateTime.Now.Ticks.ToString());
                NewTicket.AddElement("Platform", ticket.Platform);
                NewTicket.AddElement("OperatingSystem", ticket.OperatingSystem);
                NewTicket.AddElement("Closed", "0");
                NewTicket.AddElement("Resolution", ticket.Resolution);
                NewTicket.AddElement("Priority", ticket.Priority);
                NewTicket.AddElement("AssigneeID", ticket.AssigneeID);
                NewTicket.AddElement("Created", DateTime.Now.Ticks.ToString());
                ticket.UniqueID = "3" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewTicket.AddElement("UniqueID", ticket.UniqueID);
                managementDB.InsertTree("Tickets", NewTicket);
                NewTicket.Dispose();
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
            parms.AddElement("@accountid", accountid);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.Dispose();
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
            parms.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
