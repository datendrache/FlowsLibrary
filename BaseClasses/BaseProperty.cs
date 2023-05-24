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

using System.Data;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseProperty
    {
        public string Key = "";
        public string Value = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string OwnerID = "";
        public string ObjectID = "";
        public string Description = "";
        public string Origin = "";

        ~BaseProperty()
        {
            Key = null;
            Value = null;
            UniqueID = null;
            GroupID = null;
            OwnerID = null;
            ObjectID = null;
            Description = null;
            Origin = null;
        }

        static public void removeProperty(IntDatabase managementDB, BaseProperty property)
        {
            Tree data = new Tree();
            data.AddElement("@uniqueid", property.UniqueID);
            managementDB.DeleteTree("[Properties]", data, "[uniqueid]=@uniqueid");
            data.Dispose();
        }


        static public string getProperty(IntDatabase managementDB, string key, string uid)
        {
            Tree data = new Tree();
            data.AddElement("@uid", uid);
            data.AddElement("@key", key);
            string SQL = "select * from [properties] where [key]=@key and [OwnerID]=@uid;";
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            if (dt.Rows.Count>0)
            {
                string result = dt.Rows[0]["Value"].ToString();
                return result;
            }
            else
            {
                return null;
            }
        }

        static public void updateProperty(IntDatabase managementDB, BaseProperty property)
        {
            if (property.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Key", property.Key);
                data.AddElement("Value", property.Value);
                data.AddElement("OwnerID", property.OwnerID);
                data.AddElement("GroupID", property.GroupID);
                data.AddElement("Description", property.Description);
                data.AddElement("Origin", property.Origin);
                data.AddElement("*@UniqueID", property.UniqueID);
                data.AddElement("*@ObjectID", property.ObjectID);
                managementDB.UpdateTree("Properties", data, "[UniqueID]=@UniqueID and [ObjectID]=@ObjectID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Properties] ([Key], [Value], [UniqueID], [ObjectID], [OwnerID], [GroupID], [Description]) VALUES (@Key, @Value, @UniqueID, @ObjectID, @OwnerID, @GroupID, @Description);";
                Tree NewPermission = new Tree();
                NewPermission.AddElement("@Key", property.Key);
                NewPermission.AddElement("@Value", property.Value);
                property.UniqueID = "X" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewPermission.AddElement("@UniqueID", property.UniqueID);
                NewPermission.AddElement("@ObjectID", property.ObjectID);
                NewPermission.AddElement("@OwnerID", property.OwnerID);
                NewPermission.AddElement("@GroupID", property.GroupID);
                NewPermission.AddElement("@Origin", property.Origin);
                NewPermission.AddElement("@Description", property.Description);
                managementDB.ExecuteDynamic(sql, NewPermission);
                NewPermission.Dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Properties](" +
                    "[Key] TEXT NULL, " +
                    "[Value] TEXT NULL, " +
                    "[ObjectID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[UniqueID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Properties](" +
                    "[Key] NVARCHAR(100) NULL, " +
                    "[Value] NVARCHAR(MAX) NULL, " +
                    "[ObjectID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(MAX) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseproperties ON Properties([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseproperties ON Properties([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getXML(BaseProperty current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("Key", current.Key);
            tmp.AddElement("Value", current.Value);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("ObjectID", current.ObjectID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.GroupID);
            tmp.AddElement("Description", current.Description);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseProperty");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        public static DataTable getPropertyList(IntDatabase managementDB)
        {
            string SQL = "select * from [Properties];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static BaseProperty loadPropertyByUniqueID(IntDatabase managementDB, string propertyid)
        {
            Tree data = new Tree();
            data.AddElement("@uniqueid", propertyid);
            string SQL = "select * from [properties] where [uniqueid]=@uniqueid;";
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();

            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                BaseProperty newProperty = new BaseProperty();
                newProperty.Description = dr["Description"].ToString();
                newProperty.Key = dr["key"].ToString();
                newProperty.Value = dr["value"].ToString();
                newProperty.OwnerID = dr["ownerid"].ToString();
                newProperty.GroupID = dr["groupid"].ToString();
                newProperty.ObjectID = dr["objectid"].ToString();
                newProperty.Origin = dr["Origin"].ToString();
                newProperty.UniqueID = propertyid;

                return newProperty;
            }
            else
            {
                return null;
            }
        }

        public static BaseProperty loadPropertyByKey(IntDatabase managementDB, string key)
        {
            Tree data = new Tree();
            data.AddElement("@key", key);
            string SQL = "select * from [properties] where [key]=@key;";
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();

            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                BaseProperty newProperty = new BaseProperty();
                newProperty.Description = dr["Description"].ToString();
                newProperty.Key = dr["key"].ToString();
                newProperty.Value = dr["value"].ToString();
                newProperty.OwnerID = dr["ownerid"].ToString();
                newProperty.GroupID = dr["groupid"].ToString();
                newProperty.ObjectID = dr["objectid"].ToString();
                newProperty.UniqueID = dr["UniqueID"].ToString();
                newProperty.Origin = dr["Origin"].ToString();
                return newProperty;
            }
            else
            {
                return null;
            }
        }
    }
}
