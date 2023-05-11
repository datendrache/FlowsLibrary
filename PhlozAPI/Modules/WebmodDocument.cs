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
    public class WebmodDocument : ModuleInterface
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
                    if (request[1] == "Document")
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

                if (request.Length > 3)
                {
                    if (request[1] == "Document")
                    {
                        string documentID = request[2];
                        try
                        {
                            BaseDocument display = BaseDocument.getDocument(State, request[2], request[3], documentID);
                            msg = Encoding.ASCII.GetBytes(displayDocument(display));
                        }
                        catch (Exception)
                        {
                            msg = Encoding.ASCII.GetBytes("Document ID " + documentID + " invalid or no longer available in the database.");
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

        private string displayDocument(BaseDocument display)
        {
            string channelHTML = "";

            channelHTML += "<tr><td>Document ID: " + display.ID.ToString() + "</td></tr>";
            channelHTML += "<tr><td>Received: " + display.received.ToString() + "</td></tr>";
            channelHTML += "<tr><td>Flow ID: " + display.FlowID + "</td></tr>";
            channelHTML += "<tr><td>Label: " + display.Label + "</td></tr>";
            channelHTML += "<tr><td>Category: " + display.Category + "</td></tr>";
            channelHTML += "<tr><td>Document: " + FatumLib.fromSafeString(display.Document) +"</td></tr>";

            return ("<HTML><HEAD><meta http-equiv=\"refresh\" content=\"30\">Document Viewer</HEAD><BODY>" + StaticHTMLLibrary.getLogStyle() + "<TABLE border=\"0\" cellpadding=\"1\" cellspacing=\"1\">" + channelHTML + "</TABLE></BODY></HTML>");
        }
    }
}
