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


using System.Data;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseMember
    {
        public string ID = "";
        public string GroupID = "";
        public string MemberID = "";

        ~BaseMember()
        {
            ID = null;
            GroupID = null;
            MemberID = null;
        }

        static public DataTable loadMembers(IntDatabase managementDB, String GroupID)
        {
            DataTable groupmembers;
            String query = "select * from [Members] where [GroupID]=@GroupID;";
            Tree parms = new Tree();
            parms.AddElement("@GroupID", GroupID);
            groupmembers = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();
            return groupmembers;
        }

        static public void addMember(IntDatabase managementDB, BaseMember perm)
        {
            if (getMembers(managementDB,perm).Rows.Count==0)   // Check to see if already a member. If already a member, don't do anything.
            {
                Tree NewPermission = new Tree();
                NewPermission.AddElement("MemberID", perm.MemberID);
                NewPermission.AddElement("GroupID", perm.GroupID);
                managementDB.InsertTree("Members", NewPermission);
                NewPermission.Dispose();
            }
        }

        static public void deleteMember(IntDatabase managementDB, BaseMember perm)
        {
            Tree NewPermission = new Tree();
            NewPermission.AddElement("MemberID", perm.MemberID);
            NewPermission.AddElement("GroupID", perm.GroupID);
            managementDB.DeleteTree("[Members]", NewPermission, "where [MemberID]=@MemberID and [GroupID]=@GroupID");
            NewPermission.Dispose();
        }

        static public DataTable getMembers(IntDatabase managementDB, BaseMember perm)
        {
            Tree allMembers = new Tree();
            string SQL = "select * from [Members] where [GroupID]=@GroupID;";
            allMembers.AddElement("@GroupID", perm.GroupID);
            DataTable result = managementDB.ExecuteDynamic(SQL, allMembers);
            allMembers.Dispose();
            return result;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Members]( [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                    "[MemberID] TEXT NULL, " +
                    "[GroupID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Members]( [ID] INTEGER NOT NULL IDENTITY PRIMARY KEY, " + 
                    "[Permission] SMALLINT NULL, " +
                    "[MemberID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public string getXML(BaseMember current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("ID", current.ID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("MemberID", current.MemberID);
            
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseMember");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }
    }
}
