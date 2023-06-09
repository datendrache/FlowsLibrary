﻿//   Flows Libraries -- Flows Common Classes and Methods
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
using DatabaseAdapters;
using Proliferation.Fatum;

namespace Proliferation.Flows
{
    public class ChannelFlow
    {
        public string DateAdded = "";
        public string ChannelID = "";
        public string FlowID = "";
        public string UniqueID = "";

        ~ChannelFlow()
        {
            DateAdded = null;
            ChannelID = null;
            FlowID = null;
            UniqueID = null;
        }

        static public ArrayList loadFlows(CollectionState State)
        {
            return loadFlows(State.managementDB);
        }

        static public ArrayList loadFlows(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [ChannelFlows];";
            processors = managementDB.Execute(query);

            ArrayList tmpProcessors = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                ChannelFlow newRule = new ChannelFlow();
                newRule.DateAdded = row["DateAdded"].ToString();
                newRule.ChannelID = row["ChannelID"].ToString();
                newRule.FlowID = row["FlowID"].ToString();
                newRule.FlowID = row["UniqueID"].ToString();
                tmpProcessors.Add(newRule);
            }

            return tmpProcessors;
        }

        static public void updateChannelFlow(ChannelFlow rule, CollectionState State)
        {
            updateChannelFlow(rule, State.managementDB);
        }

        static public void updateChannelFlow(ChannelFlow channel, IntDatabase managementDB)
        {
            if (channel.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("ChannelID", channel.ChannelID);
                data.AddElement("FlowID", channel.FlowID);
                data.AddElement("*@UniqueID", channel.UniqueID);
                managementDB.UpdateTree("[ChannelFlows]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [ChannelFlows] ([DateAdded], [ChannelID], [UniqueID], [FlowID]) VALUES ( " +
                    "'" + channel.DateAdded + "', " +
                    "'" + channel.ChannelID + "', " +
                    "'" + channel.UniqueID + "', " +
                    "'" + channel.FlowID + "');";
                managementDB.ExecuteNonQuery(sql);
            }
        }

        static public void removeChannelFlowByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [ChannelFlows] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [ChannelFlows](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[ChannelID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[FlowID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [ChannelFlows](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[ChannelID] VARCHAR(16) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[FlowID] VARCHAR(16) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basechannelflows ON ChannelFlows([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basechannelflowschannel ON ChannelFlows([ChannelID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basechannelflows ON ChannelFlows([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basechannelflowschannel ON ChannelFlows([ChannelID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void updateFlows(string ChannelID, ArrayList newLinks, CollectionState State)
        {
            updateFlows(ChannelID, newLinks, State.managementDB);
        }
        static public void updateFlows(string ChannelID, ArrayList newLinks, IntDatabase managementDB)
        {
            ArrayList tmpList = new ArrayList();
            ArrayList ChannelFlows = ChannelFlow.loadFlows(managementDB);

            if (ChannelID != null)
            {
                if (ChannelID != "")
                {
                    string delSQL = "DELETE FROM [ChannelFlows] WHERE [UniqueID]=" + ChannelID + ";";
                    managementDB.ExecuteNonQuery(delSQL);
                }
            }

            foreach (ChannelFlow current in ChannelFlows)
            {
                if (!(current.ChannelID == ChannelID))
                {
                    tmpList.Add(current);
                }
            }

            foreach (BaseFlow current in newLinks)
            {
                ChannelFlow newChannelFlow = new ChannelFlow();
                newChannelFlow.ChannelID = ChannelID;
                newChannelFlow.FlowID = current.UniqueID;
                tmpList.Add(newChannelFlow);
            }

            ChannelFlows.Clear();
            updateFlows(tmpList, managementDB);
            tmpList.Clear();
        }

        static public void updateFlows(ArrayList Links, IntDatabase managementDB)
        {
            foreach (ChannelFlow current in Links)
            {
                updateChannelFlow(current, managementDB);
            }
        }

        static public ArrayList retrieveFlows(String ChannelID, CollectionState state)
        {
            ArrayList result = new ArrayList();
            ArrayList ChannelFlows = ChannelFlow.loadFlows(state.managementDB);
            ArrayList Sources = BaseSource.loadSources(state.managementDB);
            ArrayList Flows = BaseFlow.loadFlows(state, Sources);

            foreach (ChannelFlow current in ChannelFlows)
            {
                if (current.ChannelID == ChannelID)
                {
                    for (int i = 0; i < Flows.Count;i++)
                    {
                        BaseFlow currentFlow = (BaseFlow)Flows[i];
                        if (current.FlowID == currentFlow.UniqueID)
                        {
                            result.Add(currentFlow);
                        }
                    }
                }
            }

            ChannelFlows.Clear();
            Flows.Clear();
            return result;
        }
    }
}
