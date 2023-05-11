using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatumCore;
using DatabaseAdapters;
using PhlozLib;
using System.Data;
using System.Collections;

namespace PhlozLib
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
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
                data.addElement("InstanceName", instance.InstanceName);
                data.addElement("InstanceType", instance.InstanceType);
                data.addElement("OwnerID", instance.OwnerID);
                data.addElement("GroupID", instance.GroupID);
                data.addElement("License", instance.License);
                data.addElement("Certificate", instance.Certificate);
                data.addElement("Description", instance.Description);
                data.addElement("Version", instance.Version);
                data.addElement("Host", instance.Host);
                data.addElement("Status", instance.Status);
                data.addElement("*@UniqueID", instance.UniqueID);
                managementDB.UpdateTree("[Instances]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree data = new Tree();
                data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.addElement("_DateAdded", "BIGINT");
                data.addElement("InstanceName", instance.InstanceName);
                if (instance.UniqueID=="")
                {
                    instance.UniqueID= "I" + System.Guid.NewGuid().ToString().Replace("-", "");
                }
                data.addElement("UniqueID", instance.UniqueID);
                data.addElement("InstanceType", instance.InstanceType);
                data.addElement("OwnerID", instance.OwnerID);
                data.addElement("GroupID", instance.GroupID);
                data.addElement("LastCommunication", DateTime.MinValue.ToString());
                data.addElement("License", instance.License);
                data.addElement("Certificate", instance.Certificate);
                data.addElement("Description", instance.Description);
                data.addElement("Version", instance.Version);
                data.addElement("Host", instance.Host);
                data.addElement("Status", instance.Status);
                managementDB.InsertTree("[Instances]", data);
                data.dispose();
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
            data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            data.addElement("_DateAdded", "BIGINT");
            data.addElement("InstanceName", instance.InstanceName);
            data.addElement("UniqueID", instance.UniqueID);
            data.addElement("GroupID", instance.GroupID);
            data.addElement("InstanceType", instance.InstanceType);
            data.addElement("OwnerID", instance.OwnerID);
            data.addElement("LastCommunication", DateTime.MinValue.ToString());
            data.addElement("License", instance.License);
            data.addElement("Certificate", instance.Certificate);
            data.addElement("Description", instance.Description);
            data.addElement("Version", instance.Version);
            data.addElement("Host", instance.Host);
            data.addElement("Status", instance.Status);
            managementDB.InsertTree("[Instances]", data);
            data.dispose();
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
            parameters.addElement("@uniqueid", instanceID);
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
