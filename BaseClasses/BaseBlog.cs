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
    public class BaseBlog
    {
        public string OwnerID = "";
        public string UniqueID = "";
        public string Title = "";
        public string Visible = "";
        public string AllowPosts = "";
        public string AllowComments = "";
        public string Description = "";

        static public void createBlog(IntDatabase managementDB, BaseBlog blog)
        {
            Tree NewBlog = new Tree();
            blog.UniqueID = "B" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewBlog.AddElement("UniqueID", blog.UniqueID);
            NewBlog.AddElement("OwnerID", blog.OwnerID);
            NewBlog.AddElement("Visible", blog.Visible);
            NewBlog.AddElement("AllowPosts", blog.AllowPosts);
            NewBlog.AddElement("Title", blog.Title);
            NewBlog.AddElement("AllowComments", blog.AllowComments);
            managementDB.InsertTree("Blog", NewBlog);
            NewBlog.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Blog](" +
                    "[UniqueID] TEXT NULL, " +
                    "[Title] TEXT NULL, " +
                    "[Visible] TEXT NULL, " +
                    "[AllowPosts] TEXT NULL, " +
                    "[AllowComments] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[OwnerID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Blog](" +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[Title] NVARCHAR(300) NULL, " +
                    "[Visible] VARCHAR(33) NULL, " +
                    "[AllowPosts] VARCHAR(33) NULL, " +
                    "[AllowComments] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(200) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public DataTable getBlogs(IntDatabase managementDB, Boolean OnlyVisible)
        {
            String query = "select * from [Blogs];";
            if (OnlyVisible)
            {
                query = "select * from [Blogs] where Visible='true';";
            }
            DataTable dt = managementDB.Execute(query);
            return dt;
        }

        static public BaseBlog loadBlogByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseBlog result = null;

            String query = "select * from [Blogs] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseBlog newBlog = new BaseBlog();
                newBlog.OwnerID = row["OwnerID"].ToString();
                newBlog.UniqueID = row["UniqueID"].ToString();
                newBlog.Title = row["Title"].ToString();
                newBlog.Visible = row["Visible"].ToString();
                newBlog.AllowPosts = row["AllowPosts"].ToString();
                newBlog.AllowComments = row["AllowComments"].ToString();
                newBlog.Description = row["Description"].ToString();
                result = newBlog;
            }
            return result;
        }
    }
}
