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
using System.Text.RegularExpressions;

namespace Proliferation.Flows.DocumentDisplay
{
    public class SyslogDisplay
    {
        public static string DocumentToHTML(string document)
        { 
            string lit;

            try
            {
                string tmp = "";

                string rawdocument = FatumLib.FromSafeString(document);
                
                Regex rx = new Regex(@"\<(?<Facility>.*?)\>((?<Timestamp>.*?:.*?:.*? ))(?<Hostname>.*?) (?<Command>.*?)(: | - (?<Id>.*?) - )(BOM)?(?<Message>.*?)$", (RegexOptions.Compiled | RegexOptions.Singleline));
                try
                {
                    Match match = rx.Match(rawdocument);
                    if (match.Success)
                    {
                        string Facility = "";
                        if (match.Groups["Facility"]!=null)
                        {
                            Facility = match.Groups["Facility"].Value;
                        }
                        string Timestamp = "";
                        if (match.Groups["Timestamp"] != null)
                        {
                            Timestamp = match.Groups["Timestamp"].ToString();
                        }
                        string Hostname = "";
                        if (match.Groups["Hostname"] != null)
                        {
                            Hostname = match.Groups["Hostname"].ToString();
                        }
                        string Command = "";
                        if (match.Groups["Command"] != null)
                        {
                            Command = match.Groups["Command"].ToString();
                        }
                        string Id = "";
                        if (match.Groups["Id"] != null)
                        {
                            Id = match.Groups["Id"].ToString();
                        }
                        string Message = "";
                        if (match.Groups["Message"] != null)
                        {
                            Message = match.Groups["Message"].ToString();
                        }

                        lit = "";
                        if (Facility != "")
                        {
                            lit += "<label style=\"color: red\"><" + Facility + "></label>";
                        }
                        if (Timestamp != "")
                        {
                            lit += "<label style=\"color: orange\">" + Timestamp + "</label>";
                        }
                        if (Hostname!="")
                        {
                            lit += "<label style=\"color: green\"> " + Hostname + "</label>";
                        }
                        if (Command != "")
                        {
                            lit += "<label style=\"color: blue\"> " + Command + ": </label>";
                        }
                        if (Id != "") 
                        {
                            lit += "<label style=\"color: purple\"> " + Id + " </label>";
                        }
                        if (Message != "") 
                        {
                            lit += "<label style=\"color: blue\">" + Message + "</label>";
                        }
                    }
                    else
                    {
                        lit = rawdocument;
                    }
                }
                catch (Exception xyz)
                {
                    lit = rawdocument;
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
