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

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using DatabaseAdapters;
using Proliferation.Fatum;
using System.IO;

namespace Proliferation.Flows
{
    public class ThreadPostLink
    {
        public int PostID = -1;
        public int ThreadID = -1;

        static public void addProcessLink(IntDatabase managementDB, ThreadPostLink link)
        {
            string sql = "INSERT INTO [PostLinks] ([ThreadID], [PostID]) VALUES (@threadid, @postid);";

            Tree NewLink = new Tree();
            NewLink.AddElement("@threadid", link.PostID.ToString());
            NewLink.AddElement("@postid", link.ThreadID.ToString());
            managementDB.ExecuteDynamic(sql, NewLink);
            NewLink.Dispose();
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
