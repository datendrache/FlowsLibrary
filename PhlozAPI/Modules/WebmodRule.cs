using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using FatumCore;

namespace PhlozLib
{
    public class WebmodRule : ModuleInterface
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
                    if (request[1] == "Rule")
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

                if (request.Length > 2)
                {
                    if (request[1] == "Rule")
                    {
                        if (request[2] == "List")
                        {
                            string channellist = getRuleList(State);
                            msg = Encoding.ASCII.GetBytes(channellist);
                        }
                        else
                        {
                            msg = Encoding.ASCII.GetBytes(getRule(FatumLib.fromSafeString(request[2])));
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

        public static string getRuleList(CollectionState State)
        {
            string channellist = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n";

            //foreach (BaseRule filter in State.Rules)
            //{
            //    channellist += BaseRule.getXML(filter);
            //}
            return channellist;
        }


        public string getRule(string filterID)
        {
            string filter = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n";

            //foreach (BaseRule current in State.Rules)
            //{
            //    if (current.UniqueID == filterID)
            //    {
            //        filter += BaseRule.getXML(current);
            //        return (filter);
            //    }
            //}
            return (filter);
        }
    }
}
