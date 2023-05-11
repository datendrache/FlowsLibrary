using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using FatumCore;
using System.IO.Compression;
using Fatum.FatumCore;

namespace PhlozLib
{
    public class WebmodArchive : ModuleInterface
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
                    if (request[1] == "Archives")
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
                    if (request[1] == "Archives")
                    {
                        switch (request[2].ToLower())
                        {
                            case "list":
                                {
                                    string channellist = getArchiveList(State);
                                    msg = Encoding.ASCII.GetBytes(channellist);
                                }
                                break;
                            case "restore":
                                {
                                    StreamReader reader = new StreamReader(context.Request.InputStream);
                                    string requestFromPost = reader.ReadToEnd();
                                    Tree archivelist = XMLTree.readXMLFromString(requestFromPost);
                                    string channellist = activateArchive(archivelist);
                                    msg = Encoding.ASCII.GetBytes(channellist);
                                }
                                break;
                            default:
                                msg = Encoding.ASCII.GetBytes(getBlank(FatumLib.fromSafeString(request[2])));
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

        public static string getArchiveList(CollectionState State)
        {
            string channellist = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n<list>\r\n";

            string archiveDirectory = State.config.GetProperty("ArchiveDirectory");
            string[] files = Directory.GetFiles(archiveDirectory);
            foreach (string filename in files)
            {
                channellist += "<archive>" + new FileInfo(filename).Name + "</archive>\r\n";
            }

            channellist += "</list>\r\n";
            return channellist;
        }


        public string getBlank(string channelID)
        {
            string channel = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n";

            return (channel);
        }

        public string activateArchive(Tree archiveList)
        {
            string channel = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n\r\n";
            string data = "<result>\r\n";

            foreach (Tree archive in archiveList.tree)
            {
                string archivename = archive.Value;
                try
                {
                    // Unzip archive back into original directory

                    string inputpath = State.config.GetProperty("ArchiveDirectory") + "\\" + archivename;
                    if (File.Exists(inputpath))
                    {
                        string outputpath = State.config.GetProperty("DocumentDatabaseDirectory") + "\\" + archivename.Replace(".zip","");
                        if (!Directory.Exists(outputpath))
                        {
                            ZipFile.ExtractToDirectory(inputpath, outputpath);
                            data += "<archive>\r\n";
                            data += "<name>" + archivename + "</name>\r\n";
                            data += "<result>Success</result>\r\n";
                            data += "</archive>\r\n";
                        }
                        else
                        {
                            data += "<archive>\r\n";
                            data += "<name>" + archivename + "</name>\r\n";
                            data += "<result>Error</result>\r\n";
                            data += "<message>Archive already active.</message>\r\n";
                            data += "</archive>\r\n";
                        }
                    }
                    else
                    {
                        data += "<archive>\r\n";
                        data += "<name>" + archivename + "</name>\r\n";
                        data += "<result>Error</result>\r\n";
                        data += "<message>Archive file not found.</message>\r\n";
                        data += "</archive>\r\n";
                    }
                }
                catch (Exception xyz)
                {
                    data += "<archive>\r\n";
                    data += "<name>"+ archivename + "</name>\r\n";
                    data += "<result>Error</result>\r\n";
                    data += "<message>"+ xyz.Message+ "</message>\r\n";
                    data += "</archive>\r\n";
                }
            }
            // Load into CollectionState object

            if (State.searchSystem!=null)
            {
                State.searchSystem.updateDatabases();
            }

            data += "</result>\r\n";

            return (channel + data);
        }
    }
}
