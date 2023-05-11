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
                ExtractedMetadata.dispose();
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
                    newCredential.ExtractedMetadata = XMLTree.readXMLFromString(FatumLib.Unscramble(newCredential.Metadata, newCredential.OwnerID));
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
                data.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(credential.ExtractedMetadata,"BaseCredential"),credential.OwnerID));
                data.addElement("CredentialType", credential.CredentialType);
                data.addElement("Name", credential.Name);
                data.addElement("OwnerID", credential.OwnerID);
                data.addElement("GroupID", credential.GroupID);
                data.addElement("Origin", credential.Origin);
                data.addElement("Description", credential.Description);
                data.addElement("Enabled", credential.Enabled.ToString());
                data.addElement("*@UniqueID", credential.UniqueID);
                managementDB.UpdateTree("[Credentials]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree newCredential = new Tree();
                newCredential.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                newCredential.addElement("CredentialType", credential.CredentialType);
                newCredential.addElement("Name", credential.Name);
                newCredential.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(credential.ExtractedMetadata, "BaseCredential"), credential.OwnerID));
                credential.UniqueID = "C" + System.Guid.NewGuid().ToString().Replace("-", "");
                newCredential.addElement("UniqueID", credential.UniqueID);
                newCredential.addElement("OwnerID", credential.OwnerID);
                newCredential.addElement("GroupID", credential.GroupID);
                newCredential.addElement("Origin", credential.Origin);
                newCredential.addElement("Description", credential.Description);
                newCredential.addElement("Enabled", credential.Enabled.ToString());
                managementDB.InsertTree("[Credentials]", newCredential);
                newCredential.dispose();
            }
        }

        static public void addCredential(IntDatabase managementDB, Tree description)
        {
            Tree newCredential = new Tree();
            newCredential.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            newCredential.addElement("CredentialType", description.getElement("CredentialType"));
            newCredential.addElement("Name", description.getElement("Name"));
            newCredential.addElement("Metadata", description.getElement("Metadata"));
            newCredential.addElement("UniqueID", description.getElement("UniqueID"));
            newCredential.addElement("OwnerID", description.getElement("OwnerID"));
            newCredential.addElement("GroupID", description.getElement("GroupID"));
            newCredential.addElement("Origin", description.getElement("Origin"));
            newCredential.addElement("Description", description.getElement("Description"));
            newCredential.addElement("Enabled", description.getElement("Enabled"));
            managementDB.InsertTree("[Credentials]", newCredential);
            newCredential.dispose();
        }

        static public void removeCredentialsByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Credentials] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
            parms.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
                    newCredential.ExtractedMetadata = XMLTree.readXMLFromString(newCredential.Metadata);
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
            TreeDataAccess.writeXML(outs, tmp, "BaseCredential");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseCredential current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("Name", current.Name);
            tmp.addElement("CredentialType", current.CredentialType);
            tmp.addElement("Metadata", current.Metadata);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("Description", current.Description);
            tmp.addElement("Enabled", current.Enabled.ToString());
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
            query.addElement("@uid", uid);
            query.addElement("@credtype", credtype);
            creds = managementDB.ExecuteDynamic(squery, query);
            query.dispose();
            return creds;
        }
    }
}
