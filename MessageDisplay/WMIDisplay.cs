using Fatum.FatumCore;
using FatumCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib.DocumentDisplay
{
    public class WMIDisplay
    {
        public static string DocumentToHTML(string document)
        { 
            string lit = "";

            try
            {
                string tmp = "";

                Tree data = XMLTree.readXMLFromString(FatumLib.fromSafeString(document));
                Tree eventdata = data.findNode("EventData");
                lit = "<table cssclass=\"documenttext\">";
                tmp = eventdata.getElement("ComputerName");
                lit += "<tr><td><span>Computer Name</span></td><td>" + tmp + "</td></tr>";
                tmp = eventdata.getElement("SourceName");
                lit += "<tr><td><span>Source</span></td><td>" + tmp + "</td></tr>";
                tmp = eventdata.getElement("Type");
                lit += "<tr><td><span>Type</span></td><td>" + tmp + "</td></tr>";
                tmp = eventdata.getElement("Document");
                lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";
                lit += "</table>";
                data.dispose();
            }
            catch (Exception xyz)
            {
                lit = document;
            }

            return lit;
        }
    }
}
