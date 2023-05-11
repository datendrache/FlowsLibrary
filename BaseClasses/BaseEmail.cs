//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Threading;
using FatumCore;
using System.Data;
using System.IO;
using DatabaseAdapters;
using System.Net.Mail;

namespace PhlozLib
{
    public class BaseEmail
    {
        public DateTime received;
        public String Message = "";
        public String From = "";
        public String To = "";
        public String CC = "";
        public String BCC = "";
        public String Subject = "";
        public String Sent = "";
        public String UniqueID = "";
        public String ForwarderID = "";
        public Boolean isHTML = false;
        public AlternateView av = null;

        public BaseEmail()
        {
        }

        public BaseEmail(Tree info)
        {
            received = Convert.ToDateTime(info.getElement("received"));
            Message = info.getElement("Message");
            To = info.getElement("To");
            From = info.getElement("From");
            CC = info.getElement("CC");
            BCC = info.getElement("BCC");
            Subject = info.getElement("Subject");
            UniqueID = info.getElement("UniqueID");
            Sent = info.getElement("Sent");
            ForwarderID = info.getElement("ForwarderID");
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Email](" +
                    "[Received] INTEGER NULL," +
                    "[To] TEXT NULL, " +
                    "[From] TEXT NULL, " +
                    "[CC] TEXT NULL, " +
                    "[BCC] TEXT NULL, " +
                    "[Subject] TEXT NULL, " +
                    "[Message] TEXT NULL, " +
                    "[Sent] TEXT NULL, " +
                    "[ForwarderID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Email](" +
                    "[Received] BIGINT NULL," +
                    "[To] NVARCHAR(256) NULL, " +
                    "[From] NVARCHAR(256) NULL, " +
                    "[CC] NVARCHAR(256) NULL, " +
                    "[BCC] NVARCHAR(256) NULL, " +
                    "[Subject] NVARCHAR(512) NULL, " +
                    "[Message] NVARCHAR(MAX) NULL, " +
                    "[Sent] VARCHAR(8) NULL, " +
                    "[ForwarderID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL );";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseemail ON Email([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseemail ON Email([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void removeEmailByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Email] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        public Tree getMetadata()
        {
            Tree information = new Tree();

            information.setElement("Time", received.ToString());
            information.setElement("From", From);
            information.setElement("To", To);
            information.setElement("CC", CC);
            information.setElement("BCC", BCC);
            information.setElement("Subject", Subject);
            information.setElement("Message", Message);
            information.setElement("Sent", Sent);
            information.setElement("ForwarderID", ForwarderID);
            information.setElement("UniqueID", UniqueID);
            information.Value = "Metadata";

            return information;
        }

        public void dispose()
        {
            Message = null;
            To = null;
            From = null;
            Subject = null;
            BCC = null;
            CC = null;
            UniqueID = null;
        }

        public static BaseEmail getEmailByUniqueID(IntDatabase managementDB, string emailid)
        {
            BaseEmail result = null;

            string SQL = "select * from [Email] where [UniqueID]=@emailid;";
            Tree parms = new Tree();
            parms.addElement("@emailid", emailid);
            DataTable table = managementDB.ExecuteDynamic(SQL, parms);
            parms.dispose();

            if (table.Rows.Count > 0)
            {
                DataRow msg = table.Rows[0];

                long.TryParse(msg["Received"].ToString(), out long ticks);
                result.received = new DateTime(ticks);
                result.Message = msg["Message"].ToString();
                result.To = msg["ForwarderID"].ToString();
                result.From = msg["Latitude"].ToString();
                result.CC = msg["Longitude"].ToString();
                result.BCC = msg["Label"].ToString();
                result.Subject = msg["Category"].ToString();
                result.Message = msg["Category"].ToString();
                result.Sent = msg["Category"].ToString();
                result.ForwarderID = msg["ForwarderID"].ToString();
                result.UniqueID = msg["UniqueID"].ToString();              
            }
            return result;
        }

        static public string getXML(BaseEmail current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("received", current.received.ToString());
            tmp.addElement("Message", current.Message);
            tmp.addElement("To", current.To);
            tmp.addElement("From", current.From);
            tmp.addElement("CC", current.CC);
            tmp.addElement("BCC", current.BCC);
            tmp.addElement("Subject", current.Subject);
            tmp.addElement("Sent", current.Sent);
            tmp.addElement("ForwarderID", current.ForwarderID);
            tmp.addElement("UniqueID", current.UniqueID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseEmail");
            tmp.dispose();
            result = outs.ToString();
            //result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            result = result.Substring(41, result.Length - 43);
            return result;
        }

        static public void updateEmail(IntDatabase managementDB, BaseEmail email)
        {
            if (email.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Sent", "true");
                data.addElement("*@UniqueID", email.UniqueID);
                managementDB.UpdateTree("Email", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewAccount = new Tree();
                NewAccount.addElement("received", DateTime.Now.Ticks.ToString());
                NewAccount.addElement("_DateAdded", "BIGINT");
                NewAccount.addElement("To", email.To);
                NewAccount.addElement("From", email.From);
                NewAccount.addElement("BCC", email.BCC);
                NewAccount.addElement("CC", email.CC);
                NewAccount.addElement("Subject", email.Subject);
                NewAccount.addElement("Message", email.Message);
                NewAccount.addElement("Sent", email.Sent);
                NewAccount.addElement("ForwarderID", email.ForwarderID);
                email.UniqueID = "V" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewAccount.addElement("UniqueID", email.UniqueID);

                managementDB.InsertTree("Email", NewAccount);
                NewAccount.dispose();
            }
        }
    }
}
