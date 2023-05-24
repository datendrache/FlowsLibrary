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

using System.Data;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseFlowStatus
    {
        public string FlowID = "";
        public BaseFlow flow;

        public int Period = 600;
        public DateTime LastPeriod = DateTime.MinValue;
        public string tags = "";

        // Key Metrics

        public long FlowPosition = 0;
        public DateTime MostRecentData = DateTime.MinValue;
        public DateTime LastCollectionAttempt = DateTime.MinValue;
        public DateTime LastServerResponse = DateTime.MinValue;
        public long BytesReceived = 0;
        public long Requests = 0;
        public long DocumentCount = 0;
        public long Iterations = 0;
        public int EmptySets = 0;
        public int Errors = 0;
        public DateTime CollectionDuration = DateTime.MinValue;
        public DateTime ProcessingDuration = DateTime.MinValue;

        public int UPDATELOCK = 0;

        public const int IMMEDIATE = 0;
        public const int PERIODIC = 1;
        public const int NEVER = 2;

        //  The following are operational variables

        public BaseFlowStatus(BaseFlow flow)
        {
            this.flow = flow;
            LastPeriod = DateTime.Now;
        }

        ~BaseFlowStatus()
        {
            FlowID = null;
            tags = null;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [FlowStatus](" +
                    "[SpotTime] INTEGER NULL, " +
                    "[FlowID] TEXT NULL, " +
                    "[LastCollectionAttempt] INTEGER NULL, " +
                    "[MostRecentData] INTEGER NULL, " +
                    "[LastServerResponse] INTEGER NULL, " +
                    "[BytesReceived] INTEGER NULL, " +
                    "[Requests] INTEGER NULL, " +
                    "[DocumentCount] INTEGER NULL, " +
                    "[Iterations] INTEGER NULL, " +
                    "[EmptySets] INTEGER NULL, " +
                    "[Errors] INTEGER NULL, " +
                    "[CollectionDuration] INTEGER NULL, " +
                    "[ProcessingDuration] INTEGER NULL, " +
                    "[FlowPosition] INTEGER NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [FlowStatus](" +
                    "[SpotTime] BIGINT NULL, " +
                    "[FlowID] VARCHAR(33) NULL, " +
                    "[LastCollectionAttempt] BIGINT NULL, " +
                    "[MostRecentData] BIGINT NULL, " +
                    "[LastServerResponse] BIGINT NULL, " +
                    "[BytesReceived] BIGINT NULL, " +
                    "[Requests] BIGINT NULL, " +
                    "[DocumentCount] BIGINT NULL, " +
                    "[Iterations] BIGINT NULL, " +
                    "[EmptySets] BIGINT NULL, " +
                    "[Errors] BIGINT NULL, " +
                    "[CollectionDuration] BIGINT NULL, " +
                    "[ProcessingDuration] BIGINT NULL, " +
                    "[FlowPosition] BIGINT NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseflowstatus ON flowstatus([FlowID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseflowstatus ON flowstatus([FlowID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void loadBaseFlowStatus(CollectionState state, BaseFlow flow)
        {
            string SQL = "select * from [FlowStatus] where [FlowID]=@flowid order by [MostRecentData] desc limit 1";
            Tree sqlparms = new Tree();
            sqlparms.AddElement("@flowid", flow.UniqueID);
            DataTable result = state.managementDB.ExecuteDynamic(SQL, sqlparms);
            sqlparms.Dispose();

            if (result.Rows.Count>0)
            {
                BaseFlowStatus newStatus = new BaseFlowStatus(flow);

                DataRow row = result.Rows[0];
                newStatus.FlowID = row["FlowID"].ToString();
                newStatus.FlowPosition = (long) row["FlowPosition"];
                long value = long.Parse(row["LastCollectionAttempt"].ToString());
                newStatus.LastCollectionAttempt = new DateTime(value);
                newStatus.MostRecentData = new DateTime(long.Parse(row["MostRecentData"].ToString()));
                flow.FlowStatus = newStatus;
            }
            else
            {
                Tree data = generateTreeForDBInsertion(flow.FlowStatus);
                data.AddElement("FlowID", flow.UniqueID);

                state.managementDB.InsertTree("[FlowStatus]", data);
                data.Dispose();

                loadBaseFlowStatus(state, flow);  // We inserted a fresh status, so we recurse and expect a different result.
            }
        }

        public void updateFlowPosition(CollectionState State)
        {
            if ((LastPeriod.Ticks + (600000000)) < DateTime.Now.Ticks)   // Approximate 60 seconds
            {
                try
                {
                    Tree data = generateTreeForDBInsertion(this);
                    data.AddElement("*@UniqueID", flow.UniqueID);
                    State.managementDB.UpdateTree("[FlowStatus]", data, "[FlowID]=@UniqueID");
                    State.statsDB.InsertTree("[FlowStatus]", data);
                    data.Dispose();

                    // Provided there is no error, we will update the local values.
                    // If there is an error, we'll leave them unchanged so the system will try again.
                    flow.FlowStatus.LastPeriod = DateTime.Now;
                    flow.FlowStatus.BytesReceived = 0;
                    flow.FlowStatus.CollectionDuration = DateTime.MinValue;
                    flow.FlowStatus.ProcessingDuration = DateTime.MinValue;
                    flow.FlowStatus.Errors = 0;
                    flow.FlowStatus.Iterations = 0;
                    flow.FlowStatus.Requests = 0;
                    flow.FlowStatus.DocumentCount = 0;
                    flow.FlowStatus.EmptySets = 0;
                }
                catch (Exception xyz)
                {
                    flow.FlowStatus.LastPeriod = DateTime.Now;  // This is to prevent infinite looping...
                }
            }
        }

        static public string getXML(BaseFlowStatus current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("FlowID", current.FlowID);
            tmp.AddElement("FlowPosition", current.FlowPosition.ToString());
            tmp.AddElement("LastCollectionAttempt", current.LastCollectionAttempt.Ticks.ToString());
            tmp.AddElement("MostRecentData", current.MostRecentData.Ticks.ToString());

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseFlowStatus");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        public BaseFlowStatus(Tree XML)
        {
            FlowID = XML.GetElement("FlowID");
            FlowPosition = long.Parse(XML.GetElement("FlowPosition"));
        }

        public static Tree generateTreeForDBInsertion(BaseFlowStatus flowstatus)
        {
            if (flowstatus!=null)
            {
                Tree data = new Tree();
                data.AddElement("FlowID", flowstatus.FlowID);
                data.AddElement("SpotTime", DateTime.Now.Ticks.ToString());
                data.AddElement("_SpotTime", "BIGINT");
                data.AddElement("FlowPosition", flowstatus.FlowPosition.ToString());
                data.AddElement("_FlowPosition", "BIGINT");
                data.AddElement("LastCollectionAttempt", flowstatus.flow.FlowStatus.LastCollectionAttempt.Ticks.ToString());
                data.AddElement("_LastCollectionAttempt", "BIGINT");
                data.AddElement("MostRecentData", flowstatus.flow.FlowStatus.MostRecentData.Ticks.ToString());
                data.AddElement("_MostRecentData", "BIGINT");
                data.AddElement("LastServerResponse", flowstatus.flow.FlowStatus.LastServerResponse.Ticks.ToString());
                data.AddElement("_LastServerResponse", "BIGINT");
                data.AddElement("BytesReceived", flowstatus.flow.FlowStatus.BytesReceived.ToString());
                data.AddElement("_BytesReceived", "BIGINT");
                data.AddElement("Requests", flowstatus.flow.FlowStatus.Requests.ToString());
                data.AddElement("_Requests", "BIGINT");
                data.AddElement("DocumentCount", flowstatus.flow.FlowStatus.DocumentCount.ToString());
                data.AddElement("_DocumentCount", "BIGINT");
                data.AddElement("Iterations", flowstatus.flow.FlowStatus.Iterations.ToString());
                data.AddElement("_Iterations", "BIGINT");
                data.AddElement("EmptySets", flowstatus.flow.FlowStatus.EmptySets.ToString());
                data.AddElement("_EmptySets", "BIGINT");
                data.AddElement("Errors", flowstatus.flow.FlowStatus.Errors.ToString());
                data.AddElement("_Errors", "BIGINT");
                data.AddElement("CollectionDuration", flowstatus.flow.FlowStatus.CollectionDuration.Ticks.ToString());
                data.AddElement("_CollectionDuration", "BIGINT");
                data.AddElement("ProcessingDuration", flowstatus.flow.FlowStatus.ProcessingDuration.Ticks.ToString());
                data.AddElement("_ProcessingDuration", "BIGINT");
                return data;
            }
            else
            {
                Tree data = new Tree();
                data.AddElement("SpotTime", DateTime.Now.Ticks.ToString());
                data.AddElement("_SpotTime", "BIGINT");
                data.AddElement("FlowPosition", "0");
                data.AddElement("_FlowPosition", "BIGINT");
                data.AddElement("LastCollectionAttempt", DateTime.MinValue.Ticks.ToString());
                data.AddElement("_LastCollectionAttempt", "BIGINT");
                data.AddElement("MostRecentData", DateTime.MinValue.Ticks.ToString());
                data.AddElement("_MostRecentData", "BIGINT");
                data.AddElement("LastServerResponse", DateTime.MinValue.Ticks.ToString());
                data.AddElement("_LastServerResponse", "BIGINT");
                data.AddElement("BytesReceived", "0");
                data.AddElement("_BytesReceived", "BIGINT");
                data.AddElement("Requests", "0");
                data.AddElement("_Requests", "BIGINT");
                data.AddElement("DocumentCount", "0");
                data.AddElement("_DocumentCount", "BIGINT");
                data.AddElement("Iterations", "0");
                data.AddElement("_Iterations", "BIGINT");
                data.AddElement("EmptySets", "0");
                data.AddElement("_EmptySets", "BIGINT");
                data.AddElement("Errors", "0");
                data.AddElement("_Errors", "BIGINT");
                data.AddElement("CollectionDuration", DateTime.MinValue.Ticks.ToString());
                data.AddElement("_CollectionDuration", "BIGINT");
                data.AddElement("ProcessingDuration", DateTime.MinValue.Ticks.ToString());
                data.AddElement("_ProcessingDuration", "BIGINT");
                return data;
            }
        }
    }
}
