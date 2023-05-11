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
    public class BaseParameter
    {
        public string Name = "";
        public string DateAdded = "";
        public string Metadata = "";
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Origin = "";
        public string Description = "";
        public Tree ExtractedMetadata = null;

        ~BaseParameter()
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

        static public void removeParameterByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Parameter] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public ArrayList loadParameters(IntDatabase managementDB)
        {
            DataTable parameters;
            String query = "select * from [Parameter];";
            parameters = managementDB.Execute(query);

            ArrayList tmpCredentials = new ArrayList();

            foreach (DataRow row in parameters.Rows)
            {
                BaseParameter newParameter = new BaseParameter();
                newParameter.DateAdded = row["DateAdded"].ToString();
                newParameter.OwnerID = row["OwnerID"].ToString();
                newParameter.Metadata = FatumLib.Unscramble(row["Metadata"].ToString(), newParameter.OwnerID);
                newParameter.GroupID = row["GroupID"].ToString();
                newParameter.UniqueID = row["UniqueID"].ToString();
                newParameter.Name = row["Name"].ToString();
                newParameter.Description = row["Description"].ToString();
                newParameter.Origin = row["Origin"].ToString();

                tmpCredentials.Add(newParameter);
                try
                {
                    newParameter.ExtractedMetadata = XMLTree.readXMLFromString(newParameter.Metadata);
                }
                catch (Exception xyz)
                {
                    newParameter.ExtractedMetadata = new Tree();
                }
            }
            return tmpCredentials;
        }

        static public void updateParameter(IntDatabase managementDB, BaseParameter parameter)
        {
            if (parameter.ExtractedMetadata == null) parameter.ExtractedMetadata = new Tree();

            if (parameter.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(parameter.ExtractedMetadata,"BaseParameter"),parameter.OwnerID));
                data.addElement("OwnerID", parameter.OwnerID);
                data.addElement("GroupID", parameter.GroupID);
                data.addElement("Description", parameter.Description);
                data.addElement("Name", parameter.Name);
                data.addElement("Origin", parameter.Origin);
                data.addElement("*@UniqueID", parameter.UniqueID);
                managementDB.UpdateTree("[Parameter]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree newParameter = new Tree();
                newParameter.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                newParameter.addElement("Metadata", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(parameter.ExtractedMetadata, "BaseParameter"), parameter.OwnerID));
                parameter.UniqueID = "J" + System.Guid.NewGuid().ToString().Replace("-", "");
                newParameter.addElement("UniqueID", parameter.UniqueID);
                newParameter.addElement("OwnerID", parameter.OwnerID);
                newParameter.addElement("GroupID", parameter.GroupID);
                newParameter.addElement("Name", parameter.Name);
                newParameter.addElement("Description", parameter.Description);
                newParameter.addElement("Origin", parameter.Origin);
                managementDB.InsertTree("[Parameter]", newParameter);
                newParameter.dispose();
            }
        }

        static public void addParameter(IntDatabase managementDB, Tree description)
        {
            Tree newParameter = new Tree();
            newParameter.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            newParameter.addElement("Metadata", description.getElement("Metadata"));
            newParameter.addElement("UniqueID", description.getElement("UniqueID"));
            newParameter.addElement("OwnerID", description.getElement("OwnerID"));
            newParameter.addElement("GroupID", description.getElement("GroupID"));
            newParameter.addElement("Name", description.getElement("Name"));
            newParameter.addElement("Description", description.getElement("Description"));
            newParameter.addElement("Origin", description.getElement("Origin"));
            managementDB.InsertTree("[Parameter]", newParameter);
            newParameter.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Parameter](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[Metadata] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Description] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Parameter](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Name] NVARCHAR(100) NULL, " +
                    "[Metadata] NVARCHAR(MAX) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(512) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseparameter ON Parameter([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseparameter ON Parameter([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public BaseParameter loadParameterByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            BaseParameter result = null;

            String query = "select * from [Parameter] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseParameter newParameter = new BaseParameter();
                newParameter.Name = row["Name"].ToString();
                newParameter.DateAdded = row["DateAdded"].ToString();
                newParameter.OwnerID = row["OwnerID"].ToString();
                newParameter.Metadata = FatumLib.Unscramble(row["Metadata"].ToString(), newParameter.OwnerID);
                newParameter.UniqueID = row["UniqueID"].ToString();
                newParameter.Description = row["Description"].ToString();
                newParameter.OwnerID = row["OwnerID"].ToString();
                newParameter.GroupID = row["GroupID"].ToString();
                newParameter.Origin = row["Origin"].ToString();

                try
                {
                    newParameter.ExtractedMetadata = XMLTree.readXMLFromString(newParameter.Metadata);
                }
                catch (Exception xyz)
                {
                    newParameter.ExtractedMetadata = new Tree();
                }
                result = newParameter;
            }
            return result;
        }

        static public string getXML(BaseParameter current)
        {
            string result = "";
            Tree tmp = getTree(current);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseParameter");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseParameter current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("Name", current.Name);
            tmp.addElement("Metadata", current.Metadata);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Description", current.Description);
            tmp.addElement("Origin", current.Origin);
            return tmp;

        }
        static public DataTable getParameters(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Parameter];";
            services = managementDB.Execute(squery);
            return services;
        }
    }
}
