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
    public class WebmodConsole : ModuleInterface
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
                    if (request[1] == "Console")
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
                    if (request[1] == "Console")
                    {
                        msg = Encoding.ASCII.GetBytes(displayConsole());
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

        private string displayConsole()
        {
            return ("<!DOCTYPE html>\n<HTML>\n<HEAD>\n" + 
                 StaticHTMLLibrary.getStartScriptBlock() + 
                 StaticHTMLLibrary.getMagicLink("I1") + 
                 StaticHTMLLibrary.getResizeScript() +
                 StaticHTMLLibrary.getEndScriptBlock() +
                 "</HEAD>\n<BODY>\n<P>Console</P>\n" +
                 StaticHTMLLibrary.getConsole(State)
                 + "</BODY>\n</HTML>\n");
        }
    }
}
