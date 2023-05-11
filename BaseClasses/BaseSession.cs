//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;

namespace PhlozLib
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
            
            tmp.addElement("DateAdded", current.DateAdded.Ticks.ToString());
            tmp.addElement("DateExpires", current.DateExpires.Ticks.ToString());
            tmp.addElement("Account", current.Account);
            tmp.addElement("IPAddress", current.IPAddress);
            tmp.addElement("SessionID", current.SessionID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseSession");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }
    }
}
