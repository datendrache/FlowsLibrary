using FatumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhlozLib.DocumentDisplay
{
    public class SyslogDisplay
    {
        public static string DocumentToHTML(string document)
        { 
            string lit;

            try
            {
                string tmp = "";

                string rawdocument = FatumLib.fromSafeString(document);
                
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
