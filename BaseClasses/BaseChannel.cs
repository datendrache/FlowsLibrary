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
    public class BaseChannel
    {
        public string Name = "";
        public string OwnerID = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string Origin = "";

        public LinkedList<BaseDocument> Documents = new LinkedList<BaseDocument>();

        ~BaseChannel()
        {
            if (Documents != null)
            {
                Documents.Clear();
                Documents = null;
            }
            Name = null;
            OwnerID = null;
            UniqueID = null;
            GroupID = null;
        }

        static public ArrayList loadChannels(CollectionState State)
        {
            return loadChannels(State.managementDB);
        }

        static public ArrayList loadChannels(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Channels];";
            processors = managementDB.Execute(query);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseChannel newChannel = new BaseChannel();
                newChannel.Name = row["Name"].ToString();
                newChannel.OwnerID = row["OwnerID"].ToString();
                newChannel.UniqueID = row["UniqueID"].ToString();
                newChannel.GroupID = row["GroupID"].ToString();
                newChannel.Origin = row["Origin"].ToString();
                tmpProcessors.Add(newChannel);
            }

            return tmpProcessors;
        }

        static public void updateChannel(BaseChannel channel, CollectionState State)
        {
            updateChannel(channel, State.managementDB);
        }

        static public void removeChannelByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Channels] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void updateChannel(BaseChannel channel, IntDatabase managementDB)
        {
            if (channel.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Name", channel.Name);
                data.AddElement("OwnerID", channel.OwnerID);
                data.AddElement("GroupID", channel.GroupID);
                data.AddElement("Origin", channel.Origin);
                data.AddElement("*@UniqueID", channel.UniqueID);
                managementDB.UpdateTree("[Channels]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Channels] ([Name], [OwnerID], [Origin]) VALUES (@Name, @OwnerID, @UniqueID, @Origin);";

                Tree NewChannel = new Tree();
                NewChannel.AddElement("@Name", channel.Name);
                NewChannel.AddElement("@OwnerID", channel.OwnerID);
                channel.UniqueID = "H" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewChannel.AddElement("@UniqueID", channel.UniqueID);
                NewChannel.AddElement("@GroupID", channel.GroupID);
                NewChannel.AddElement("@Origin", channel.Origin);
                managementDB.ExecuteDynamic(sql, NewChannel);
                NewChannel.Dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Channels](" +
                        "[OwnerID] TEXT NULL, " +
                        "[UniqueID] TEXT NULL, " +
                        "[GroupID] TEXT NULL, " +
                        "[Origin] TEXT NULL, " +
                        "[Name] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Channels](" +
                        "[OwnerID] VARCHAR(33) NULL, " +
                        "[UniqueID] VARCHAR(33) NULL, " +
                        "[GroupID] VARCHAR(33) NULL, " +
                        "[Origin] VARCHAR(33) NULL, " +
                        "[Name] NVARCHAR(100) NULL);";   
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basechannels ON Channels([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basechannels ON Channels([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }


        static public void deleteChannel(BaseChannel Channel, IntDatabase managementDB)
        {
            string delSQL = "DELETE FROM [Channels] WHERE [UniqueID]=@channelid;";
            Tree parms = new Tree();
            parms.AddElement("@channelid", Channel.UniqueID);
            managementDB.ExecuteDynamic(delSQL, parms);
            parms.Dispose();

            managementDB.ExecuteNonQuery(delSQL);
        }

        static public string getXML(BaseChannel current)
        {
            string result = "";
            Tree tmp = new Tree();
            tmp.AddElement("Name", current.Name);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseChannel");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n","");
            return result;
        }
    }
}
