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
    public class BaseDiscussion
    {
        public string OwnerID = "";
        public string ForumID = "";
        public string ThreadID = "";
        public string UniqueID = "";

        static public void createDiscussion(IntDatabase managementDB, BaseDiscussion messageThread)
        {
            Tree NewThread = new Tree();
            messageThread.UniqueID = "7" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewThread.AddElement("UniqueID", messageThread.UniqueID);
            NewThread.AddElement("OwnerID", messageThread.OwnerID);
            NewThread.AddElement("ForumID", messageThread.ForumID);
            NewThread.AddElement("ThreadID", messageThread.ThreadID);
            managementDB.InsertTree("Discussions", NewThread);
            NewThread.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Discussions](" +
                    "[UniqueID] TEXT NULL, " +
                    "[ThreadID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[ForumID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Discussions](" +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[ThreadID] VARCHAR(33) NULL, " +
                    "[ForumID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public DataTable getDiscussionsByForumID(IntDatabase managementDB, string forumid)
        {
            String query = "select [Discussions].[UniqueID], [ThreadID], [Discussions].[OwnerID], Accounts.AccountName, Accounts.IconURL, [MessageThreads].[DateAdded], [MessageThreads].[Subject], (select count(*) from [Messages] where [Messages].ThreadID=[Discussions].ThreadID) as [MessageCount], Accounts.DisplayName from [Discussions] join MessageThreads on discussions.ThreadID = MessageThreads.uniqueID join Accounts on Discussions.OwnerID = Accounts.UniqueID where [ForumID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", forumid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return dt;
        }

        static public BaseDiscussion loadDiscussionByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseDiscussion result = null;

            String query = "select * from [Discussions] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseDiscussion newMessageThread = new BaseDiscussion();
                newMessageThread.OwnerID = row["OwnerID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.ThreadID = row["ThreadID"].ToString();
                newMessageThread.ForumID = row["ForumID"].ToString();
                result = newMessageThread;
            }
            return result;
        }

        static public DataTable getDiscussionMostRecentMessage(IntDatabase managementDB, string discussionid)
        {
            String query = "select top 1 Accounts.AccountName, Messages.DateAdded, MessageThreads.Subject from MessageThreads join Messages on Messages.ThreadID = MessageThreads.UniqueID join Accounts on Messages.OwnerID = Accounts.UniqueID  where MessageThreads.UniqueID=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", discussionid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return dt;
        }

        static public String isThreadDiscussion(IntDatabase managementDB, string threadid)
        {
            String result = null;

            String query = "select [UniqueID] from Discussions where [ThreadID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", threadid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            if (dt.Rows.Count>0)
            {
                result = dt.Rows[0]["UniqueID"].ToString();
            }
            return result;
        }

        static public void deleteDiscussionByUniqueID(IntDatabase managementDB, string discussionid)
        {
            // remove discussion
            String query = "delete from [Discussions] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", discussionid);
            managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
        }
    }
}
