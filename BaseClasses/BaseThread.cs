//   Phloz
//   Copyright (C) 2003-2019 Eric Knight


using System;
using FatumCore;
using DatabaseAdapters;
using System.Data.Entity;
using System.Data;

namespace PhlozLib
{
    public class BaseThread
    {
        public string OwnerID = "";
        public string DateAdded = "";
        public string GroupID = "";
        public string UniqueID = "";
        public string ThreadType = "";
        public string Visible = "";
        public string Subject = "";

        static public void updateThread(IntDatabase managementDB, BaseThread messageThread)
        {
            if (messageThread.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("ThreadType", messageThread.ThreadType);
                data.addElement("Visible", messageThread.Visible);
                data.addElement("Subject", messageThread.Subject);
                data.addElement("GroupID", messageThread.GroupID);
                data.addElement("LastEdit", DateTime.Now.Ticks.ToString());
                data.addElement("*@UniqueID", messageThread.UniqueID);
                managementDB.UpdateTree("[MessageThreads]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewThread = new Tree();
                messageThread.UniqueID = "2" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewThread.addElement("UniqueID", messageThread.UniqueID);
                NewThread.addElement("OwnerID", messageThread.OwnerID);
                NewThread.addElement("GroupID", messageThread.GroupID);
                NewThread.addElement("ThreadType", messageThread.ThreadType);
                NewThread.addElement("Subject", messageThread.Subject);
                NewThread.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewThread.addElement("LastEdit", DateTime.Now.Ticks.ToString());
                NewThread.addElement("Visible", messageThread.Visible);
                managementDB.InsertTree("MessageThreads", NewThread);
                NewThread.dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [MessageThreads](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[ThreadType] TEXT NULL, " +
                    "[Subject] TEXT NULL, " +
                    "[Visible] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [MessageThreads](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[LastEdit] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[ThreadType] VARCHAR(33) NULL, "+
                    "[Subject] NVARCHAR(80) NULL, " +
                    "[Visible] VARCHAR(10) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public DataTable getMessagesByThreadID(IntDatabase managementDB, string threadid, string direction, Boolean visibleonly)
        {
            String query = "select [Messages].DateAdded, [Messages].LastEdit, [Messages].UniqueID, [Messages].OwnerID, [Messages].Visible, [Messages].Document, [Accounts].AccountName, [Accounts].[Role], [Accounts].IconURL, [Accounts].DisplayName from [Messages] join Accounts on messages.ownerid=accounts.UniqueID where [ThreadID]=@uid order by messages.DateAdded " + direction + ";";
            if (visibleonly)
            {
                query = "select [Messages].DateAdded, [Messages].LastEdit, [Messages].UniqueID, [Messages].OwnerID, [Messages].Visible, [Messages].Document, [Accounts].AccountName, [Accounts].[Role], [Accounts].IconURL, [Accounts].DisplayName  from [Messages] join Accounts on messages.ownerid=accounts.UniqueID where [ThreadID]=@uid and [Messages].Visible='true' order by messages.DateAdded " + direction + ";";
            }
            Tree parms = new Tree();
            parms.addElement("@uid", threadid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            return dt;
        }

        static public BaseThread loadThreadByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseThread result = null;

            String query = "select * from [MessageThreads] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseThread newMessageThread = new BaseThread();
                newMessageThread.DateAdded = row["DateAdded"].ToString();
                newMessageThread.OwnerID = row["OwnerID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.GroupID = row["GroupID"].ToString();
                newMessageThread.Visible = row["Visible"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.Subject = row["Subject"].ToString();
                result = newMessageThread;
            }
            return result;
        }

        static public int countMessagesByThreadID(IntDatabase managementDB, string threadid)
        {
            int result = 0;
            String query = "select count(*) as [MessageCount] where [ThreadID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", threadid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            result = Convert.ToInt32(dt.Rows[0]["MessageCount"]);
            return result;
        }

        static public Boolean isMessageThreadRoot(IntDatabase managementDB, string threadid, string messageid)
        {
            Boolean result = true;

            String query = "select top 1 [UniqueID] from [Messages] where [ThreadID]=@uid order by dateadded asc;";
            Tree parms = new Tree();
            parms.addElement("@uid", threadid);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            string rootMessageID = dt.Rows[0]["UniqueID"].ToString();
            if (rootMessageID != messageid)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        static public void deleteThreadByUniqueID(IntDatabase managementDB, string threadid)
        {
            // remove all messages in thread

            DataTable dt = getMessagesByThreadID(managementDB, threadid, "asc", false);
            foreach (DataRow row in dt.Rows)
            {
                string messageid = row["UniqueID"].ToString();
                BaseMessage.deleteMessageByUniqueID(managementDB, messageid);
            }

            // remove thread 
            String query = "delete from [MessageThreads] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", threadid);
            managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
        }
    }
}
