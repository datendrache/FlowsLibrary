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

using Proliferation.Fatum;
using DatabaseAdapters;
using System.Data;
using System.Collections;

namespace Proliferation.Flows
{
    public class BaseInstance
    {
        public DateTime DateAdded;
        public String InstanceName = "";
        public String UniqueID = "";
        public String InstanceType = "";
        public String OwnerID = "";
        public String LastCommunication = "";
        public String License = "";
        public String Certificate = "";
        public String Description = "";
        public String Version = "";
        public String GroupID = "";
        public String Host = "";
        public String Status = "";
        public String Hostname = "";

        public BaseInstance()
        {

        }

        static public void removeInstanceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Instances] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        public static void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Instances] ( " +
                        "[DateAdded] INTEGER NULL, " +
                        "[InstanceName] TEXT NULL, " +
                        "[UniqueID] TEXT NULL, " +
                        "[InstanceType] TEXT NULL, " +
                        "[OwnerID] TEXT NULL, " +
                        "[GroupID] TEXT NULL, " +
                        "[LastCommunication] TEXT NULL, " +
                        "[License] TEXT NULL, " +
                        "[Certificate] TEXT NULL, " +
                        "[Version] TEXT NULL, " +
                        "[Host] TEXT NULL, " +
                        "[Status] TEXT NULL, " +
                        "[Description] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Instances] (" +
                        "[DateAdded] BIGINT NULL, " +
                        "[InstanceName] NVARCHAR(100) NULL, " +
                        "[UniqueID] VARCHAR(33) NULL, " +
                        "[InstanceType] VARCHAR(20) NULL, " +
                        "[OwnerID] VARCHAR(33) NULL, " +
                        "[GroupID] VARCHAR(33) NULL, " +
                        "[LastCommunication] VARCHAR(22) NULL, " +
                        "[License] VARCHAR(512) NULL, " +
                        "[Certificate] VARCHAR(512) NULL, " +
                        "[Version] VARCHAR(50) NULL, " +
                        "[Host] NVARCHAR(256) NULL, " +
                        "[Status] VARCHAR(20) NULL, " +
                        "[Description] NVARCHAR(MAX) NULL );";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseinstance ON Instances([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseinstance ON Instances([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void updateInstance(IntDatabase managementDB, BaseInstance instance)
        {
            if (instance.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("InstanceName", instance.InstanceName);
                data.AddElement("InstanceType", instance.InstanceType);
                data.AddElement("OwnerID", instance.OwnerID);
                data.AddElement("GroupID", instance.GroupID);
                data.AddElement("License", instance.License);
                data.AddElement("Certificate", instance.Certificate);
                data.AddElement("Description", instance.Description);
                data.AddElement("Version", instance.Version);
                data.AddElement("Host", instance.Host);
                data.AddElement("Status", instance.Status);
                data.AddElement("*@UniqueID", instance.UniqueID);
                managementDB.UpdateTree("[Instances]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree data = new Tree();
                data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.AddElement("_DateAdded", "BIGINT");
                data.AddElement("InstanceName", instance.InstanceName);
                if (instance.UniqueID=="")
                {
                    instance.UniqueID= "I" + System.Guid.NewGuid().ToString().Replace("-", "");
                }
                data.AddElement("UniqueID", instance.UniqueID);
                data.AddElement("InstanceType", instance.InstanceType);
                data.AddElement("OwnerID", instance.OwnerID);
                data.AddElement("GroupID", instance.GroupID);
                data.AddElement("LastCommunication", DateTime.MinValue.ToString());
                data.AddElement("License", instance.License);
                data.AddElement("Certificate", instance.Certificate);
                data.AddElement("Description", instance.Description);
                data.AddElement("Version", instance.Version);
                data.AddElement("Host", instance.Host);
                data.AddElement("Status", instance.Status);
                managementDB.InsertTree("[Instances]", data);
                data.Dispose();
            }
        }

        internal static ArrayList getInstances(IntDatabase managementDB)
        {
            DataTable sources;
            String squery = "select * from [Instances];";
            sources = managementDB.Execute(squery);

            ArrayList tmpInstance = new ArrayList();
            foreach (DataRow row in sources.Rows)
            {
                BaseInstance newSource = new BaseInstance();
                newSource.DateAdded = new DateTime(Convert.ToInt64(row["DateAdded"]));
                newSource.InstanceName = row["InstanceName"].ToString();
                newSource.UniqueID = row["UniqueID"].ToString();
                newSource.InstanceType = row["InstanceType"].ToString();
                newSource.OwnerID = row["OwnerID"].ToString();
                newSource.LastCommunication = row["LastCommunication"].ToString();
                newSource.License = row["License"].ToString();
                newSource.Certificate = row["Certificate"].ToString();
                newSource.Description = row["Description"].ToString();
                newSource.Version = row["Version"].ToString();
                newSource.GroupID = row["GroupID"].ToString();
                newSource.Host = row["Host"].ToString();
                newSource.Status = row["Status"].ToString();
                tmpInstance.Add(newSource);
            }
            return tmpInstance;
        }

        static public void createInstance(IntDatabase managementDB, BaseInstance instance)
        {
            Tree data = new Tree();
            data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            data.AddElement("_DateAdded", "BIGINT");
            data.AddElement("InstanceName", instance.InstanceName);
            data.AddElement("UniqueID", instance.UniqueID);
            data.AddElement("GroupID", instance.GroupID);
            data.AddElement("InstanceType", instance.InstanceType);
            data.AddElement("OwnerID", instance.OwnerID);
            data.AddElement("LastCommunication", DateTime.MinValue.ToString());
            data.AddElement("License", instance.License);
            data.AddElement("Certificate", instance.Certificate);
            data.AddElement("Description", instance.Description);
            data.AddElement("Version", instance.Version);
            data.AddElement("Host", instance.Host);
            data.AddElement("Status", instance.Status);
            managementDB.InsertTree("[Instances]", data);
            data.Dispose();
        }

        public static DataTable getInstanceList(IntDatabase managementDB)
        {
            string SQL = "select * from [Instances];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getInstanceListCompact(IntDatabase managementDB)
        {
            string SQL = "select [UniqueID], [InstanceName] from [Instances];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getInstanceSearchableList(IntDatabase managementDB)
        {
            string SQL = "select [UniqueID], [Host] from [Instances] where [Status]='Active';";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getInstanceSearchableListByGroup(IntDatabase managementDB, string GroupID)
        {
            string SQL = "select inst.[UniqueID], inst.[Host] from [Instances] as inst join [Members] as memb on memb.[memberID]=inst.[UniqueID] where inst.[Status]='Active' and memb.[GroupID]='" + GroupID + "';";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static BaseInstance loadInstanceByUniqueID(IntDatabase managementDB, string instanceID)
        {
            BaseInstance instance = new BaseInstance();
            string SQL = "select * from [Instances] where [uniqueid]=@uniqueid;";
            Tree parameters = new Tree();
            parameters.AddElement("@uniqueid", instanceID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parameters);
            DataRow dr = null;
            if (dt.Rows.Count > 0)
            {
                dr = dt.Rows[0];
            }
            if (dr != null)
            {
                instance.DateAdded = new DateTime(Convert.ToInt64(dr["DateAdded"]));
                instance.InstanceName = dr["InstanceName"].ToString();
                instance.UniqueID = dr["UniqueID"].ToString();
                instance.InstanceType = dr["InstanceType"].ToString();
                instance.OwnerID = dr["OwnerID"].ToString();
                instance.LastCommunication = dr["LastCommunication"].ToString();
                instance.License = dr["License"].ToString();
                instance.Certificate = dr["Certificate"].ToString();
                instance.Description = dr["Description"].ToString();
                instance.Version = dr["Version"].ToString();
                instance.GroupID = dr["GroupID"].ToString();
                instance.Host = dr["Host"].ToString();
                instance.Status = dr["Status"].ToString();
                return instance;
            }
            else
            {
                return null;
            }
        }
    }
}
