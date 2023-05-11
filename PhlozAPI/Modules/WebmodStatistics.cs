using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using FatumCore;
using System.Diagnostics;
using DatabaseAdapters;
using System.Data;

namespace PhlozLib
{
    public class WebmodStatistics : ModuleInterface
    {
        CollectionState State = null;

        public void Init(CollectionState state)
        {
            State = state;
        }

        public void SendRequest(HttpListenerContext httpContext)
        {
            SendResponse(httpContext);
        }

        public Boolean IsHandler(string uri)
        {
            Boolean result = false;

            char[] sep = { '/' };
            string[] request = uri.Split(sep);

            if (request.Length > 1)
                {
                    if (request[1] == "Statistics")
                    {
                        result = true;
                    }
                }

            return result;
        }

        public void SendResponse(HttpListenerContext context)
        {
            try
            {
                char[] sep = { '/' };
                string[] request = context.Request.RawUrl.Split(sep);
                byte[] msg = null;

                if (request.Length > 1)
                {
                    if (request[1] == "Statistics")
                    {
                        if (request[2] == "General")
                        {
                            string SQL = "select sum([DocumentCount]) as DocumentSum, sum([BytesReceived]) as ByteSum from [FlowStatus] where SpotTime >= @StartTime and SpotTime <= @EndTime";
                            Tree data = new Tree();
                            data.addElement("StartTime", DateTime.Now.AddDays(-1).Ticks.ToString());
                            data.addElement("EndTime", DateTime.Now.Ticks.ToString());
                            DataTable dt = State.statsDB.ExecuteDynamic(SQL, data);
                            data.dispose();
                            Tree generalStats = new Tree();
                            if (dt.Rows.Count > 0)
                            {
                                DataRow row = dt.Rows[0];

                                generalStats.setElement("DocumentCount", row["DocumentSum"].ToString());
                                generalStats.setElement("BytesReceived", row["ByteSum"].ToString());
                            }
                            string channellist = TreeDataAccess.writeTreeToXMLString(generalStats, "Stats");
                            generalStats.dispose();
                            msg = Encoding.ASCII.GetBytes(channellist);
                        }

                        if (request[2] == "Flows")
                        {
                            string SQL = "select [FlowID], sum([DocumentCount]) as DocumentSum, sum([BytesReceived]) as ByteSum, sum([Requests]) as RequestTotal, max([LastServerResponse]) as [LastResponse], max([LastCollectionAttempt]) as [LastRequest], sum([Errors]) as TotalErrors, sum([ProcessingDuration]) as [TotalProcessingTime], sum([CollectionDuration]) as [TotalCollectionTime], sum([EmptySets]) as [TotalEmptySets], sum([Iterations]) as [TotalIterations]from [FlowStatus] where SpotTime >= @StartTime and SpotTime <= @EndTime Group By [FlowID]";
                            Tree data = new Tree();
                            data.addElement("StartTime", DateTime.Now.AddDays(-1).Ticks.ToString());
                            data.addElement("EndTime", DateTime.Now.Ticks.ToString());
                            DataTable dt = State.statsDB.ExecuteDynamic(SQL, data);
                            data.dispose();
                            Tree generalStats = new Tree();
                            foreach (DataRow row in dt.Rows)
                            {
                                Tree flowstat = new Tree();
                                flowstat.addElement("FlowName", BaseFlow.locateCachedFlowByUniqueID(row["FlowID"].ToString(), State).FlowName);
                                flowstat.setElement("DocumentCount", row["DocumentSum"].ToString());
                                flowstat.setElement("BytesReceived", row["ByteSum"].ToString());
                                flowstat.setElement("RequestTotal", row["RequestTotal"].ToString());
                                flowstat.setElement("LastServerResponse", row["LastResponse"].ToString());
                                flowstat.setElement("LastCollectionAttempt", row["LastRequest"].ToString());
                                flowstat.setElement("TotalErrors", row["TotalErrors"].ToString());
                                flowstat.setElement("TotalProcessingTime", row["TotalProcessingTime"].ToString());
                                flowstat.setElement("TotalCollectionTime", row["TotalCollectionTime"].ToString());
                                flowstat.setElement("TotalEmptySets", row["TotalEmptySets"].ToString());
                                flowstat.setElement("TotalIterations", row["TotalIterations"].ToString());
                                generalStats.addNode(flowstat, row["FlowID"].ToString());
                            }
                            string channellist = TreeDataAccess.writeTreeToXMLString(generalStats, "Stats");
                            generalStats.dispose();
                            msg = Encoding.ASCII.GetBytes(channellist);
                        }
                    }
                }

                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
            catch (Exception xyzzy)
            {
                byte[] msg = null;
                msg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, xyzzy.Message + "\n\n" + xyzzy.StackTrace)); 
                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
        }
    }
}
