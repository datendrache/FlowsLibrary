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
            NewBlog.AddElement("UniqueID", messageBlog.UniqueID);
            NewBlog.AddElement("OwnerID", messageBlog.OwnerID);
            NewBlog.AddElement("BlogID", messageBlog.BlogID);
            NewBlog.AddElement("MessageID", messageBlog.MessageID);
            NewBlog.AddElement("Title", messageBlog.Title);
            managementDB.InsertTree("BlogEntry", NewBlog);
            NewBlog.Dispose();
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
            parms.AddElement("@uid", BlogID);
            DataTable dt = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return dt;
        }

        static public BaseBlogEntry loadBlogEntryByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseBlogEntry result = null;

            String query = "select * from [BlogEntry] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
                parms.AddElement("@uid", exBlogEntry.UniqueID);
                managementDB.ExecuteDynamic(query, parms);
                parms.Dispose();
            }
        }
    }
}
