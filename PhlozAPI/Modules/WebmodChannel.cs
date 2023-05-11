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
    public class WebmodChannel : ModuleInterface
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
                    if (request[1] == "Channel")
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
                    if (request[1] == "Channel")
                    {
                        switch (request[2])
                        {
                            case "List":
                                {
                                    string channellist = getChannelList(State);
                                    msg = Encoding.ASCII.GetBytes(channellist);
                                }
                                break;

                            case "Documents":
                                {
                                    string channellist = getChannelDocuments(FatumLib.fromSafeString(request[3]));
                                    msg = Encoding.ASCII.GetBytes(channellist);
                                }
                                break;

                            default:
                                msg = Encoding.ASCII.GetBytes(getChannel(FatumLib.fromSafeString(request[2])));
                                break;

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

        public static string getChannelList(CollectionState State)
        {
            string channellist = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n<list>\r\n";

            foreach (BaseChannel channel in State.Channels)
            {
                channellist += BaseChannel.getXML(channel);
            }

            channellist += "</list>\r\n";
            return channellist;
        }


        public string getChannel(string channelID)
        {
            string channel = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n";

            foreach (BaseChannel current in State.Channels)
            {
                if (current.UniqueID == channelID)
                {
                    channel += BaseChannel.getXML(current);
                    return (channel);
                }
            }
            return (channel);
        }

        public string getChannelDocuments(string channelID)
        {
            string channel = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n";
            string data = "<list>\r\n";

            foreach (BaseChannel current in State.Channels)
            {
                if (current.UniqueID == channelID)
                {
                    lock (current.Documents)
                    {
                        try {
                            foreach (BaseDocument msg in current.Documents)
                            {
                                data += BaseDocument.getXML(msg);
                            }
                        }
                        catch (Exception xyz)
                        {
                            System.Console.Out.WriteLine("WebmodChannel: Error getting channel documents. " + xyz.Message + ", " + xyz.StackTrace);
                        }
                    }
                }
            }
            data += "</list>\r\n";

            return (channel + data);
        }
    }
}
