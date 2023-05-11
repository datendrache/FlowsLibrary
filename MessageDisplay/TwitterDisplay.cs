using FatumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib.DocumentDisplay
{
    public class TwitterDisplay
    {
        public static string DocumentToHTML(string document)
        {
            string lit = "";
            try
            {
                string tmp = "";

                Tree data = TreeDataAccess.readJSONFromString(FatumLib.fromSafeString(document));
                Tree eventdata = data.findNode("Tweet");
                if (eventdata != null)
                {  // Hashtag Tweet
                    Tree userinfo = eventdata.findNode("user");
                    lit = "<table>";
                    tmp = eventdata.getElement("created_at");
                    lit += "<tr><td><span>Timestamp:</span></td><td>" + tmp + "</td></tr>";

                    string references = "";
                    Tree entities = eventdata.findNode("entities");
                    if (entities != null)
                    {
                        Tree hashtags = entities.findNode("hashtags");
                        if (hashtags != null)
                        {
                            foreach (Tree line in hashtags.tree)
                            {
                                if (line.getElement("name") != "")
                                {
                                    string name = "#" + line.getElement("name");
                                    string url = line.getElement("expanded_url");
                                    references += "<a href=\"" + url + "\" target=\"new\">" + name + "</a>";
                                }
                            }
                        }

                        Tree users = entities.findNode("user_mentions");
                        if (users != null)
                        {
                            foreach (Tree line in users.tree)
                            {
                                if (line.getElement("name") != "")
                                {
                                    string name = "@" + line.getElement("screen_name");
                                    string fullname = line.getElement("name");
                                    references += name + " (" + fullname + ")";
                                }
                            }
                        }
                        if (references != "")
                        {
                            lit += "<tr><td><span>Mentions:</span></td><td>" + references + "</td></tr>";
                        }

                        string medialines = "";
                        Tree media = entities.findNode("media");
                        if (media != null)
                        {
                            string media_url = media.getElement("media_url");
                            if (media_url != "")
                            {
                                medialines += "<a href=\"" + media.getElement("url") + "\" target=\"new\"><img src=\"" + media_url + "\" height=\"128\" width=\"128\"></a>";
                            }
                        }
                        if (medialines != "")
                        {
                            lit += "<tr><td><span>Media:</span></td><td>" + medialines + "</td></tr>";
                        }
                    }

                    if (userinfo != null)
                    {
                        tmp = userinfo.getElement("screen_name");
                        lit += "<tr><td><span>Screen name:</span></td><td>@" + tmp + " (" + userinfo.getElement("name") + ")</td></tr>";
                    }

                    tmp = eventdata.getElement("text");
                    lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";
                    lit += "</table>";
                    eventdata.dispose();
                }
                else // User Tweet
                {
                    Tree userinfo = data.findNode("user");
                    lit = "<table>";
                    tmp = data.getElement("created_at");
                    lit += "<tr><td><span>Timestamp:</span></td><td>" + tmp + "</td></tr>";

                    string references = "";
                    Tree entities = data.findNode("entities");
                    if (entities != null)
                    {
                        Tree hashtags = entities.findNode("hashtags");
                        if (hashtags != null)
                        {
                            foreach (Tree line in hashtags.tree)
                            {
                                if (line.getElement("name") != "")
                                {
                                    string name = "#" + line.getElement("name");
                                    string url = line.getElement("expanded_url");
                                    references += "<a href=\"" + url + "\" target=\"new\">" + name + "</a>";
                                }
                            }
                        }

                        Tree users = entities.findNode("user_mentions");
                        if (users != null)
                        {
                            foreach (Tree line in users.tree)
                            {
                                if (line.getElement("name") != "")
                                {
                                    string name = "@" + line.getElement("screen_name");
                                    string fullname = line.getElement("name");
                                    references += name + " (" + fullname + ")";
                                }
                            }
                        }
                        if (references != "")
                        {
                            lit += "<tr><td><span>Mentions:</span></td><td>" + references + "</td></tr>";
                        }

                        string medialines = "";
                        Tree media = entities.findNode("media");
                        if (media != null)
                        {
                            string media_url = media.getElement("media_url");
                            if (media_url != "")
                            {
                                medialines += "<a href=\"" + media.getElement("url") + "\" target=\"new\"><img src=\"" + media_url + "\" height=\"128\" width=\"128\"></a>";
                            }
                        }
                        if (medialines != "")
                        {
                            lit += "<tr><td><span>Media:</span></td><td>" + medialines + "</td></tr>";
                        }
                    }

                    if (userinfo != null)
                    {
                        tmp = userinfo.getElement("screen_name");
                        lit += "<tr><td><span>Screen name:</span></td><td>@" + tmp + " (" + userinfo.getElement("name") + ")</td></tr>";
                    }

                    tmp = data.getElement("text");
                    lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";
                    lit += "</table>";
                }
                
               
            }
            catch (Exception xyz)
            {
                lit = document;
            }
            return lit;
        }
    }
}
