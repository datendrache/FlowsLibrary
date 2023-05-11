using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.IO;
using FatumCore;
using System.Net;

namespace PhlozLib
{
    public class ExtractForm
    {
        public Tree extraction = null;
        public int Error = -1;
        public string ErrorMessage = "";

        public ExtractForm(HttpListenerContext context)
        {
            if (context.Request.HttpMethod=="POST")
            {
                if (!context.Request.HasEntityBody)
                {
                    extraction = new Tree();
                }
                else
                {
                    HttpListenerRequest request = context.Request;
                    if (request.ContentLength64>(1024*1024*20))
                    {
                        ErrorMessage = "Cannot send a file greater than 20 megabytes.";
                    }
                    else
                    {
                        using (System.IO.Stream body = request.InputStream) // here we have data
                        {
                            using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                            {
                                string firstline = reader.ReadLine();
                                if (firstline.Contains("Content-Disposition:"))
                                {
                                    // Contains data

                                }
                                else
                                {
                                    extraction = new Tree();

                                    // Contains form valuesa
                                    char[] sep = { '&' };
                                    string[] assignments = firstline.Split(sep);
                                    for (int i=0;i<assignments.Length;i++)
                                    {
                                        try 
                                        {
                                            string chunk = assignments[i];
                                            int location = chunk.IndexOf("=");
                                            if (location < 0)
                                            {
                                                string property = chunk;
                                                string value = "true";
                                                extraction.addElement(property, value);
                                            }
                                            else
                                            {
                                                string property = chunk.Substring(0, location);
                                                string value;
                                                if ((location - chunk.Length - 1) != 0)
                                                {
                                                    value = chunk.Substring(location + 1, chunk.Length - location - 1);
                                                }
                                                else
                                                {
                                                    value = "";
                                                }
                                                extraction.addElement(property, value);
                                            }
                                            
                                        }
                                        catch (Exception xyz)
                                        {

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                char[] sep = { '/' };
                string[] parts = context.Request.RawUrl.Split(sep);
                string args = parts[parts.Length-1];
                sep[0] = '&';
                parts = args.Split(sep);

                extraction = new Tree();
                foreach (String key in parts)
                {
                    string property = "";
                    string value = "";

                    for (int i = 0; i < key.Length; i++)
                    {
                        if (key[i]=='=')
                        {
                            value = key.Substring(i + 1, key.Length - i - 1);
                            break;
                        }
                        else
                        {
                            property += key[i];
                        }
                    }
                    extraction.addElement(property,value);
                }
            }
        }

        ~ExtractForm()
        {
            if (extraction!=null)
            {
                extraction.dispose();
                extraction = null;
            }
        }
    }
}
