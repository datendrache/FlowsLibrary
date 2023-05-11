//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using DatabaseAdapters;
using FatumCore;
using System.IO;

namespace PhlozLib
{
    public class ThreadPostLink
    {
        public int PostID = -1;
        public int ThreadID = -1;

        static public void addProcessLink(IntDatabase managementDB, ThreadPostLink link)
        {
            string sql = "INSERT INTO [PostLinks] ([ThreadID], [PostID]) VALUES (@threadid, @postid);";

            Tree NewLink = new Tree();
            NewLink.addElement("@threadid", link.PostID.ToString());
            NewLink.addElement("@postid", link.ThreadID.ToString());
            managementDB.ExecuteDynamic(sql, NewLink);
            NewLink.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            configDB = "CREATE TABLE [PostLinks](" +
            "[ThreadID] INTEGER NULL, " +
            "[PostID] INTEGER NULL;";
            database.ExecuteNonQuery(configDB);
        }
    }
}
