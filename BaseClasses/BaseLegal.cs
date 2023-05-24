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
using System.Data;
using DatabaseAdapters;

namespace Proliferation.Flows
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
            NewLegal.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            NewLegal.AddElement("_DateAdded", "BIGINT");
            NewLegal.AddElement("Name", legal.Name);
            NewLegal.AddElement("GroupID", legal.GroupID);
            NewLegal.AddElement("OwnerID", legal.OwnerID);
            NewLegal.AddElement("NetworkID", legal.NetworkID);
            NewLegal.AddElement("InstanceID", legal.InstanceID);
            NewLegal.AddElement("TransactionID", legal.TransactionID);
            NewLegal.AddElement("DocumentID", legal.DocumentID);
            legal.UniqueID = "N" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewLegal.AddElement("UniqueID", legal.UniqueID);
            if (legal.Accepted)
            {
                NewLegal.AddElement("Accepted", "true");
            }
            else
            {
                NewLegal.AddElement("Accepted", "false");
            }
            managementDB.InsertTree("[Legal]", NewLegal);
            NewLegal.Dispose();
        }

        static public BaseLegal loadLegalByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseLegal result = null;

            String query = "select * from [Legal] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
