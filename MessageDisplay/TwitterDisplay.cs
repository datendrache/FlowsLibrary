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

namespace Proliferation.Flows.DocumentDisplay
{
    public class TwitterDisplay
    {
        public static string DocumentToHTML(string document)
        {
            string lit = "";
            try
            {
                string tmp = "";

                Tree data = TreeDataAccess.ReadJsonFromString(FatumLib.FromSafeString(document));
                Tree eventdata = data.FindNode("Tweet");
                if (eventdata != null)
                {  // Hashtag Tweet
                    Tree userinfo = eventdata.FindNode("user");
                    lit = "<table>";
                    tmp = eventdata.GetElement("created_at");
                    lit += "<tr><td><span>Timestamp:</span></td><td>" + tmp + "</td></tr>";

                    string references = "";
                    Tree entities = eventdata.FindNode("entities");
                    if (entities != null)
                    {
                        Tree hashtags = entities.FindNode("hashtags");
                        if (hashtags != null)
                        {
                            foreach (Tree line in hashtags.tree)
                            {
                                if (line.GetElement("name") != "")
                                {
                                    string name = "#" + line.GetElement("name");
                                    string url = line.GetElement("expanded_url");
                                    references += "<a href=\"" + url + "\" target=\"new\">" + name + "</a>";
                                }
                            }
                        }

                        Tree users = entities.FindNode("user_mentions");
                        if (users != null)
                        {
                            foreach (Tree line in users.tree)
                            {
                                if (line.GetElement("name") != "")
                                {
                                    string name = "@" + line.GetElement("screen_name");
                                    string fullname = line.GetElement("name");
                                    references += name + " (" + fullname + ")";
                                }
                            }
                        }
                        if (references != "")
                        {
                            lit += "<tr><td><span>Mentions:</span></td><td>" + references + "</td></tr>";
                        }

                        string medialines = "";
                        Tree media = entities.FindNode("media");
                        if (media != null)
                        {
                            string media_url = media.GetElement("media_url");
                            if (media_url != "")
                            {
                                medialines += "<a href=\"" + media.GetElement("url") + "\" target=\"new\"><img src=\"" + media_url + "\" height=\"128\" width=\"128\"></a>";
                            }
                        }
                        if (medialines != "")
                        {
                            lit += "<tr><td><span>Media:</span></td><td>" + medialines + "</td></tr>";
                        }
                    }

                    if (userinfo != null)
                    {
                        tmp = userinfo.GetElement("screen_name");
                        lit += "<tr><td><span>Screen name:</span></td><td>@" + tmp + " (" + userinfo.GetElement("name") + ")</td></tr>";
                    }

                    tmp = eventdata.GetElement("text");
                    lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";
                    lit += "</table>";
                    eventdata.Dispose();
                }
                else // User Tweet
                {
                    Tree userinfo = data.FindNode("user");
                    lit = "<table>";
                    tmp = data.GetElement("created_at");
                    lit += "<tr><td><span>Timestamp:</span></td><td>" + tmp + "</td></tr>";

                    string references = "";
                    Tree entities = data.FindNode("entities");
                    if (entities != null)
                    {
                        Tree hashtags = entities.FindNode("hashtags");
                        if (hashtags != null)
                        {
                            foreach (Tree line in hashtags.tree)
                            {
                                if (line.GetElement("name") != "")
                                {
                                    string name = "#" + line.GetElement("name");
                                    string url = line.GetElement("expanded_url");
                                    references += "<a href=\"" + url + "\" target=\"new\">" + name + "</a>";
                                }
                            }
                        }

                        Tree users = entities.FindNode("user_mentions");
                        if (users != null)
                        {
                            foreach (Tree line in users.tree)
                            {
                                if (line.GetElement("name") != "")
                                {
                                    string name = "@" + line.GetElement("screen_name");
                                    string fullname = line.GetElement("name");
                                    references += name + " (" + fullname + ")";
                                }
                            }
                        }
                        if (references != "")
                        {
                            lit += "<tr><td><span>Mentions:</span></td><td>" + references + "</td></tr>";
                        }

                        string medialines = "";
                        Tree media = entities.FindNode("media");
                        if (media != null)
                        {
                            string media_url = media.GetElement("media_url");
                            if (media_url != "")
                            {
                                medialines += "<a href=\"" + media.GetElement("url") + "\" target=\"new\"><img src=\"" + media_url + "\" height=\"128\" width=\"128\"></a>";
                            }
                        }
                        if (medialines != "")
                        {
                            lit += "<tr><td><span>Media:</span></td><td>" + medialines + "</td></tr>";
                        }
                    }

                    if (userinfo != null)
                    {
                        tmp = userinfo.GetElement("screen_name");
                        lit += "<tr><td><span>Screen name:</span></td><td>@" + tmp + " (" + userinfo.GetElement("name") + ")</td></tr>";
                    }

                    tmp = data.GetElement("text");
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
