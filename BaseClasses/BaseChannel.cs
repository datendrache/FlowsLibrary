//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using FatumCore;
using DatabaseAdapters;

namespace PhlozLib
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
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updateChannel(BaseChannel channel, IntDatabase managementDB)
        {
            if (channel.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Name", channel.Name);
                data.addElement("OwnerID", channel.OwnerID);
                data.addElement("GroupID", channel.GroupID);
                data.addElement("Origin", channel.Origin);
                data.addElement("*@UniqueID", channel.UniqueID);
                managementDB.UpdateTree("[Channels]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Channels] ([Name], [OwnerID], [Origin]) VALUES (@Name, @OwnerID, @UniqueID, @Origin);";

                Tree NewChannel = new Tree();
                NewChannel.addElement("@Name", channel.Name);
                NewChannel.addElement("@OwnerID", channel.OwnerID);
                channel.UniqueID = "H" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewChannel.addElement("@UniqueID", channel.UniqueID);
                NewChannel.addElement("@GroupID", channel.GroupID);
                NewChannel.addElement("@Origin", channel.Origin);
                managementDB.ExecuteDynamic(sql, NewChannel);
                NewChannel.dispose();
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
            parms.addElement("@channelid", Channel.UniqueID);
            managementDB.ExecuteDynamic(delSQL, parms);
            parms.dispose();

            managementDB.ExecuteNonQuery(delSQL);
        }

        static public string getXML(BaseChannel current)
        {
            string result = "";
            Tree tmp = new Tree();
            tmp.addElement("Name", current.Name);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseChannel");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n","");
            return result;
        }
    }
}
