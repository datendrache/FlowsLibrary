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
using System.Collections;
using DatabaseAdapters;
using Proliferation.Fatum;

namespace Proliferation.Flows
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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void updateWebResource(IntDatabase managementDB, BaseWebResource webResource)
        {
            if (webResource.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Name", webResource.Name);
                data.AddElement("Description", webResource.Description);
                data.AddElement("NetworkType", webResource.NetworkType);
                data.AddElement("URI", webResource.URI);
                data.AddElement("*@UniqueID", webResource.UniqueID);
                managementDB.UpdateTree("[WebResources]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [WebResources] ([DateAdded], [Name], [Description], [UniqueID], [NetworkType], [URI]) VALUES (@DateAdded, @Name, @Description, @UniqueID, @NetworkType, @URI);";
                    
                Tree newWebResource = new Tree();
                newWebResource.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
                newWebResource.AddElement("@Name", webResource.Name);
                newWebResource.AddElement("@Description", webResource.Description);
                webResource.UniqueID = "?" + System.Guid.NewGuid().ToString().Replace("-", "");
                newWebResource.AddElement("@UniqueID", webResource.UniqueID);
                newWebResource.AddElement("@NetworkType", webResource.NetworkType);
                newWebResource.AddElement("@URI", webResource.URI);
                managementDB.ExecuteDynamic(sql, newWebResource);
                newWebResource.Dispose();
            }
        }

        static public void addForwarderLink(IntDatabase managementDB, Tree description)
        {
            string sql = "";
            sql = "INSERT INTO [WebResources] ([DateAdded], [Name], [Description], [UniqueID], [NetworkType], [URI]) VALUES (@DateAdded, @Name, @Description, @UniqueID, @NetworkType, @URI);";

            Tree NewForwarderLink = new Tree();
            NewForwarderLink.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
            NewForwarderLink.AddElement("@Name", description.GetElement("Name"));
            NewForwarderLink.AddElement("@Description", description.GetElement("Description"));
            NewForwarderLink.AddElement("@UniqueID", description.GetElement("UniqueID"));
            NewForwarderLink.AddElement("@NetworkType", description.GetElement("NetworkType"));
            NewForwarderLink.AddElement("@URI", description.GetElement("URI"));
            managementDB.ExecuteDynamic(sql, NewForwarderLink);
            NewForwarderLink.Dispose();
        }

        static public string getXML(BaseWebResource current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "WebResource");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseWebResource current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("NetworkType", current.NetworkType);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("URI", current.URI);
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
            parms.Dispose();
            return dt;
        }

        static public BaseWebResource loadWebResourcesbyUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable webResources;
            String query = "select * from [WebResources] where UniqueID=@UniqueID;";
            Tree parms = new Tree();
            parms.AddElement("@UniqueID", uniqueid);
            webResources = managementDB.ExecuteDynamic(query,parms);
            parms.Dispose();

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
