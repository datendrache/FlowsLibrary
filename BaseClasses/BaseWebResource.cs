//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Data;
using System.Collections;
using DatabaseAdapters;
using FatumCore;
using System.IO;

namespace PhlozLib
{
    public class BaseWebResource
    {
        public string DateAdded = "";
        public string Name = "";   // Alarm or Document
        public string Description = "";
        public string NetworkType = "";
        public string URI = "";
        public string UniqueID = "";


        ~BaseWebResource()
        {
            DateAdded = null;
            Name = null;
            Description = null;
            NetworkType = null;
            UniqueID = null;
            URI = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [WebResources](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[NetworkType] TEXT NULL, " +
                    "[URI] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [WebResources](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Name] NVARCHAR(200) NULL, " +
                    "[Description] NVARCHAR(MAX) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[NetworkType] VARCHAR(33) NULL, " +
                    "[URI] NVARCHAR(500) NULL );";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_WebResourcesUniqueId ON WebResources([UniqueID]);";
                    database.ExecuteNonQuery(configDB); 
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_WebResourcesUniqueId ON WebResources([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void removeWebResourceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [WebResources] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updateWebResource(IntDatabase managementDB, BaseWebResource webResource)
        {
            if (webResource.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Name", webResource.Name);
                data.addElement("Description", webResource.Description);
                data.addElement("NetworkType", webResource.NetworkType);
                data.addElement("URI", webResource.URI);
                data.addElement("*@UniqueID", webResource.UniqueID);
                managementDB.UpdateTree("[WebResources]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [WebResources] ([DateAdded], [Name], [Description], [UniqueID], [NetworkType], [URI]) VALUES (@DateAdded, @Name, @Description, @UniqueID, @NetworkType, @URI);";
                    
                Tree newWebResource = new Tree();
                newWebResource.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
                newWebResource.addElement("@Name", webResource.Name);
                newWebResource.addElement("@Description", webResource.Description);
                webResource.UniqueID = "?" + System.Guid.NewGuid().ToString().Replace("-", "");
                newWebResource.addElement("@UniqueID", webResource.UniqueID);
                newWebResource.addElement("@NetworkType", webResource.NetworkType);
                newWebResource.addElement("@URI", webResource.URI);
                managementDB.ExecuteDynamic(sql, newWebResource);
                newWebResource.dispose();
            }
        }

        static public void addForwarderLink(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [WebResources] ([DateAdded], [Name], [Description], [UniqueID], [NetworkType], [URI]) VALUES (@DateAdded, @Name, @Description, @UniqueID, @NetworkType, @URI);";

            Tree NewForwarderLink = new Tree();
            NewForwarderLink.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewForwarderLink.addElement("@Name", description.getElement("Name"));
            NewForwarderLink.addElement("@Description", description.getElement("Description"));
            NewForwarderLink.addElement("@UniqueID", description.getElement("UniqueID"));
            NewForwarderLink.addElement("@NetworkType", description.getElement("NetworkType"));
            NewForwarderLink.addElement("@URI", description.getElement("URI"));
            managementDB.ExecuteDynamic(sql, NewForwarderLink);
            NewForwarderLink.dispose();
        }

        static public string getXML(BaseWebResource current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "WebResource");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseWebResource current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("Name", current.Name);
            tmp.addElement("Description", current.Description);
            tmp.addElement("NetworkType", current.NetworkType);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("URI", current.URI);
            return tmp;
        }

        static public ArrayList loadWebResources(CollectionState State)
        {
            return loadWebResources(State.managementDB);
        }

        static public ArrayList loadWebResources(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [WebResources];";
            processors = managementDB.Execute(query);

            ArrayList tmpForwarders = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseWebResource newRule = new BaseWebResource();
                newRule.DateAdded = row["DateAdded"].ToString();
                newRule.Name = row["Name"].ToString();
                newRule.Description = row["Description"].ToString();
                newRule.UniqueID = row["UniqueID"].ToString();
                newRule.NetworkType = row["NetworkType"].ToString();
                newRule.URI = row["URI"].ToString();
                tmpForwarders.Add(newRule);
            }
            return tmpForwarders;
        }

        public static DataTable getWebResources(IntDatabase managementDB)
        {
            string SQL = "select * from WebResources;";
            Tree parms = new Tree();
            DataTable dt = managementDB.ExecuteDynamic(SQL, parms);
            parms.dispose();
            return dt;
        }

        static public BaseWebResource loadWebResourcesbyUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable webResources;
            String query = "select * from [WebResources] where UniqueID=@UniqueID;";
            Tree parms = new Tree();
            parms.addElement("@UniqueID", uniqueid);
            webResources = managementDB.ExecuteDynamic(query,parms);
            parms.dispose();

            BaseWebResource newWebResource = null;
            foreach (DataRow row in webResources.Rows)
            {
                newWebResource = new BaseWebResource();
                newWebResource.DateAdded = row["DateAdded"].ToString();
                newWebResource.Name = row["Name"].ToString();
                newWebResource.Description = row["Description"].ToString();
                newWebResource.UniqueID = row["UniqueID"].ToString();
                newWebResource.NetworkType = row["NetworkType"].ToString();
                newWebResource.URI = row["URI"].ToString();
                break;
            }
            return newWebResource;
        }
    }
}
