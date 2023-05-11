using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;

namespace PhlozLib
{
    public class WebmodColors : ModuleInterface
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
                    if (request[1] == "Colors")
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

                string channellist = getColors(State);
                msg = Encoding.ASCII.GetBytes(channellist);
            
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

        public static string getColors(CollectionState State)
        {
            string channellist = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n<list>\r\n";

            foreach (DocumentColor color in State.DocumentColors)
            {
                channellist += DocumentColor.getXML(color);
            }

            channellist += "</list>\r\n";
            return channellist;
        }
    }
}
