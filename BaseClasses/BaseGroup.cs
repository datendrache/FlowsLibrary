//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using DatabaseAdapters;

namespace PhlozLib
{
    public class BaseGroup
    {
        public string DateAdded = "";
        public string Category = "";
        public string Subcategory = "";
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Name = "";
        public string Origin = "";

        ~BaseGroup()
        {
            DateAdded = null;
            Category = null;
            Subcategory = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            Name = null;
            Origin = null;
        }

        static public ArrayList loadGroups(CollectionState State)
        {
            return loadGroups(State.managementDB);
        }

        static public ArrayList loadGroups(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from Groups;";
            processors = managementDB.Execute(query);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseGroup newGroup = new BaseGroup();
                newGroup.DateAdded = row["DateAdded"].ToString();
                newGroup.Category = row["Category"].ToString();
                newGroup.Subcategory = row["Subcategory"].ToString();
                newGroup.Name = row["Name"].ToString();
                newGroup.UniqueID = row["UniqueID"].ToString();
                newGroup.GroupID = row["GroupID"].ToString();
                newGroup.OwnerID = row["OwnerID"].ToString();
                newGroup.Origin = row["Origin"].ToString();
                tmpProcessors.Add(newGroup);
            }
            return tmpProcessors;
        }

        static public void removeGroupByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Groups] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updateGroup(IntDatabase managementDB, BaseGroup group)
        {
            if (group.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Name", group.Name);
                data.addElement("Category", group.Category);
                data.addElement("Subcategory", group.Subcategory);
                data.addElement("OwnerID", group.OwnerID);
                data.addElement("GroupID", group.GroupID);
                data.addElement("Origin", group.Origin);
                data.addElement("*@UniqueID", group.UniqueID);
                managementDB.UpdateTree("[Groups]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Groups] ([DateAdded], [Name], [Category], [Subcategory], [UniqueID], [OwnerID], [GroupID], [Origin]) VALUES (@DateAdded, @Name, @Category, @Subcategory, @UniqueID, @OwnerID, @GroupID, @Origin);";
                Tree NewGroup = new Tree();
                group.DateAdded = DateTime.Now.Ticks.ToString();
                NewGroup.addElement("@DateAdded", group.DateAdded);
                NewGroup.addElement("@Name", group.Name);
                NewGroup.addElement("@Category", group.Category);
                NewGroup.addElement("@Subcategory", group.Subcategory);
                group.UniqueID = "G" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewGroup.addElement("@UniqueID", group.UniqueID);
                NewGroup.addElement("@GroupID", group.GroupID);
                NewGroup.addElement("@OwnerID", group.OwnerID);
                NewGroup.addElement("@Origin", group.Origin);
                managementDB.ExecuteDynamic(sql, NewGroup);
                NewGroup.dispose();
            }
        }

        static public void addGroup(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [Groups] ([DateAdded], [Name], [Category], [Subcategory], [UniqueID], [OwnerID], [GroupID], [Origin]) VALUES (@DateAdded, @Name, @Category, @Subcategory, @UniqueID, @OwnerID, @GroupID, @Origin);";
            Tree NewGroup = new Tree();
            NewGroup.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewGroup.addElement("@Name", description.getElement("Name"));
            NewGroup.addElement("@Category", description.getElement("Category"));
            NewGroup.addElement("@Subcategory", description.getElement("Subcategory"));
            NewGroup.addElement("@UniqueID", description.getElement("UniqueID"));
            NewGroup.addElement("@GroupID", description.getElement("GroupID"));
            NewGroup.addElement("@OwnerID", description.getElement("OwnerID"));
            NewGroup.addElement("@Origin", description.getElement("Origin"));
            managementDB.ExecuteDynamic(sql, NewGroup);
            NewGroup.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Groups](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[Category] TEXT NULL, " +
                    "[Subcategory] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Name] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Groups](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[Category] NVARCHAR(100) NULL, " +
                    "[Subcategory] NVARCHAR(100) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Name] NVARCHAR(100) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basegroups ON Groups([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseagroups ON Groups([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getXML(BaseGroup current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseGroup");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseGroup current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("Name", current.Name);
            tmp.addElement("Category", current.Category);
            tmp.addElement("Subcategory", current.Subcategory);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);
            return tmp;
        }

        public static DataTable getGroupList(IntDatabase managementDB)
        {
            string SQL = "select * from [Groups]";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getGroupList(IntDatabase managementDB, string category)
        {
            string SQL = "select * from [Groups] where [category]=@category;";
            Tree parms = new Tree();
            parms.addElement("@category", category);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.dispose();
            return dt;
        }

        public static DataTable getGroupList(IntDatabase managementDB, string category, string subcategory)
        {
            string SQL = "select * from [Groups] where [category]=@category and subcategory=@subcategory;";
            Tree parms = new Tree();
            parms.addElement("@category", category);
            parms.addElement("@subcategory", subcategory);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.dispose();
            return dt;
        }

        static public BaseGroup loadGroupByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            BaseGroup result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Groups] where [UniqueID]=@uid;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select * from [Groups] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseGroup newGroup = new BaseGroup();
                newGroup.DateAdded = row["DateAdded"].ToString();
                newGroup.Name = row["Name"].ToString();
                newGroup.Category = row["Category"].ToString();
                newGroup.Subcategory = row["Subcategory"].ToString();
                newGroup.UniqueID = row["UniqueID"].ToString();
                newGroup.OwnerID = row["OwnerID"].ToString();
                newGroup.GroupID = row["GroupID"].ToString();
                newGroup.Origin = row["Origin"].ToString();
                result = newGroup;
            }
            return result;
        }
    }
}
