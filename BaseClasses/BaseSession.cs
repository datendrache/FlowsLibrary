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

namespace Proliferation.Flows
{
    public class BaseSession
    {
        public DateTime DateAdded;
        public DateTime DateExpires;
        public string Account = "";
        public string IPAddress = "";
        public string SessionID = "";

        ~BaseSession()
        {
            Account = null;
            IPAddress = null;
            SessionID = null;
        }

        static public string getXML(BaseSession current)
        {
            string result = "";
            Tree tmp = new Tree();
            
            tmp.AddElement("DateAdded", current.DateAdded.Ticks.ToString());
            tmp.AddElement("DateExpires", current.DateExpires.Ticks.ToString());
            tmp.AddElement("Account", current.Account);
            tmp.AddElement("IPAddress", current.IPAddress);
            tmp.AddElement("SessionID", current.SessionID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseSession");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }
    }
}
