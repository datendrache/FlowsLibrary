//   Phloz
//   Copyright (C) 2003-2021 Eric Knight


using System;
using FatumCore;
using DatabaseAdapters;
using System.Data.Entity;
using System.Data;

namespace PhlozLib
{
    public class BaseBlogEntry
    {
        public string OwnerID = "";
        public string BlogID = "";
        public string Title = "";
        public string MessageID = "";
        public string UniqueID = "";

        static public void createBlogEntry(IntDatabase managementDB, BaseBlogEntry messageBlog)
        {
            Tree NewBlog = new Tree();
            messageBlog.UniqueID = "8" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewBlog.addElement("UniqueID", messageBlog.UniqueID);
            NewBlog.addElement("OwnerID", messageBlog.OwnerID);
            NewBlog.addElement("BlogID", messageBlog.BlogID);
            NewBlog.addElement("MessageID", messageBlog.MessageID);
            NewBlog.addElement("Title", messageBlog.Title);
            managementDB.InsertTree("BlogEntry", NewBlog);
            NewBlog.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [BlogEntry](" +
                    "[UniqueID] TEXT NULL, " +
                    "[DateAdded] BIGINT NULL, " +
                    "[MessageID] TEXT NULL, " +
                    "[BlogID] TEXT NULL, " +
                    "[Title] TEXT NULL, " +
                    "[OwnerID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [BlogEntry](" +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[DateAdded] BIGINT NULL, " +
                    "[MessageID] VARCHAR(33) NULL, " +
                    "[BlogID] VARCHAR(33) NULL, " +
                    "[Title] NVARCHAR(300) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public DataTable getBlogEntriesByBlog(IntDatabase managementDB, string BlogID)
        {
            String query = "select [Title], [Messages].DateAdded, [Messages].OwnerID, [Messages].LastEdit, [Messages].[Document], [Messages].[UniqueID] as [MessageID], [BlogEntry].UniqueID as [BlogEntryID], [AccountName], [Role], [IconURL], [Accounts].DisplayName  from [BlogEntry] join [Messages] on [BlogEntry].[MessageID]=[Messages].[UniqueID] join [Accounts] on [Messages].[OwnerID]=[Accounts].[UniqueID] where BlogID=@uid order by [Messages].DateAdded desc";
            Tree parms = new Tree();
            parms.addElement("@uid", BlogID);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            return dt;
        }

        static public BaseBlogEntry loadBlogEntryByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseBlogEntry result = null;

            String query = "select * from [BlogEntry] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseBlogEntry newMessageThread = new BaseBlogEntry ();
                newMessageThread.OwnerID = row["OwnerID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.Title = row["Title"].ToString();
                newMessageThread.MessageID = row["MessageID"].ToString();
                newMessageThread.BlogID = row["BlogID"].ToString();
                result = newMessageThread;
            }
            return result;
        }

        static public void deleteBlogEntryByUniqueID(IntDatabase managementDB, string blogentryid)
        {
            // remove all messages in thread

            BaseBlogEntry exBlogEntry = BaseBlogEntry.loadBlogEntryByUniqueID(managementDB, blogentryid);
            if (exBlogEntry!=null)
            {
                BaseMessage.deleteMessageByUniqueID(managementDB, exBlogEntry.MessageID);
                
                String query = "delete from [BlogEntry] where [UniqueID]=@uid;";
                Tree parms = new Tree();
                parms.addElement("@uid", exBlogEntry.UniqueID);
                managementDB.ExecuteDynamic(query, parms);
                parms.dispose();
            }
        }
    }
}
