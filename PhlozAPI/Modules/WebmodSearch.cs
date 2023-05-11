using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using FatumCore;
using FatumAnalytics;
using PhlozLib.SearchCore;
using System.Collections;
using System.Management.Automation.Language;
using Fatum.FatumCore;

namespace PhlozLib
{
    public class WebmodSearch : ModuleInterface
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
                    if (request[1] == "Search")
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
                    if (request[1] == "Search")
                    {
                        if (request.Length > 2)
                        {
                            if (State.searchSystem != null)
                            {
                                try
                                {
                                    SearchRequest Request = new SearchRequest();
                                    StreamReader reader = new StreamReader(context.Request.InputStream);
                                    string requestFromPost = reader.ReadToEnd();

                                    Request.Query = XMLTree.readXMLFromString(requestFromPost);
                                    try
                                    {
                                        if (Request.Query  != null)
                                        {
                                            string auth = Request.Query.getElement("Auth");
                                            string countStr = Request.Query.getElement("MaxResults");
                                            int maxResults = 500;
                                            if (countStr!="")
                                            {
                                                if (countStr=="0")
                                                {
                                                    maxResults = int.MaxValue;
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        maxResults = int.Parse(countStr);
                                                    }
                                                    catch (Exception)
                                                    {

                                                    }
                                                }
                                            }

                                            if (auth.ToLower() == State.InstanceUniqueID.ToLower())
                                            {
                                                BaseQueryHost.performQuery(State.searchSystem, Request, maxResults);
                                                BaseSearch.redux((Tree) Request.Result.tree[0], maxResults);
                                                string document = TreeDataAccess.writeTreeToXMLString(Request.Result, "Result");
                                                byte[] amsg = Encoding.ASCII.GetBytes(document);
                                                context.Response.ContentLength64 = amsg.Length;
                                                using (Stream s = context.Response.OutputStream)
                                                    s.Write(amsg, 0, amsg.Length);
                                                Request.Result.dispose();
                                                Request.Result = null;
                                            }
                                            else
                                            {
                                                byte[] amsg = null;
                                                amsg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, "Not authorized.\n\n"));
                                                context.Response.ContentLength64 = amsg.Length;
                                                using (Stream s = context.Response.OutputStream)
                                                    s.Write(amsg, 0, amsg.Length);
                                            }
                                        }
                                    }
                                    catch (Exception xyz)
                                    {
                                        byte[] amsg = null;
                                        amsg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, xyz.Message + "\n\n" + xyz.StackTrace));
                                        context.Response.ContentLength64 = amsg.Length;
                                        using (Stream s = context.Response.OutputStream)
                                            s.Write(amsg, 0, amsg.Length);
                                    }
                                    if (Request!=null)
                                    {
                                        if (Request.Result!=null)
                                        {
                                            Request.Result.dispose();
                                            Request.Result = null;
                                        }
                                    }
                                    if (Request.Query != null)
                                    {
                                        Request.Query.dispose();
                                        Request.Query = null;
                                    }
                                }
                                catch (Exception xyz)
                                {
                                    byte[] amsg = null;
                                    amsg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, xyz.Message + "\n\n" + xyz.StackTrace));
                                    context.Response.ContentLength64 = amsg.Length;
                                    using (Stream s = context.Response.OutputStream)
                                        s.Write(amsg, 0, amsg.Length);
                                }
                            }
                            else
                            {
                                byte[] amsg = null;
                                amsg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, "The searching system is currently unavailable."));
                                context.Response.ContentLength64 = amsg.Length;
                                using (Stream s = context.Response.OutputStream)
                                    s.Write(amsg, 0, amsg.Length);
                            }
                        }
                        else
                        {
                            string channellist = getGeneralStatus(State);
                            msg = Encoding.ASCII.GetBytes(channellist);
                        }
                    }
                }
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

        public static string getGeneralStatus(CollectionState State)
        {
            string generalStatus = "";

            Tree status = new Tree();
            status.setElement("FlowCount", BaseFlow.countActiveFlows(State).ToString());
            status.setElement("ChannelCount", State.Channels.Count.ToString());
            status.setElement("Started", State.Started.ToString());
            status.setElement("DocumentCount", State.DocumentCount.ToString());
            return generalStatus;
        }  
    }
}
