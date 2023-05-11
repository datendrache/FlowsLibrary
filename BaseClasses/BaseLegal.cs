using System;
using System.Collections.Generic;
using FatumCore;
using System.Collections;
using System.Data;
using System.IO;
using DatabaseAdapters;
using PhlozLanguages;
using System.ServiceModel.Configuration;

namespace PhlozLib
{
    public class BaseLegal
    {
        public string DateAdded = "";
        public string Name = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string OwnerID = "";
        public string NetworkID = "";
        public string InstanceID = "";
        public string TransactionID = "";
        public string DocumentID = "";
        public Boolean Accepted = false;

        ~BaseLegal()
        {
            DateAdded = null;
            Name = null;
            UniqueID = null;
            GroupID = null;
            OwnerID = null;
            NetworkID = null;
            InstanceID = null;
            TransactionID = null;
            DocumentID = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Legal](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[Name] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[NetworkID] TEXT NULL, " +
                    "[InstanceID] TEXT NULL, " +
                    "[Accepted] TEXT NULL, " +
                    "[DocumentID] TEXT NULL, " +
                    "[TransactionID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Legal](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[Name] NVARCHAR(50) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[NetworkID] VARCHAR(256) NULL, " +
                    "[InstanceID] VARCHAR(33) NULL, " +
                    "[Accepted] VARCHAR(10) NULL, " +
                    "[DocumentID] VARCHAR(33) NULL, " +
                    "[TransactionID] VARCHAR(33) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);
        }

        static public void recordLegal(IntDatabase managementDB, BaseLegal legal)
        {
            Tree NewLegal = new Tree();
            NewLegal.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            NewLegal.addElement("_DateAdded", "BIGINT");
            NewLegal.addElement("Name", legal.Name);
            NewLegal.addElement("GroupID", legal.GroupID);
            NewLegal.addElement("OwnerID", legal.OwnerID);
            NewLegal.addElement("NetworkID", legal.NetworkID);
            NewLegal.addElement("InstanceID", legal.InstanceID);
            NewLegal.addElement("TransactionID", legal.TransactionID);
            NewLegal.addElement("DocumentID", legal.DocumentID);
            legal.UniqueID = "N" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewLegal.addElement("UniqueID", legal.UniqueID);
            if (legal.Accepted)
            {
                NewLegal.addElement("Accepted", "true");
            }
            else
            {
                NewLegal.addElement("Accepted", "false");
            }
            managementDB.InsertTree("[Legal]", NewLegal);
            NewLegal.dispose();
        }

        static public BaseLegal loadLegalByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseLegal result = null;

            String query = "select * from [Legal] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseLegal newTask = new BaseLegal();
                newTask.Name = row["Name"].ToString();
                newTask.DateAdded = row["DateAdded"].ToString();
                newTask.OwnerID = row["OwnerID"].ToString();
                newTask.UniqueID = row["UniqueID"].ToString();
                newTask.NetworkID = row["NetworkID"].ToString();
                newTask.GroupID = row["GroupID"].ToString();
                newTask.InstanceID = row["InstanceID"].ToString();
                newTask.TransactionID= row["TransactionID"].ToString();
                newTask.DocumentID = row["DocumentID"].ToString();
                if (row["accepted"].ToString().ToLower()=="true")
                {
                    newTask.Accepted = true;
                }
                else
                {
                    newTask.Accepted = false;
                }
                result = newTask;
            }
            return result;
        }
    }
}
