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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void updateFilter(BaseFilter channelFilter, IntDatabase managementDB)
        {
            if (channelFilter.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Order", channelFilter.Order);
                data.AddElement("Category", channelFilter.Category);
                data.AddElement("Label", channelFilter.Label);
                data.AddElement("Action", channelFilter.Action);
                data.AddElement("OwnerID", channelFilter.OwnerID);
                data.AddElement("GroupID", channelFilter.GroupID);
                data.AddElement("Origin", channelFilter.Origin);
                data.AddElement("*@UniqueID", channelFilter.UniqueID);
                managementDB.UpdateTree("[Filters]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Filters] ([ChannelID], [Order], [Category], [Label], [Action], [OwnerID], [UniqueID]) VALUES (@ChannelID, @Order, @Category, @Label, @Action, @OwnerID, @UniqueID);";
                
                Tree NewFilter = new Tree();
                NewFilter.AddElement("@ChannelID", channelFilter.ChannelID);
                NewFilter.AddElement("@Order", channelFilter.Order);
                NewFilter.AddElement("@Category", channelFilter.Category);
                NewFilter.AddElement("@Label", channelFilter.Label);
                NewFilter.AddElement("@Action", channelFilter.Action);
                NewFilter.AddElement("@OwnerID", channelFilter.OwnerID);
                channelFilter.UniqueID = "I" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewFilter.AddElement("@UniqueID", channelFilter.UniqueID);
                NewFilter.AddElement("@GroupID", channelFilter.GroupID);
                NewFilter.AddElement("@Origin", channelFilter.Origin);
                managementDB.ExecuteDynamic(sql, NewFilter);
                NewFilter.Dispose();
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

            tmp.AddElement("ChannelID", current.ChannelID);
            tmp.AddElement("Order", current.Order);
            tmp.AddElement("Category", current.Category);
            tmp.AddElement("Label", current.Label);
            tmp.AddElement("Action", current.Action);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseChannel");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }
    }
}
