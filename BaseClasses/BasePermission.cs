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
    public class BasePermission
    {
        public string UniqueID = "";
        public string SourceID = "";
        public string DestinationID = "";
        public int Permission = 0;

        public Boolean AllowRead = false;
        public Boolean AllowUpdate = false;
        public Boolean AllowCreate = false;
        public Boolean AllowDelete = false;
        public Boolean AllowView = false;
        public Boolean AllowExecute = false;

        public ArrayList PermissionTree = null;

        public const int FULL = 0b00011111;
        public const int READONLY = 0b00010001;
        public const int READWRITE = 0b00010011;
        public const int EXECUTE = 0b00110000;
        public const int CREATEINVISIBLE = 0b00000100;
        public const int CREATEWRITE = 0b00010110;
        public const int FULLDEVELOPER = 0b00111111;
        public const int NOACCESS = 0b00000000;

        ~BasePermission()
        {
            UniqueID = null;
            SourceID = null;
            DestinationID = null;

            if (PermissionTree != null)
            {
                PermissionTree.Clear();
                PermissionTree = null;
            }
        }

        static public BasePermission loadPermission(IntDatabase managementDB, String UniqueID)
        {
            DataTable permissions;
            String query = "select * from [Permissions] where [UniqueID]=@UniqueID;";
            Tree parms = new Tree();
            parms.addElement("@UniqueID", UniqueID);
            permissions = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            ArrayList tmpProcessors = new ArrayList();

            DataRow row = permissions.Rows[0];
            BasePermission newPermission = null;
            
            new BasePermission();
            if (permissions.Rows.Count>0)
            {
                newPermission = new BasePermission();
                newPermission.UniqueID = row["UniqueID"].ToString();
                newPermission.SourceID = row["SourceID"].ToString();
                newPermission.DestinationID = row["DestinationID"].ToString();
                newPermission.Permission = Convert.ToInt32(row["Permission"]);
                updatePermissionBits(newPermission);
            }
            return newPermission;
        }

        static public void updatePermissionBits(BasePermission perm)
        {
            int permbits = perm.Permission;
            if ((permbits & 1) > 0)
            {
                perm.AllowRead = true;
            }
            if ((permbits & 2) > 0)
            {
                perm.AllowUpdate = true;
            }
            if ((permbits & 4) > 0)
            {
                perm.AllowCreate = true;
            }
            if ((permbits & 8) > 0)
            {
                perm.AllowDelete = true;
            }
            if ((permbits & 16) > 0)
            {
                perm.AllowView = true;
            }
            if ((permbits & 32) > 0)
            {
                perm.AllowExecute = true;
            }
        }

        static public void updatePermissionBits(BasePermission perm, int bits)
        {
            int result = 0;

            if (perm.AllowRead == true)
            {
                result = result + 1;
            }
            if (perm.AllowUpdate == true)
            {
                result = result + 2;
            }
            if (perm.AllowCreate == true)
            {
                result = result + 4;
            }
            if (perm.AllowDelete == true)
            {
                result = result + 8;
            }
            if (perm.AllowView == true)
            {
                result = result + 16;
            }
            if (perm.AllowExecute == true)
            {
                result = result + 32;
            }
            perm.Permission = result;
        }

        static public void removePermissionByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Permissions] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updatePermission(IntDatabase managementDB, BasePermission perm)
        {
            if (perm.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Permission", perm.Permission.ToString());
                data.addElement("_Permission", "INTEGER");
                data.addElement("*@UniqueID", perm.UniqueID);
                managementDB.UpdateTree("Permissions", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewPermission = new Tree();
                NewPermission.addElement("permission", perm.Permission.ToString());
                NewPermission.addElement("_permission", "smallint");
                perm.UniqueID = "P" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewPermission.addElement("UniqueID", perm.UniqueID);
                NewPermission.addElement("SourceID", perm.SourceID);
                NewPermission.addElement("DestinationID", perm.DestinationID);
                managementDB.InsertTree("Permissions", NewPermission);
                NewPermission.dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Permissions](" +
                    "[Permission] INTEGER NULL, " +
                    "[SourceID] TEXT NULL, " +
                    "[DestinationID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Permissions](" +
                    "[Permission] SMALLINT NULL, " +
                    "[SourceID] VARCHAR(33) NULL, " +
                    "[DestinationID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basepermissions ON Permissions([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basepermissionssource ON Permissions([SourceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basepermissions ON Permissions([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basepermissionssource ON Permissions([SourceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getXML(BasePermission current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("Permission", current.Permission.ToString());
            tmp.addElement("_Permission", "SMALLNT");
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("SourceID", current.UniqueID);
            tmp.addElement("DestinationID", current.UniqueID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BasePermission");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static BasePermission getObjectPermissions(IntDatabase managementDB, string UniqueID)
        {
            BasePermission rootPerm = new BasePermission();
            rootPerm.UniqueID = "";
            rootPerm.DestinationID = "";
            rootPerm.Permission = 0;
            rootPerm.SourceID = "";
            rootPerm.PermissionTree = new ArrayList();
            getPermissionsTree(managementDB, UniqueID, rootPerm);
            return rootPerm;
        }

        static void getPermissionsTree(IntDatabase managementDB, string UniqueID, BasePermission rootperm)
        {
            Tree parms = new Tree();
            parms.addElement("@SourceID", UniqueID);
            string SQL = "select * from [Permissions] where [SourceID]=@SourceID";
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.dispose();
            if (dt.Rows.Count>0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    BasePermission newPerm = new BasePermission();
                    newPerm.UniqueID = row["UniqueID"].ToString();
                    newPerm.DestinationID = row["ResourceID"].ToString();
                    newPerm.SourceID = UniqueID;
                    newPerm.Permission = Convert.ToInt32(row["Permission"]);
                    rootperm.PermissionTree.Add(newPerm);
                    if (newPerm.DestinationID.Substring(0,1)=="G")
                    {
                        getPermissionsTree(managementDB, UniqueID, newPerm);
                    }
                    rootperm.PermissionTree.Add(newPerm);
                }
            }
        }
    }
}
