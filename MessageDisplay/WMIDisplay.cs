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
    public class WMIDisplay
    {
        public static string DocumentToHTML(string document)
        { 
            string lit = "";

            try
            {
                string tmp = "";

                Tree data = XMLTree.ReadXmlFromString(FatumLib.FromSafeString(document));
                Tree eventdata = data.FindNode("EventData");
                lit = "<table cssclass=\"documenttext\">";
                tmp = eventdata.GetElement("ComputerName");
                lit += "<tr><td><span>Computer Name</span></td><td>" + tmp + "</td></tr>";
                tmp = eventdata.GetElement("SourceName");
                lit += "<tr><td><span>Source</span></td><td>" + tmp + "</td></tr>";
                tmp = eventdata.GetElement("Type");
                lit += "<tr><td><span>Type</span></td><td>" + tmp + "</td></tr>";
                tmp = eventdata.GetElement("Document");
                lit += "<tr><td><span>Message</span></td><td>" + tmp + "</td></tr>";
                lit += "</table>";
                data.Dispose();
            }
            catch (Exception xyz)
            {
                lit = document;
            }

            return lit;
        }
    }
}
