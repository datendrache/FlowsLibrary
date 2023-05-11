using FatumCore;
using MimeKit;
using Org.BouncyCastle.Asn1.Esf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PhlozLib.DocumentDisplay
{
    public class RFC822Display
    {
        public static string DocumentToHTML(string document)
        {
            string lit;
            try
            {
                lit = "<table>";
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(document);
                writer.Flush();
                stream.Position = 0;
                var message = MimeMessage.Load(stream);
                stream.Close();
                var tmpDir = Path.Combine(Path.GetTempPath(), message.MessageId);
                var visitor = new HtmlPreviewVisitor(tmpDir);
                Directory.CreateDirectory(tmpDir);
                message.Accept(visitor);

                lit += "<tr><td>From: " + message.From + "</td></tr>";
                lit += "<tr><td>To: ";
                Boolean multiple = false;
                foreach (var person in message.To)
                {
                    if (multiple) lit += ", ";
                    lit += person.Name;
                    multiple = true;
                }
                lit += "</td></tr>";
                if (message.Cc.Count>0)
                {
                    lit += "<tr><td>";
                    multiple = false;
                    foreach (var person in message.Cc)
                    {
                        if (multiple) lit += ", ";
                        lit += person.Name;
                        multiple = true;
                    }
                    lit += "</td></tr>";
                }
                lit += "<tr><td>Date: " + message.Date.ToString() + "</td></tr>";
                lit += "<tr><td>Subject: " + message.Subject + "</td></tr>";
                lit += "<tr><td>Priority: " + message.Priority.ToString() + "</td></tr>";
                if (visitor.Attachments.Count>0)
                {
                    lit += "<tr><td>Attachments: ";
                    multiple = false;
                    foreach (var attach in visitor.Attachments)
                    {
                        if (multiple) lit += ", ";
                        attach.ToString();
                        multiple = true;
                    }
                    lit += "</td></tr>";
                }
                lit += "<tr><td>"+visitor.HtmlBody+"</td></tr>";
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
    }
}
