//   Flows Libraries -- Flows Common Classes and Methods
//
//   Copyright (C) 2003-2023 Eric Knight
//   This software is distributed under the GNU Public v3 License
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Proliferation.Fatum;
using System.Collections;
using System.Web;

namespace Proliferation.Flows.DocumentDisplay
{
    public class RSSDisplay
    {
        public static string DocumentToHTML(string document)
        {
            string lit;
            try
            {
                string tmp = "";
                string xml = cleanPrefixes(FatumLib.FromSafeString(document));
                xml = replaceURLAmps(xml);
                Tree data = XMLTree.ReadXmlFromString(xml);
                lit = "<table>";
                tmp = data.GetElement("pubDate");
                if (tmp!="")
                {
                    lit += "<tr><td><span>Timestamp:</span></td><td>" + tmp + "</td></tr>";
                }

                tmp = data.GetElement("dccreator");
                if (tmp != "")
                {
                    lit += "<tr><td><span>Creator:</span></td><td>" + tmp + "</td></tr>";
                }

                string title = data.GetElement("title");
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

                string description = data.GetElement("description");
                if (description!="")
                {
                    lit += "<tr><td><span>Description:</span></td><td>" + description + "</td></tr>";
                }

                tmp = data.GetElement("text");
                lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";

                string medialines = "";
                Tree media = data.FindNode("mediacontent");
                if (media != null)
                {
                    string media_url = media.GetAttribute("url");
                    if (media_url != "")
                    {
                        medialines += "<a href=\"" + media.GetElement("url") + "\" target=\"new\"><img src=\"" + media_url + "\" height=\"128\" width=\"128\"></a>";
                    }
                }
                if (medialines != "")
                {
                    lit += "<tr><td><span>Media:</span></td><td>" + medialines + "</td></tr>";
                }

                Tree content = data.FindNode("contentencoded");
                if (content != null)
                {
                    string contentlines = content.GetElement("CDATA");
                    string attrib = content.GetAttribute("HexConv");

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
                data.Dispose();

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
        }
    }
}
