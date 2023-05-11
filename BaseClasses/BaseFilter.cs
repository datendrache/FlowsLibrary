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
    public class BaseFilter
    {
        public string ChannelID = "";
        public string Order = "";
        public string Category = "";
        public string Label = "";
        public string Action = "";
        public string OwnerID = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string Origin = "";
        
        static public ArrayList loadFilters(CollectionState State)
        {
            return loadFilters(State.managementDB);
        }

        ~BaseFilter()
        {
            ChannelID = null;
            Order = null;
            Category = null;
            Label = null;
            Action = null;
            OwnerID = null;
            UniqueID = null;
            GroupID = null;
            Origin = null;
        }
        
        static public ArrayList loadFilters(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Filters];";
            processors = managementDB.Execute(query);

            ArrayList colorList = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseFilter newFilter = new BaseFilter();
                newFilter.ChannelID = row["ChannelID"].ToString();
                newFilter.Category = row["Category"].ToString();
                newFilter.Label = row["Label"].ToString();
                newFilter.Action = row["Action"].ToString();
                newFilter.UniqueID = row["UniqueID"].ToString();
                newFilter.OwnerID = row["OwnerID"].ToString();
                newFilter.GroupID = row["GroupID"].ToString();
                newFilter.Origin = row["Origin"].ToString();
                colorList.Add(newFilter);
            }

            return colorList;
        }

        static public void updateFilter(BaseFilter channelFilter, CollectionState State)
        {
            updateFilter(channelFilter, State.managementDB);
        }

        static public void removeFilterByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Filters] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updateFilter(BaseFilter channelFilter, IntDatabase managementDB)
        {
            if (channelFilter.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Order", channelFilter.Order);
                data.addElement("Category", channelFilter.Category);
                data.addElement("Label", channelFilter.Label);
                data.addElement("Action", channelFilter.Action);
                data.addElement("OwnerID", channelFilter.OwnerID);
                data.addElement("GroupID", channelFilter.GroupID);
                data.addElement("Origin", channelFilter.Origin);
                data.addElement("*@UniqueID", channelFilter.UniqueID);
                managementDB.UpdateTree("[Filters]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Filters] ([ChannelID], [Order], [Category], [Label], [Action], [OwnerID], [UniqueID]) VALUES (@ChannelID, @Order, @Category, @Label, @Action, @OwnerID, @UniqueID);";
                
                Tree NewFilter = new Tree();
                NewFilter.addElement("@ChannelID", channelFilter.ChannelID);
                NewFilter.addElement("@Order", channelFilter.Order);
                NewFilter.addElement("@Category", channelFilter.Category);
                NewFilter.addElement("@Label", channelFilter.Label);
                NewFilter.addElement("@Action", channelFilter.Action);
                NewFilter.addElement("@OwnerID", channelFilter.OwnerID);
                channelFilter.UniqueID = "I" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewFilter.addElement("@UniqueID", channelFilter.UniqueID);
                NewFilter.addElement("@GroupID", channelFilter.GroupID);
                NewFilter.addElement("@Origin", channelFilter.Origin);
                managementDB.ExecuteDynamic(sql, NewFilter);
                NewFilter.dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Filters](" +
                    "[ChannelID] TEXT NULL, " +
                    "[Order] TEXT NULL, " +
                    "[Category] TEXT NULL, " +
                    "[Label] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Action] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Filters](" +
                    "[ChannelID] VARCHAR(20) NULL, " +
                    "[Order] VARCHAR(20) NULL, " +
                    "[Category] NVARCHAR(100) NULL, " +
                    "[Label] NVARCHAR(100) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Action] VARCHAR(100) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basefilters ON Filters([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basefilters ON Filters([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getXML(BaseFilter current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("ChannelID", current.ChannelID);
            tmp.addElement("Order", current.Order);
            tmp.addElement("Category", current.Category);
            tmp.addElement("Label", current.Label);
            tmp.addElement("Action", current.Action);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseChannel");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }
    }
}
