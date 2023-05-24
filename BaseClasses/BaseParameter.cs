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
                ExtractedMetadata.Dispose();
                ExtractedMetadata = null;
            }
        }

        static public void removeParameterByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Parameter] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
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
                    newParameter.ExtractedMetadata = XMLTree.ReadXmlFromString(newParameter.Metadata);
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
                data.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(parameter.ExtractedMetadata,"BaseParameter"),parameter.OwnerID));
                data.AddElement("OwnerID", parameter.OwnerID);
                data.AddElement("GroupID", parameter.GroupID);
                data.AddElement("Description", parameter.Description);
                data.AddElement("Name", parameter.Name);
                data.AddElement("Origin", parameter.Origin);
                data.AddElement("*@UniqueID", parameter.UniqueID);
                managementDB.UpdateTree("[Parameter]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree newParameter = new Tree();
                newParameter.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                newParameter.AddElement("Metadata", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(parameter.ExtractedMetadata, "BaseParameter"), parameter.OwnerID));
                parameter.UniqueID = "J" + System.Guid.NewGuid().ToString().Replace("-", "");
                newParameter.AddElement("UniqueID", parameter.UniqueID);
                newParameter.AddElement("OwnerID", parameter.OwnerID);
                newParameter.AddElement("GroupID", parameter.GroupID);
                newParameter.AddElement("Name", parameter.Name);
                newParameter.AddElement("Description", parameter.Description);
                newParameter.AddElement("Origin", parameter.Origin);
                managementDB.InsertTree("[Parameter]", newParameter);
                newParameter.Dispose();
            }
        }

        static public void addParameter(IntDatabase managementDB, Tree description)
        {
            Tree newParameter = new Tree();
            newParameter.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            newParameter.AddElement("Metadata", description.GetElement("Metadata"));
            newParameter.AddElement("UniqueID", description.GetElement("UniqueID"));
            newParameter.AddElement("OwnerID", description.GetElement("OwnerID"));
            newParameter.AddElement("GroupID", description.GetElement("GroupID"));
            newParameter.AddElement("Name", description.GetElement("Name"));
            newParameter.AddElement("Description", description.GetElement("Description"));
            newParameter.AddElement("Origin", description.GetElement("Origin"));
            managementDB.InsertTree("[Parameter]", newParameter);
            newParameter.Dispose();
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
            parms.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
                    newParameter.ExtractedMetadata = XMLTree.ReadXmlFromString(newParameter.Metadata);
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
            TreeDataAccess.WriteXML(outs, tmp, "BaseParameter");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseParameter current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("Metadata", current.Metadata);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("Origin", current.Origin);
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
