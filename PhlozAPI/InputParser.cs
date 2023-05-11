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
using System.Web;
using Fatum.FatumCore;

namespace PhlozLib.PhlozHTTP
{
    public class InputParser
    {

        public static Tree parseRequest(HttpListenerContext context)
        {
            Tree result = new Tree();

            HttpListenerRequest request = context.Request;

            if (request.ContentType == "application/xml")
            {
                Tree data = null;

                if (request.HasEntityBody)
                {
                    using (System.IO.Stream body = request.InputStream) // here we have data
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                        {
                            try
                            {
                                string xml = reader.ReadToEnd();
                                data = XMLTree.readXMLFromString(xml);
                                result.addNode(data, "Data");
                            }
                            catch (Exception xyz)
                            {
                                System.Console.Out.WriteLine(xyz.Message + ": " + xyz.StackTrace);
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        data = new Tree();

                        if (request.HasEntityBody)
                        {
                            using (System.IO.Stream body = request.InputStream) // here we have data
                            {
                                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                                {
                                    try
                                    {
                                        Boolean readingHeaders = true;

                                        while (readingHeaders)
                                        {
                                            string line = reader.ReadLine();
                                            if (line != null)
                                            {
                                                if (line == "")
                                                {
                                                    readingHeaders = false;
                                                }
                                                else
                                                {
                                                    char[] sep = { '&' };
                                                    char[] sep2 = { '=' };
                                                    string[] pairs = line.Split(sep);
                                                    foreach (string assignment in pairs)
                                                    {
                                                        string[] chunks = assignment.Split(sep2);
                                                        if (chunks.Length == 2)
                                                        {
                                                            data.addElement(chunks[0], chunks[1]);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                readingHeaders = false;
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
                        result.addNode(data, "Data");
                    }
                    catch (Exception xyz)
                    {
                        System.Console.Out.WriteLine(xyz.Message + ": " + xyz.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}
