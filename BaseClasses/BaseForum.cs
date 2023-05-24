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
    public class BaseForum
    {
        public string OwnerID = "";
        public string Name = "";
        public string UniqueID = "";
        public string ParameterID = "";
        public string IconURL = "";
        public string Description = "";
        public string Category = "";

        static public void updateThread(IntDatabase managementDB, BaseForum forum)
        {
            if (forum.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Name", forum.Name);
                data.AddElement("IconURL", forum.IconURL);
                data.AddElement("Description", forum.Description);
                data.AddElement("ParameterID", forum.ParameterID);
                data.AddElement("Category", forum.Category);
                data.AddElement("*@UniqueID", forum.UniqueID);
                managementDB.UpdateTree("[Forums]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewThread = new Tree();
                forum.UniqueID = "f" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewThread.AddElement("UniqueID", forum.UniqueID);
                NewThread.AddElement("OwnerID", forum.OwnerID);
                NewThread.AddElement("IconURL", forum.IconURL);
                NewThread.AddElement("Name", forum.IconURL);
                NewThread.AddElement("Description", forum.Description);
                NewThread.AddElement("Category", forum.Category);
                NewThread.AddElement("ParameterID", forum.ParameterID);
                managementDB.InsertTree("MessageThreads", NewThread);
                NewThread.Dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Forums](" +
                    "[UniqueID] TEXT NULL, " +
                    "[Name] TEXT NULL, " +
                    "[Category] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[IconURL] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[Description] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Forums](" +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[Name] NVARCHAR(100) NULL, " +
                    "[Category] NVARCHAR(100) NULL, " +
                    "[IconURL] VARCHAR(100) NULL, "+
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(200) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public DataTable getForumByUniqueID(IntDatabase managementDB, string forumid)
        {
            String query = "select * from [Forums] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", forumid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return dt;
        }

        static public DataTable getForums(IntDatabase managementDB)
        {
            String query = "select *, (select count (*) from [Discussions] where Discussions.ForumID=[Forums].UniqueID) as [DiscussionCount] from [Forums] order by [position] asc;";
            DataTable dt = managementDB.Execute(query);
            return dt;
        }

        static public BaseForum loadForumByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseForum result = null;

            String query = "select * from [Forums] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseForum newMessageThread = new BaseForum();
                newMessageThread.OwnerID = row["OwnerID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.IconURL = row["IconURL"].ToString();
                newMessageThread.ParameterID = row["ParameterID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.Description = row["Description"].ToString();
                newMessageThread.Name = row["Name"].ToString();
                result = newMessageThread;
            }
            return result;
        }

        static public int getForumMessageCount(IntDatabase managementDB, string forumid)
        {
            String query = "select count(*) as TotalMessages from Forums join Discussions on Forums.UniqueId = Discussions.ForumID join Messages on Messages.ThreadID = Discussions.ThreadID where Forums.UniqueID=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", forumid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return Convert.ToInt32(dt.Rows[0]["TotalMessages"]);
        }

        static public DataTable getForumMostRecentMessage(IntDatabase managementDB, string forumid)
        {
            String query = "select top 1 Accounts.AccountName, Messages.DateAdded, Messages.OwnerID, MessageThreads.Subject, Accounts.DisplayName from Forums join Discussions on Forums.UniqueId = Discussions.ForumID join MessageThreads on Discussions.ThreadID = MessageThreads.UniqueID join Messages on Messages.ThreadID = Discussions.ThreadID join Accounts on Messages.OwnerID = Accounts.UniqueID  where Forums.UniqueID=@uid order by DateAdded desc";
            Tree parms = new Tree();
            parms.AddElement("@uid", forumid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return dt;
        }
    }
}
