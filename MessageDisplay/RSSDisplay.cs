using Fatum.FatumCore;
using FatumCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhlozLib.DocumentDisplay
{
    public class RSSDisplay
    {
        public static string DocumentToHTML(string document)
        {
            string lit;
            try
            {
                string tmp = "";
                string xml = cleanPrefixes(FatumLib.fromSafeString(document));
                xml = replaceURLAmps(xml);
                Tree data = XMLTree.readXMLFromString(xml);
                lit = "<table>";
                tmp = data.getElement("pubDate");
                if (tmp!="")
                {
                    lit += "<tr><td><span>Timestamp:</span></td><td>" + tmp + "</td></tr>";
                }

                tmp = data.getElement("dccreator");
                if (tmp != "")
                {
                    lit += "<tr><td><span>Creator:</span></td><td>" + tmp + "</td></tr>";
                }

                string title = data.getElement("title");
                if (title != "")
                {
                    lit += "<tr><td><span>Title:</span></td><td>@" + title + "</td></tr>";
                }

                ArrayList categories = new ArrayList();
                for (int i=0;i<data.leafnames.Count;i++)
                {
                    string current = (string)data.leafnames[i];
                    if (current.ToLower() == "category")
                    {
                        Tree catagorydata = (Tree)data.tree[i];
                        categories.Add(catagorydata.Value);
                    }
                }

                if (categories.Count>0)
                {
                    tmp = "";
                    foreach (string current in categories)
                    {
                        if (tmp!="")
                        {
                            tmp += " ";
                        }
                        tmp += current;
                    }
                    lit += "<tr><td><span>Category Tags:</span></td><td>" + tmp + "</td></tr>";
                }
                categories.Clear();

                string description = data.getElement("description");
                if (description!="")
                {
                    lit += "<tr><td><span>Description:</span></td><td>" + description + "</td></tr>";
                }

                tmp = data.getElement("text");
                lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";

                string medialines = "";
                Tree media = data.findNode("mediacontent");
                if (media != null)
                {
                    string media_url = media.getAttribute("url");
                    if (media_url != "")
                    {
                        medialines += "<a href=\"" + media.getElement("url") + "\" target=\"new\"><img src=\"" + media_url + "\" height=\"128\" width=\"128\"></a>";
                    }
                }
                if (medialines != "")
                {
                    lit += "<tr><td><span>Media:</span></td><td>" + medialines + "</td></tr>";
                }

                Tree content = data.findNode("contentencoded");
                if (content != null)
                {
                    string contentlines = content.getElement("CDATA");
                    string attrib = content.getAttribute("HexConv");

                    if (attrib != null)
                    {
                        if (attrib.ToLower().Contains("true"))
                        {
                            contentlines = HttpUtility.HtmlDecode(contentlines);
                        }
                    }

                    if (contentlines!="")
                    {
                        lit += "<tr><td><span>content:</span></td><td>" + contentlines + "</td></tr>";
                    }
                    else
                    {
                        int ytt = 1;
                    }
                }
                data.dispose();

                lit += "</table>";
            }
            catch (Exception xyz)
            {
                lit = document;
            }
            return lit;
        }

        private static string cleanPrefixes(string input)
        {
            string result;
            result = input.Replace("dc:creator", "dccreator");
            result = result.Replace("content:encoded", "contentencoded");
            result = result.Replace("wfw:commentRss", "wfwcommentRss");
            result = result.Replace("media:content", "mediacontent");
            result = result.Replace("media:description", "mediadescription");
            result = result.Replace("dcterms:created", "dctermscreated");
            result = result.Replace("dcterms:modified", "dctermsmodified");
            result = result.Replace("dcterms:created", "dctermscreated");
            result = result.Replace("slash:comments", "slashcomments");
            return result;
        }

        private static string replaceURLAmps(string input)
        {
            return input.Replace("&", "&amp;");

            //Boolean replacing = true;
            //int lastindex = 0;
            //do
            //{
            //    int locationstart = input.IndexOf("url=\"", lastindex);
            //    if (locationstart > -1)
            //    {
            //        int locationend = input.IndexOf("\"", locationstart + 5);
            //        string tmp = result.Substring(locationstart, locationend - locationstart);
            //        string replacewith = tmp.Replace("&", "&amp;");
            //        result = result.Replace(tmp, replacewith);
            //        lastindex = locationend;
            //    }
            //    else
            //    {
            //        replacing = false;
            //    }
            //}
            //while (replacing);

            //replacing = true;
            //lastindex = 0;
            //do
            //{
            //    int locationstart = input.IndexOf("<CDATA>", lastindex);
            //    if (locationstart > -1)
            //    {
            //        int locationend = input.IndexOf("</CDATA>", locationstart + 8);
            //        string tmp = result.Substring(locationstart, locationend - locationstart);
            //        string replacewith = tmp.Replace("&", "&amp;");
            //        result = result.Replace(tmp, replacewith);
            //        lastindex = locationend;
            //    }
            //    else
            //    {
            //        replacing = false;
            //    }
            //}
            //while (replacing);

            //return result;
        }
    }
}
