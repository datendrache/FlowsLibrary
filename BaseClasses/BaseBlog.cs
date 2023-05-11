//   Phloz
//   Copyright (C) 2003-2019 Eric Knight


using System;
using FatumCore;
using DatabaseAdapters;
using System.Data.Entity;
using System.Data;

namespace PhlozLib
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
            NewBlog.addElement("UniqueID", blog.UniqueID);
            NewBlog.addElement("OwnerID", blog.OwnerID);
            NewBlog.addElement("Visible", blog.Visible);
            NewBlog.addElement("AllowPosts", blog.AllowPosts);
            NewBlog.addElement("Title", blog.Title);
            NewBlog.addElement("AllowComments", blog.AllowComments);
            managementDB.InsertTree("Blog", NewBlog);
            NewBlog.dispose();
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
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
