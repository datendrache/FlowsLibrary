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
            data.addElement("@uniqueid", property.UniqueID);
            managementDB.DeleteTree("[Properties]", data, "[uniqueid]=@uniqueid");
            data.dispose();
        }


        static public string getProperty(IntDatabase managementDB, string key, string uid)
        {
            Tree data = new Tree();
            data.addElement("@uid", uid);
            data.addElement("@key", key);
            string SQL = "select * from [properties] where [key]=@key and [OwnerID]=@uid;";
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
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
                data.addElement("Key", property.Key);
                data.addElement("Value", property.Value);
                data.addElement("OwnerID", property.OwnerID);
                data.addElement("GroupID", property.GroupID);
                data.addElement("Description", property.Description);
                data.addElement("Origin", property.Origin);
                data.addElement("*@UniqueID", property.UniqueID);
                data.addElement("*@ObjectID", property.ObjectID);
                managementDB.UpdateTree("Properties", data, "[UniqueID]=@UniqueID and [ObjectID]=@ObjectID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Properties] ([Key], [Value], [UniqueID], [ObjectID], [OwnerID], [GroupID], [Description]) VALUES (@Key, @Value, @UniqueID, @ObjectID, @OwnerID, @GroupID, @Description);";
                Tree NewPermission = new Tree();
                NewPermission.addElement("@Key", property.Key);
                NewPermission.addElement("@Value", property.Value);
                property.UniqueID = "X" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewPermission.addElement("@UniqueID", property.UniqueID);
                NewPermission.addElement("@ObjectID", property.ObjectID);
                NewPermission.addElement("@OwnerID", property.OwnerID);
                NewPermission.addElement("@GroupID", property.GroupID);
                NewPermission.addElement("@Origin", property.Origin);
                NewPermission.addElement("@Description", property.Description);
                managementDB.ExecuteDynamic(sql, NewPermission);
                NewPermission.dispose();
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

            tmp.addElement("Key", current.Key);
            tmp.addElement("Value", current.Value);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("ObjectID", current.ObjectID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.GroupID);
            tmp.addElement("Description", current.Description);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseProperty");
            tmp.dispose();
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
            data.addElement("@uniqueid", propertyid);
            string SQL = "select * from [properties] where [uniqueid]=@uniqueid;";
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();

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
            data.addElement("@key", key);
            string SQL = "select * from [properties] where [key]=@key;";
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();

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
