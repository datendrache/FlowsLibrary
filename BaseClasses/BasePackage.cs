//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using DatabaseAdapters;
using Fatum.FatumCore;

namespace PhlozLib
{
    public class BasePackage
    {
        public string Name = "";
        public string DateAdded = "";
        public int BuildNumber;
        public string Metadata = "";
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Description = "";
        public Tree ExtractedMetadata = null;

        ~BasePackage()
        {
            Name = null;
            DateAdded = null;
            Metadata = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            Description = null;

            if (ExtractedMetadata != null)
            {
                ExtractedMetadata.dispose();
                ExtractedMetadata = null;
            }
        }

        static public ArrayList loadPackages(IntDatabase managementDB)
        {
            DataTable packages;
            String query = "select * from [Packages];";
            packages = managementDB.Execute(query);

            ArrayList tmpCredentials = new ArrayList();

            foreach (DataRow row in packages.Rows)
            {
                BasePackage newPackage = new BasePackage();
                newPackage.DateAdded = row["DateAdded"].ToString();
                newPackage.OwnerID = row["OwnerID"].ToString();
                newPackage.Metadata = FatumLib.Unscramble(row["Metadata"].ToString(), newPackage.OwnerID);
                newPackage.GroupID = row["GroupID"].ToString();
                newPackage.UniqueID = row["UniqueID"].ToString();
                newPackage.Name = row["Name"].ToString();
                newPackage.Description = row["Description"].ToString();
                newPackage.BuildNumber = Convert.ToInt32(row["BuildNumber"]);

                tmpCredentials.Add(newPackage);
                try
                {
                    newPackage.ExtractedMetadata = XMLTree.readXMLFromString(newPackage.Metadata);
                }
                catch (Exception xyz)
                {
                    newPackage.ExtractedMetadata = new Tree();
                }
            }
            return tmpCredentials;
        }

        static public void updatePackage(IntDatabase managementDB, BasePackage package)
        {
            if (package.ExtractedMetadata == null) package.ExtractedMetadata = new Tree();

            if (package.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(package.ExtractedMetadata,"BaseParameter"),package.OwnerID));
                data.addElement("OwnerID", package.OwnerID);
                data.addElement("GroupID", package.GroupID);
                data.addElement("Description", package.Description);
                data.addElement("Name", package.Name);
                data.addElement("BuildNumber", package.BuildNumber.ToString());
                data.addElement("*@UniqueID", package.UniqueID);
                managementDB.UpdateTree("[Package]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree newParameter = new Tree();
                newParameter.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                newParameter.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(package.ExtractedMetadata, "BaseParameter"), package.OwnerID));
                newParameter.addElement("UniqueID", package.UniqueID);
                newParameter.addElement("OwnerID", package.OwnerID);
                newParameter.addElement("GroupID", package.GroupID);
                newParameter.addElement("Name", package.Name);
                newParameter.addElement("Description", package.Description);
                newParameter.addElement("BuildNumber", package.BuildNumber.ToString());
                managementDB.InsertTree("[Package]", newParameter);
                newParameter.dispose();
            }
        }

        static public void createNewPackage(IntDatabase managementDB, BasePackage package)
        {
            if (package.ExtractedMetadata == null) package.ExtractedMetadata = new Tree();

            Tree newParameter = new Tree();
            newParameter.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            newParameter.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(package.ExtractedMetadata, "BaseParameter"), package.OwnerID));
            newParameter.addElement("UniqueID", package.UniqueID);
            newParameter.addElement("OwnerID", package.OwnerID);
            newParameter.addElement("GroupID", package.GroupID);
            newParameter.addElement("Name", package.Name);
            newParameter.addElement("Description", package.Description);
            newParameter.addElement("BuildNumber", package.BuildNumber.ToString());
            managementDB.InsertTree("[Package]", newParameter);
            newParameter.dispose();
        }
        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Package](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[Metadata] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[BuildNumber] INTEGER NULL, " +
                    "[Description] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Package](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Name] NVARCHAR(100) NULL, " +
                    "[Metadata] NVARCHAR(MAX) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[BuildNumber] INT NULL, " +
                    "[Description] NVARCHAR(512) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public BasePackage loadPackageByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable packages;
            BasePackage result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Package] where [UniqueID]=@uid;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select * from [Package] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            packages = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in packages.Rows)
            {
                BasePackage newPackage = new BasePackage();
                newPackage.Name = row["Name"].ToString();
                newPackage.DateAdded = row["DateAdded"].ToString();
                newPackage.OwnerID = row["OwnerID"].ToString();
                newPackage.Metadata = FatumLib.Unscramble(row["Metadata"].ToString(), newPackage.OwnerID);
                newPackage.UniqueID = row["UniqueID"].ToString();
                newPackage.Description = row["Description"].ToString();
                newPackage.OwnerID = row["OwnerID"].ToString();
                newPackage.GroupID = row["GroupID"].ToString();
                newPackage.BuildNumber = Convert.ToInt32(row["BuildNumber"]);

                try
                {
                    newPackage.ExtractedMetadata = XMLTree.readXMLFromString(newPackage.Metadata);
                }
                catch (Exception xyz)
                {
                    newPackage.ExtractedMetadata = new Tree();
                }
                result = newPackage;
            }
            return result;
        }

        static public string getXML(BasePackage current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("Name", current.Name);
            tmp.addElement("Metadata", current.Metadata);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Description", current.Description);
            tmp.addElement("BuildNumber", current.BuildNumber.ToString());

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BasePackage");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public DataTable getPackages(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Package];";
            services = managementDB.Execute(squery);
            return services;
        }

        static public void removePackageByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Package] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }
    }
}
