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
    public class BaseCredential
    {
        public string DateAdded = "";
        public string Name = "";
        public string CredentialType = "";
        public string Metadata = "";
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Description = "";
        public string Origin = "";
        public Boolean Enabled = true;

        public Tree ExtractedMetadata = null;

        ~BaseCredential()
        {
            if (ExtractedMetadata != null)
            {
                ExtractedMetadata.Dispose();
                ExtractedMetadata = null;
            }
            CredentialType = null;
            Metadata = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            Description = null;
            Origin = null;
        }

        static public ArrayList loadCredentials(CollectionState state)
        {
            return loadCredentials(state.managementDB);
        }

        static public ArrayList loadCredentials(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Credentials];";
            processors = managementDB.Execute(query);

            ArrayList tmpCredentials = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseCredential newCredential = new BaseCredential();
                newCredential.DateAdded = row["DateAdded"].ToString();
                newCredential.Name = row["Name"].ToString();
                newCredential.CredentialType = row["CredentialType"].ToString();
                newCredential.OwnerID = row["OwnerID"].ToString();
                newCredential.Metadata = FatumLib.Unscramble(row["Metadata"].ToString(), newCredential.OwnerID);
                newCredential.UniqueID = row["UniqueID"].ToString();
                newCredential.GroupID = row["GroupID"].ToString();
                newCredential.Origin = row["Origin"].ToString();
                newCredential.Description = row["Description"].ToString();

                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newCredential.Enabled = true;
                }
                else
                {
                    newCredential.Enabled = false;
                }
                tmpCredentials.Add(newCredential);
                try
                {
                    newCredential.ExtractedMetadata = XMLTree.ReadXmlFromString(FatumLib.Unscramble(newCredential.Metadata, newCredential.OwnerID));
                }
                catch (Exception xyz)
                {
                    newCredential.ExtractedMetadata = new Tree();
                }
            }
            return tmpCredentials;
        }

        static public void updateCredential(IntDatabase managementDB, BaseCredential credential)
        {
            if (credential.ExtractedMetadata == null) credential.ExtractedMetadata = new Tree();

            if (credential.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(credential.ExtractedMetadata,"BaseCredential"),credential.OwnerID));
                data.AddElement("CredentialType", credential.CredentialType);
                data.AddElement("Name", credential.Name);
                data.AddElement("OwnerID", credential.OwnerID);
                data.AddElement("GroupID", credential.GroupID);
                data.AddElement("Origin", credential.Origin);
                data.AddElement("Description", credential.Description);
                data.AddElement("Enabled", credential.Enabled.ToString());
                data.AddElement("*@UniqueID", credential.UniqueID);
                managementDB.UpdateTree("[Credentials]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree newCredential = new Tree();
                newCredential.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                newCredential.AddElement("CredentialType", credential.CredentialType);
                newCredential.AddElement("Name", credential.Name);
                newCredential.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(credential.ExtractedMetadata, "BaseCredential"), credential.OwnerID));
                credential.UniqueID = "C" + System.Guid.NewGuid().ToString().Replace("-", "");
                newCredential.AddElement("UniqueID", credential.UniqueID);
                newCredential.AddElement("OwnerID", credential.OwnerID);
                newCredential.AddElement("GroupID", credential.GroupID);
                newCredential.AddElement("Origin", credential.Origin);
                newCredential.AddElement("Description", credential.Description);
                newCredential.AddElement("Enabled", credential.Enabled.ToString());
                managementDB.InsertTree("[Credentials]", newCredential);
                newCredential.Dispose();
            }
        }

        static public void addCredential(IntDatabase managementDB, Tree description)
        {
            Tree newCredential = new Tree();
            newCredential.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            newCredential.AddElement("CredentialType", description.GetElement("CredentialType"));
            newCredential.AddElement("Name", description.GetElement("Name"));
            newCredential.AddElement("Metadata", description.GetElement("Metadata"));
            newCredential.AddElement("UniqueID", description.GetElement("UniqueID"));
            newCredential.AddElement("OwnerID", description.GetElement("OwnerID"));
            newCredential.AddElement("GroupID", description.GetElement("GroupID"));
            newCredential.AddElement("Origin", description.GetElement("Origin"));
            newCredential.AddElement("Description", description.GetElement("Description"));
            newCredential.AddElement("Enabled", description.GetElement("Enabled"));
            managementDB.InsertTree("[Credentials]", newCredential);
            newCredential.Dispose();
        }

        static public void removeCredentialsByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Credentials] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Credentials](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[CredentialType] TEXT NULL, " +
                    "[Metadata] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[Enabled] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Credentials](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Name] NVARCHAR(100) NULL, " +
                    "[CredentialType] VARCHAR(40) NULL, " +
                    "[Metadata] NVARCHAR(MAX) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(512) NULL, " +
                    "[Enabled] VARCHAR(10) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basecredentials ON Credentials([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basecredentials ON Credentials([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public BaseCredential loadCredentialByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            BaseCredential result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Credentials] where [UniqueID]=@uid;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select * from [Credentials] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseCredential newCredential = new BaseCredential();
                newCredential.Name = row["Name"].ToString();
                newCredential.DateAdded = row["DateAdded"].ToString();
                newCredential.CredentialType = row["CredentialType"].ToString();
                newCredential.OwnerID = row["OwnerID"].ToString();
                newCredential.Metadata = FatumLib.Unscramble(row["Metadata"].ToString(), newCredential.OwnerID);
                newCredential.UniqueID = row["UniqueID"].ToString();
                newCredential.GroupID = row["GroupID"].ToString();
                newCredential.Origin = row["Origin"].ToString();
                newCredential.Description = row["Description"].ToString();

                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newCredential.Enabled = true;
                }
                else
                {
                    newCredential.Enabled = false;
                }
                try
                {
                    newCredential.ExtractedMetadata = XMLTree.ReadXmlFromString(newCredential.Metadata);
                }
                catch (Exception xyz)
                {
                    newCredential.ExtractedMetadata = new Tree();
                }
                result = newCredential;
            }
            return result;
        }

        static public string getXML(BaseCredential current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseCredential");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseCredential current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("CredentialType", current.CredentialType);
            tmp.AddElement("Metadata", current.Metadata);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("Enabled", current.Enabled.ToString());
            return tmp;
        }

        static public DataTable getCredentials(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Credentials];";
            services = managementDB.Execute(squery);
            return services;
        }

        static public DataTable getCredentialByOwnerByType(IntDatabase managementDB, string uid, string credtype)
        {
            DataTable creds;
            String squery = "select * from [Credentials] where ownerid=@uid and credentialtype=@credtype;";
            Tree query = new Tree();
            query.AddElement("@uid", uid);
            query.AddElement("@credtype", credtype);
            creds = managementDB.ExecuteDynamic(squery, query);
            query.Dispose();
            return creds;
        }
    }
}
