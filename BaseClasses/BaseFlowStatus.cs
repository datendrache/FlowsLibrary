//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using System.Data;
using System.Net;
using System.IO;
using FatumCore;
using FatumAnalytics;
using System.Data.SQLite;
using DatabaseAdapters;

namespace PhlozLib
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
            sqlparms.addElement("@flowid", flow.UniqueID);
            DataTable result = state.managementDB.ExecuteDynamic(SQL, sqlparms);
            sqlparms.dispose();

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
                data.addElement("FlowID", flow.UniqueID);

                state.managementDB.InsertTree("[FlowStatus]", data);
                data.dispose();

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
                    data.addElement("*@UniqueID", flow.UniqueID);
                    State.managementDB.UpdateTree("[FlowStatus]", data, "[FlowID]=@UniqueID");
                    State.statsDB.InsertTree("[FlowStatus]", data);
                    data.dispose();

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

            tmp.addElement("FlowID", current.FlowID);
            tmp.addElement("FlowPosition", current.FlowPosition.ToString());
            tmp.addElement("LastCollectionAttempt", current.LastCollectionAttempt.Ticks.ToString());
            tmp.addElement("MostRecentData", current.MostRecentData.Ticks.ToString());

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseFlowStatus");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        public BaseFlowStatus(Tree XML)
        {
            FlowID = XML.getElement("FlowID");
            FlowPosition = long.Parse(XML.getElement("FlowPosition"));
        }

        public static Tree generateTreeForDBInsertion(BaseFlowStatus flowstatus)
        {
            if (flowstatus!=null)
            {
                Tree data = new Tree();
                data.addElement("FlowID", flowstatus.FlowID);
                data.addElement("SpotTime", DateTime.Now.Ticks.ToString());
                data.addElement("_SpotTime", "BIGINT");
                data.addElement("FlowPosition", flowstatus.FlowPosition.ToString());
                data.addElement("_FlowPosition", "BIGINT");
                data.addElement("LastCollectionAttempt", flowstatus.flow.FlowStatus.LastCollectionAttempt.Ticks.ToString());
                data.addElement("_LastCollectionAttempt", "BIGINT");
                data.addElement("MostRecentData", flowstatus.flow.FlowStatus.MostRecentData.Ticks.ToString());
                data.addElement("_MostRecentData", "BIGINT");
                data.addElement("LastServerResponse", flowstatus.flow.FlowStatus.LastServerResponse.Ticks.ToString());
                data.addElement("_LastServerResponse", "BIGINT");
                data.addElement("BytesReceived", flowstatus.flow.FlowStatus.BytesReceived.ToString());
                data.addElement("_BytesReceived", "BIGINT");
                data.addElement("Requests", flowstatus.flow.FlowStatus.Requests.ToString());
                data.addElement("_Requests", "BIGINT");
                data.addElement("DocumentCount", flowstatus.flow.FlowStatus.DocumentCount.ToString());
                data.addElement("_DocumentCount", "BIGINT");
                data.addElement("Iterations", flowstatus.flow.FlowStatus.Iterations.ToString());
                data.addElement("_Iterations", "BIGINT");
                data.addElement("EmptySets", flowstatus.flow.FlowStatus.EmptySets.ToString());
                data.addElement("_EmptySets", "BIGINT");
                data.addElement("Errors", flowstatus.flow.FlowStatus.Errors.ToString());
                data.addElement("_Errors", "BIGINT");
                data.addElement("CollectionDuration", flowstatus.flow.FlowStatus.CollectionDuration.Ticks.ToString());
                data.addElement("_CollectionDuration", "BIGINT");
                data.addElement("ProcessingDuration", flowstatus.flow.FlowStatus.ProcessingDuration.Ticks.ToString());
                data.addElement("_ProcessingDuration", "BIGINT");
                return data;
            }
            else
            {
                Tree data = new Tree();
                data.addElement("SpotTime", DateTime.Now.Ticks.ToString());
                data.addElement("_SpotTime", "BIGINT");
                data.addElement("FlowPosition", "0");
                data.addElement("_FlowPosition", "BIGINT");
                data.addElement("LastCollectionAttempt", DateTime.MinValue.Ticks.ToString());
                data.addElement("_LastCollectionAttempt", "BIGINT");
                data.addElement("MostRecentData", DateTime.MinValue.Ticks.ToString());
                data.addElement("_MostRecentData", "BIGINT");
                data.addElement("LastServerResponse", DateTime.MinValue.Ticks.ToString());
                data.addElement("_LastServerResponse", "BIGINT");
                data.addElement("BytesReceived", "0");
                data.addElement("_BytesReceived", "BIGINT");
                data.addElement("Requests", "0");
                data.addElement("_Requests", "BIGINT");
                data.addElement("DocumentCount", "0");
                data.addElement("_DocumentCount", "BIGINT");
                data.addElement("Iterations", "0");
                data.addElement("_Iterations", "BIGINT");
                data.addElement("EmptySets", "0");
                data.addElement("_EmptySets", "BIGINT");
                data.addElement("Errors", "0");
                data.addElement("_Errors", "BIGINT");
                data.addElement("CollectionDuration", DateTime.MinValue.Ticks.ToString());
                data.addElement("_CollectionDuration", "BIGINT");
                data.addElement("ProcessingDuration", DateTime.MinValue.Ticks.ToString());
                data.addElement("_ProcessingDuration", "BIGINT");
                return data;
            }
        }
    }
}
