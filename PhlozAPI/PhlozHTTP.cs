using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using System.Collections;
using FatumCore;
using PhlozLib.PhlozHTTP;

namespace PhlozLib
{
    public class PhlozWebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        CollectionState State = null;
        ArrayList Modules = new ArrayList();
        string port = "7777";

        public PhlozWebServer(CollectionState state)
        {
            State = state;

            // Add new web site modules here

            //Modules.Add(new WebmodAccount());
            //Modules.Add(new WebmodChannel());
            //Modules.Add(new WebmodConsole());
            //Modules.Add(new WebmodColors());
            //Modules.Add(new WebmodCredential());
            //Modules.Add(new WebmodFlow());
            //Modules.Add(new WebmodForwarder());
            //Modules.Add(new WebmodLogin());
            //Modules.Add(new WebmodDocument());
            //Modules.Add(new WebmodProcess());
            //Modules.Add(new WebmodRule());
            Modules.Add(new WebmodSearch());
            //Modules.Add(new WebmodService());
            //Modules.Add(new WebmodSource());
            Modules.Add(new WebmodStatus());
            Modules.Add(new WebmodStatistics());
            Modules.Add(new WebmodArchive());

            foreach (ModuleInterface current in Modules)
            {
                current.Init(State);
            }

            // URI prefixes are required, for example 
            // "http://+:8080/index/".

            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            _listener.Prefixes.Add("https://+:" + port + "/");
            _listener.Start();
        }

        public void Shutdown()
        {
            try
            {
                _listener.Stop();
            }
            catch (Exception xyz)
            {
                int guessnot = 0;
            }
        }

        private void StuckQueryCallBack(Object o, EventArgs e)
        {
            TaggedTimer timer = (TaggedTimer) o;

            var ctx = timer.Tag as HttpListenerContext;
            try
            {
                byte[] msg = null;
                msg = Encoding.ASCII.GetBytes(string.Format("<HTML><BODY>Requested Timed Out.<br>{0}</BODY></HTML>", DateTime.Now));

                ctx.Response.ContentLength64 = msg.Length;
                using (Stream s = ctx.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
            catch { } // suppress any exceptions
            finally
            {
                // always close the stream
                try
                {
                    if (ctx != null)
                    {
                        if (ctx.Request != null)
                        {
                            ctx.Request.InputStream.Close();
                        }

                        if (ctx.Response != null)
                        {
                            ctx.Response.OutputStream.Close();
                        }
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    Console.WriteLine(ex.Message + ": " + ex.StackTrace);
                }
            }
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            TaggedTimer processingTimer = new TaggedTimer(3600000);
                            processingTimer.Elapsed += new System.Timers.ElapsedEventHandler(StuckQueryCallBack);
                            processingTimer.Tag = c;
                            processingTimer.AutoReset = false;
                            processingTimer.Enabled = true;

                            var ctx = c as HttpListenerContext;
                            try
                            {
                                SendResponse(ctx);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                try
                                {
                                    if (ctx != null)
                                    {
                                        if (ctx.Request != null)
                                        {
                                            ctx.Request.InputStream.Close();
                                        }

                                        if (ctx.Response != null)
                                        {
                                            ctx.Response.OutputStream.Close();
                                        }
                                    }
                                    processingTimer.Enabled = false;
                                    processingTimer.Tag = null;
                                    processingTimer.Dispose();
                                }
                                catch (ObjectDisposedException ex)
                                {
                                    Console.WriteLine(ex.Message + ": " + ex.StackTrace);
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception xyz)
                {
                    Console.WriteLine(xyz.Message + ": " + xyz.StackTrace);
                } // suppress any exceptions
            });
        }

        public void Stop()
        {
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (Exception xyz)
            {

            }
        }

        static string GuessContentType(string ext)
        {
            switch (ext)
            {
                case ".js":
                    return "text/javascript";
                case ".htm":
                case ".html":
                    return "text/html";
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpg";
                case ".ico":
                    return "image/x-icon";
                case ".css":
                case ".scss":
                    return "text/css";
                case ".woff":
                    return "application/x-font-woff";
                case ".ttf":
                    return "application/x-font-ttf";
                case ".svg":
                    return "image/svg+xml";
                case "eot":
                    return "font/opentype";
                default:
                    return "application/octet-stream";
            }
        }

        public void SendResponse(HttpListenerContext context)
        {
            string[] request = null;
            byte[] msg = null;

            try
            {
                char[] sep = { '/' };
                request = context.Request.RawUrl.Split(sep);
                
                if (request.Length > 1)
                {
                    Boolean found = false;
                    for (int i = 0; i < Modules.Count; i++)
                    {
                        ModuleInterface module = (ModuleInterface)Modules[i];
                        found = module.IsHandler(context.Request.RawUrl);
                        if (found)
                        {
                            Tree action = InputParser.parseRequest(context);
                            i = Modules.Count;
                            module.SendRequest(context);
                            action.dispose();
                        }
                    }

                    //  If the URL fails, we simply resort to showing contents from the drop-in directory

                    if (!found)
                    {
                        string filename = Path.GetFileName(context.Request.RawUrl);
                        if (filename == "")
                        {
                            filename = "index.html";
                        }
                        else
                        {
                            filename = context.Request.RawUrl;
                            filename = filename.Replace('/', '\\');
                            filename = filename.Replace("\\..\\", "");
                            filename = filename.Replace("\\.\\", "");
                        }

                        string path = State.fatumConfig.HtmlDirectory + "\\"+filename;

                        if (!File.Exists(path))
                        {
                            Console.WriteLine("Client requested nonexistent file, sending error...");
                            Console.Write(">");
                            context.Response.ContentType = "text/html";

                            msg = Encoding.ASCII.GetBytes(string.Format("<HTML><BODY>404 Page Not Found.<br>{0}</BODY></HTML>", DateTime.Now));

                            context.Response.ContentLength64 = msg.Length;
                            using (Stream s = context.Response.OutputStream)
                                s.Write(msg, 0, msg.Length);
                        }
                        else
                        {
                            context.Response.ContentType = GuessContentType(Path.GetExtension(path));
                            msg = File.ReadAllBytes(path);

                            context.Response.ContentLength64 = msg.Length;
                            using (Stream s = context.Response.OutputStream)
                                s.Write(msg, 0, msg.Length);
                        }
                    }
                }
            }
            catch (Exception xyz)
            {
                System.Console.Out.WriteLine(xyz.Message + ": " + xyz.StackTrace);
            }
        } 
    }
}
