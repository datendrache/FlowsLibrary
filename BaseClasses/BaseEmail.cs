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
using System.Net.Mail;

namespace Proliferation.Flows
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
            received = Convert.ToDateTime(info.GetElement("received"));
            Message = info.GetElement("Message");
            To = info.GetElement("To");
            From = info.GetElement("From");
            CC = info.GetElement("CC");
            BCC = info.GetElement("BCC");
            Subject = info.GetElement("Subject");
            UniqueID = info.GetElement("UniqueID");
            Sent = info.GetElement("Sent");
            ForwarderID = info.GetElement("ForwarderID");
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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        public Tree getMetadata()
        {
            Tree information = new Tree();

            information.SetElement("Time", received.ToString());
            information.SetElement("From", From);
            information.SetElement("To", To);
            information.SetElement("CC", CC);
            information.SetElement("BCC", BCC);
            information.SetElement("Subject", Subject);
            information.SetElement("Message", Message);
            information.SetElement("Sent", Sent);
            information.SetElement("ForwarderID", ForwarderID);
            information.SetElement("UniqueID", UniqueID);
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
            parms.AddElement("@emailid", emailid);
            DataTable table = managementDB.ExecuteDynamic(SQL, parms);
            parms.Dispose();

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

            tmp.AddElement("received", current.received.ToString());
            tmp.AddElement("Message", current.Message);
            tmp.AddElement("To", current.To);
            tmp.AddElement("From", current.From);
            tmp.AddElement("CC", current.CC);
            tmp.AddElement("BCC", current.BCC);
            tmp.AddElement("Subject", current.Subject);
            tmp.AddElement("Sent", current.Sent);
            tmp.AddElement("ForwarderID", current.ForwarderID);
            tmp.AddElement("UniqueID", current.UniqueID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseEmail");
            tmp.Dispose();
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
                data.AddElement("Sent", "true");
                data.AddElement("*@UniqueID", email.UniqueID);
                managementDB.UpdateTree("Email", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewAccount = new Tree();
                NewAccount.AddElement("received", DateTime.Now.Ticks.ToString());
                NewAccount.AddElement("_DateAdded", "BIGINT");
                NewAccount.AddElement("To", email.To);
                NewAccount.AddElement("From", email.From);
                NewAccount.AddElement("BCC", email.BCC);
                NewAccount.AddElement("CC", email.CC);
                NewAccount.AddElement("Subject", email.Subject);
                NewAccount.AddElement("Message", email.Message);
                NewAccount.AddElement("Sent", email.Sent);
                NewAccount.AddElement("ForwarderID", email.ForwarderID);
                email.UniqueID = "V" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewAccount.AddElement("UniqueID", email.UniqueID);

                managementDB.InsertTree("Email", NewAccount);
                NewAccount.Dispose();
            }
        }
    }
}
