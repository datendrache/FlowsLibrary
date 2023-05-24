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
using DatabaseAdapters;
using Proliferation.Fatum;

namespace Proliferation.Flows
{
    public class BaseMessage
    {
        public string DateAdded = "";
        public string LastEdit = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string OwnerID = "";
        public string Document = "";
        public string ThreadID = "";
        public string Visible = "true";

        ~BaseMessage()
        {
            UniqueID = null;
            GroupID = null;
            OwnerID = null;
            Document = null;
            Visible = null;
            ThreadID = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Messages](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[LastEdit] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[ThreadID] TEXT NULL, " +
                    "[Visible] TEXT NULL, " +
                    "[Document] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Messages](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[LastEdit] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[ThreadID] VARCHAR(33) NULL, " +
                    "[Visible] VARCHAR(10) NULL, " +
                    "[Document] TEXT NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public void updateMessage(IntDatabase managementDB, BaseMessage message)
        {
            if (message.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("LastEdit", DateTime.Now.Ticks.ToString());
                data.AddElement("Document", message.Document);
                data.AddElement("Visible", message.Visible);
                data.AddElement("*@UniqueID", message.UniqueID.ToString());
                managementDB.UpdateTree("[Messages]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewMessage = new Tree();
                NewMessage.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewMessage.AddElement("GroupID", message.GroupID);
                NewMessage.AddElement("OwnerID", message.OwnerID);
                NewMessage.AddElement("Document", message.Document);
                message.UniqueID = "1" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewMessage.AddElement("UniqueID", message.UniqueID);
                NewMessage.AddElement("ThreadID", message.ThreadID);
                NewMessage.AddElement("Visible", message.Visible);
                managementDB.InsertTree("[Messages]", NewMessage);
                NewMessage.Dispose();
            }
        }

        static public BaseMessage loadMessageByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseMessage result = null;

            String query = "select * from [Messages] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);

            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseMessage newMessage = new BaseMessage();
                newMessage.DateAdded = row["DateAdded"].ToString();
                newMessage.OwnerID = row["OwnerID"].ToString();
                newMessage.UniqueID = row["UniqueID"].ToString();
                newMessage.GroupID = row["GroupID"].ToString();
                newMessage.Visible = row["Visible"].ToString();
                newMessage.ThreadID = row["ThreadID"].ToString();
                newMessage.Document = row["Document"].ToString();
                result = newMessage;
            }
            return result;
        }

        static public void deleteMessageByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseMessage result = null;

            String query = "delete from [Messages] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);

            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
        }

        public static DataTable getPostsMostRecent(IntDatabase managementDB)
        {
            string SQL = "SELECT TOP (10) * from [MostRecentPosts] order by [DateAdded] desc;";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }
    }
}
