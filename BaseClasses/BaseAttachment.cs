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
    public class BaseAttachment
    {
        public string OwnerID = "";
        public string DateAdded = "";
        public string GroupID = "";
        public string UniqueID = "";
        public string AttachedTo = "";
        public string Filename = "";

        static public void updateAttachment(IntDatabase managementDB, BaseAttachment attachment)
        {
            if (attachment.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("AttachedTo", attachment.AttachedTo);
                data.AddElement("Filename", attachment.Filename);
                data.AddElement("GroupID", attachment.GroupID);
                data.AddElement("*@UniqueID", attachment.UniqueID);
                managementDB.UpdateTree("[MessageThreads]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewAttachment = new Tree();
                attachment.UniqueID = "4" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewAttachment.AddElement("UniqueID", attachment.UniqueID);
                NewAttachment.AddElement("OwnerID", attachment.OwnerID);
                NewAttachment.AddElement("GroupID", attachment.GroupID);
                NewAttachment.AddElement("AttachedTo", attachment.AttachedTo);
                NewAttachment.AddElement("Filename", attachment.Filename);
                NewAttachment.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                managementDB.InsertTree("Attachments", NewAttachment);
                NewAttachment.Dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Attachments](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[AttachedTo] TEXT NULL, " +
                    "[Filename] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Attachments](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[AttachedTo] VARCHAR(33) NULL, "+
                    "[Filename] VARCHAR(512) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public BaseAttachment loadAttachmentByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable tasks;
            BaseAttachment result = null;

            String query = "select * from [Attachments] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseAttachment newMessageThread = new BaseAttachment();
                newMessageThread.DateAdded = row["DateAdded"].ToString();
                newMessageThread.OwnerID = row["OwnerID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.GroupID = row["GroupID"].ToString();
                newMessageThread.Filename = row["Filename"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.AttachedTo = row["AttachedTo"].ToString();
                result = newMessageThread;
            }
            return result;
        }

        static public ArrayList loadAttachmentsByObjectID(IntDatabase managementDB, string objectid)
        {
            ArrayList result = new ArrayList();
            DataTable tasks;

            String query = "select * from [Attachments] where [AttachedTo]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", objectid);
            tasks = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in tasks.Rows)
            {
                BaseAttachment newMessageThread = new BaseAttachment();
                newMessageThread.DateAdded = row["DateAdded"].ToString();
                newMessageThread.OwnerID = row["OwnerID"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.GroupID = row["GroupID"].ToString();
                newMessageThread.Filename = row["Filename"].ToString();
                newMessageThread.UniqueID = row["UniqueID"].ToString();
                newMessageThread.AttachedTo = row["AttachedTo"].ToString();
                result.Add(newMessageThread);
            }
            return result;
        }
    }
}
