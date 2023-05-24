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
                ExtractedMetadata.Dispose();
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
                    newPackage.ExtractedMetadata = XMLTree.ReadXmlFromString(newPackage.Metadata);
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
                data.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(package.ExtractedMetadata,"BaseParameter"),package.OwnerID));
                data.AddElement("OwnerID", package.OwnerID);
                data.AddElement("GroupID", package.GroupID);
                data.AddElement("Description", package.Description);
                data.AddElement("Name", package.Name);
                data.AddElement("BuildNumber", package.BuildNumber.ToString());
                data.AddElement("*@UniqueID", package.UniqueID);
                managementDB.UpdateTree("[Package]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree newParameter = new Tree();
                newParameter.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                newParameter.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(package.ExtractedMetadata, "BaseParameter"), package.OwnerID));
                newParameter.AddElement("UniqueID", package.UniqueID);
                newParameter.AddElement("OwnerID", package.OwnerID);
                newParameter.AddElement("GroupID", package.GroupID);
                newParameter.AddElement("Name", package.Name);
                newParameter.AddElement("Description", package.Description);
                newParameter.AddElement("BuildNumber", package.BuildNumber.ToString());
                managementDB.InsertTree("[Package]", newParameter);
                newParameter.Dispose();
            }
        }

        static public void createNewPackage(IntDatabase managementDB, BasePackage package)
        {
            if (package.ExtractedMetadata == null) package.ExtractedMetadata = new Tree();

            Tree newParameter = new Tree();
            newParameter.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            newParameter.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(package.ExtractedMetadata, "BaseParameter"), package.OwnerID));
            newParameter.AddElement("UniqueID", package.UniqueID);
            newParameter.AddElement("OwnerID", package.OwnerID);
            newParameter.AddElement("GroupID", package.GroupID);
            newParameter.AddElement("Name", package.Name);
            newParameter.AddElement("Description", package.Description);
            newParameter.AddElement("BuildNumber", package.BuildNumber.ToString());
            managementDB.InsertTree("[Package]", newParameter);
            newParameter.Dispose();
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
            parms.AddElement("@uid", uniqueid);
            packages = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
                    newPackage.ExtractedMetadata = XMLTree.ReadXmlFromString(newPackage.Metadata);
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

            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("Metadata", current.Metadata);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("BuildNumber", current.BuildNumber.ToString());

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BasePackage");
            tmp.Dispose();
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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }
    }
}
