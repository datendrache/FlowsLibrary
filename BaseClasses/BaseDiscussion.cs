//   Phloz
//   Copyright (C) 2003-2019 Eric Knight


using System;
using FatumCore;
using DatabaseAdapters;
using System.Data.Entity;
using System.Data;

namespace PhlozLib
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
            NewThread.addElement("UniqueID", messageThread.UniqueID);
            NewThread.addElement("OwnerID", messageThread.OwnerID);
            NewThread.addElement("ForumID", messageThread.ForumID);
            NewThread.addElement("ThreadID", messageThread.ThreadID);
            managementDB.InsertTree("Discussions", NewThread);
            NewThread.dispose();
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
            parms.addElement("@uid", forumid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            return dt;
        }

        static public BaseDiscussion loadDiscussionByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseDiscussion result = null;

            String query = "select * from [Discussions] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
            parms.addElement("@uid", discussionid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            return dt;
        }

        static public String isThreadDiscussion(IntDatabase managementDB, string threadid)
        {
            String result = null;

            String query = "select [UniqueID] from Discussions where [ThreadID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", threadid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
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
            parms.addElement("@uid", discussionid);
            managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
        }
    }
}
