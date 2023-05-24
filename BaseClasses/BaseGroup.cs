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

using System.Collections;
using System.Data;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void updateGroup(IntDatabase managementDB, BaseGroup group)
        {
            if (group.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Name", group.Name);
                data.AddElement("Category", group.Category);
                data.AddElement("Subcategory", group.Subcategory);
                data.AddElement("OwnerID", group.OwnerID);
                data.AddElement("GroupID", group.GroupID);
                data.AddElement("Origin", group.Origin);
                data.AddElement("*@UniqueID", group.UniqueID);
                managementDB.UpdateTree("[Groups]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Groups] ([DateAdded], [Name], [Category], [Subcategory], [UniqueID], [OwnerID], [GroupID], [Origin]) VALUES (@DateAdded, @Name, @Category, @Subcategory, @UniqueID, @OwnerID, @GroupID, @Origin);";
                Tree NewGroup = new Tree();
                group.DateAdded = DateTime.Now.Ticks.ToString();
                NewGroup.AddElement("@DateAdded", group.DateAdded);
                NewGroup.AddElement("@Name", group.Name);
                NewGroup.AddElement("@Category", group.Category);
                NewGroup.AddElement("@Subcategory", group.Subcategory);
                group.UniqueID = "G" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewGroup.AddElement("@UniqueID", group.UniqueID);
                NewGroup.AddElement("@GroupID", group.GroupID);
                NewGroup.AddElement("@OwnerID", group.OwnerID);
                NewGroup.AddElement("@Origin", group.Origin);
                managementDB.ExecuteDynamic(sql, NewGroup);
                NewGroup.Dispose();
            }
        }

        static public void addGroup(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [Groups] ([DateAdded], [Name], [Category], [Subcategory], [UniqueID], [OwnerID], [GroupID], [Origin]) VALUES (@DateAdded, @Name, @Category, @Subcategory, @UniqueID, @OwnerID, @GroupID, @Origin);";
            Tree NewGroup = new Tree();
            NewGroup.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewGroup.AddElement("@Name", description.GetElement("Name"));
            NewGroup.AddElement("@Category", description.GetElement("Category"));
            NewGroup.AddElement("@Subcategory", description.GetElement("Subcategory"));
            NewGroup.AddElement("@UniqueID", description.GetElement("UniqueID"));
            NewGroup.AddElement("@GroupID", description.GetElement("GroupID"));
            NewGroup.AddElement("@OwnerID", description.GetElement("OwnerID"));
            NewGroup.AddElement("@Origin", description.GetElement("Origin"));
            managementDB.ExecuteDynamic(sql, NewGroup);
            NewGroup.Dispose();
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
            TreeDataAccess.WriteXML(outs, tmp, "BaseGroup");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseGroup current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("Category", current.Category);
            tmp.AddElement("Subcategory", current.Subcategory);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);
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
            parms.AddElement("@category", category);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.Dispose();
            return dt;
        }

        public static DataTable getGroupList(IntDatabase managementDB, string category, string subcategory)
        {
            string SQL = "select * from [Groups] where [category]=@category and subcategory=@subcategory;";
            Tree parms = new Tree();
            parms.AddElement("@category", category);
            parms.AddElement("@subcategory", subcategory);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.Dispose();
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
            parms.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
