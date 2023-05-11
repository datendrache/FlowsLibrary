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
    public class BaseMessage
    {
        public string DateAdded = "";
        public string LastEdit = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string OwnerID = "";
        public string Document = "";
        public string ThreadID = "";
        public string Visible = "true";

        ~BaseMessage()
        {
            UniqueID = null;
            GroupID = null;
            OwnerID = null;
            Document = null;
            Visible = null;
            ThreadID = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Messages](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[LastEdit] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[ThreadID] TEXT NULL, " +
                    "[Visible] TEXT NULL, " +
                    "[Document] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Messages](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[LastEdit] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[ThreadID] VARCHAR(33) NULL, " +
                    "[Visible] VARCHAR(10) NULL, " +
                    "[Document] TEXT NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public void updateMessage(IntDatabase managementDB, BaseMessage message)
        {
            if (message.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("LastEdit", DateTime.Now.Ticks.ToString());
                data.addElement("Document", message.Document);
                data.addElement("Visible", message.Visible);
                data.addElement("*@UniqueID", message.UniqueID.ToString());
                managementDB.UpdateTree("[Messages]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewMessage = new Tree();
                NewMessage.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewMessage.addElement("GroupID", message.GroupID);
                NewMessage.addElement("OwnerID", message.OwnerID);
                NewMessage.addElement("Document", message.Document);
                message.UniqueID = "1" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewMessage.addElement("UniqueID", message.UniqueID);
                NewMessage.addElement("ThreadID", message.ThreadID);
                NewMessage.addElement("Visible", message.Visible);
                managementDB.InsertTree("[Messages]", NewMessage);
                NewMessage.dispose();
            }
        }

        static public BaseMessage loadMessageByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseMessage result = null;

            String query = "select * from [Messages] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);

            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseMessage newMessage = new BaseMessage();
                newMessage.DateAdded = row["DateAdded"].ToString();
                newMessage.OwnerID = row["OwnerID"].ToString();
                newMessage.UniqueID = row["UniqueID"].ToString();
                newMessage.GroupID = row["GroupID"].ToString();
                newMessage.Visible = row["Visible"].ToString();
                newMessage.ThreadID = row["ThreadID"].ToString();
                newMessage.Document = row["Document"].ToString();
                result = newMessage;
            }
            return result;
        }

        static public void deleteMessageByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseMessage result = null;

            String query = "delete from [Messages] where [UniqueID]=@uid;";

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);

            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
        }

        public static DataTable getPostsMostRecent(IntDatabase managementDB)
        {
            string SQL = "SELECT TOP (10) * from [MostRecentPosts] order by [DateAdded] desc;";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }
    }
}
