using System;
using System.Collections.Generic;
using FatumCore;
using System.Collections;
using System.Data;
using System.IO;
using DatabaseAdapters;
using PhlozLanguages;
using System.ServiceModel.Configuration;

namespace PhlozLib
{
    public class BaseLicense
    {
        public string DateAdded = "";
        public string Expiration = "";
        public string Name = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string OwnerID = "";
        public string InstanceID = "";
        public string ParameterID = "";
        public string Version = "";
        public string Description = "";

        ~BaseLicense()
        {
            DateAdded = null;
            Expiration = null;
            Name = null;
            UniqueID = null;
            GroupID = null;
            OwnerID = null;
            InstanceID = null;
            ParameterID = null;
            Description = null;
            Version = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Licenses](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Expiration] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[InstanceID] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[Version] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Licenses](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Expiration] BIGINT NULL, " +
                    "[Name] NVARCHAR(50) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[InstanceID] VARCHAR(256) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(200) NULL, " +
                    "[Version] VARCHAR(33) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);
        }

        static public void registerLicense(IntDatabase managementDB, BaseLicense license)
        {
            Tree NewLicense = new Tree();
            NewLicense.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            DateTime expire = DateTime.Now;
            expire.AddMonths(12);
            NewLicense.addElement("Expiration", expire.Ticks.ToString());
            NewLicense.addElement("Name", license.Name);
            NewLicense.addElement("GroupID", license.GroupID);
            NewLicense.addElement("OwnerID", license.OwnerID);
            NewLicense.addElement("InstanceID", license.InstanceID);
            NewLicense.addElement("ParameterID", license.ParameterID);
            NewLicense.addElement("Description", license.Description);
            license.UniqueID = "Y" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewLicense.addElement("UniqueID", license.UniqueID);
            NewLicense.addElement("Version", license.Version);
            managementDB.InsertTree("[Licenses]", NewLicense);
            NewLicense.dispose();
        }

        static public DataTable getLicensesByInstanceID(IntDatabase managementDB, string instanceid)
        {
            String query = "select * from [Licenses] where [InstanceID]=@instanceid";
            Tree data = new Tree();
            data.addElement("@instanceid", instanceid);
            return managementDB.ExecuteDynamic(query, data);
        }

        static public DataTable getLicensesByOwnerID(IntDatabase managementDB, string ownerid)
        {
            String query = "select [Licenses].DateAdded, Expiration, InstanceName, InstanceType, [Instances].Version, [Instances].Host  from [Licenses] join [Instances] on Licenses.InstanceID=Instances.UniqueID where [Licenses].ownerid=@ownerid";
            Tree data = new Tree();
            data.addElement("@ownerid", ownerid);
            return managementDB.ExecuteDynamic(query, data);
        }

        static public BaseLicense loadLicenseByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseLicense result = null;

            String query = "select * from [Licenses] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseLicense newTask = new BaseLicense();
                newTask.Name = row["Name"].ToString();
                newTask.DateAdded = row["DateAdded"].ToString();
                newTask.Expiration = row["Expiration"].ToString();
                newTask.OwnerID = row["OwnerID"].ToString();
                newTask.UniqueID = row["UniqueID"].ToString();
                newTask.GroupID = row["GroupID"].ToString();
                newTask.InstanceID = row["InstanceID"].ToString();
                newTask.ParameterID = row["ParameterID"].ToString();
                newTask.Version = row["Version"].ToString();
                newTask.Description = row["Description"].ToString();

                result = newTask;
            }
            return result;
        }
    }
}
