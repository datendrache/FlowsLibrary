//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using DatabaseAdapters;

namespace PhlozLib
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
            parms.addElement("@GroupID", GroupID);
            groupmembers = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();
            return groupmembers;
        }

        static public void addMember(IntDatabase managementDB, BaseMember perm)
        {
            if (getMembers(managementDB,perm).Rows.Count==0)   // Check to see if already a member. If already a member, don't do anything.
            {
                Tree NewPermission = new Tree();
                NewPermission.addElement("MemberID", perm.MemberID);
                NewPermission.addElement("GroupID", perm.GroupID);
                managementDB.InsertTree("Members", NewPermission);
                NewPermission.dispose();
            }
        }

        static public void deleteMember(IntDatabase managementDB, BaseMember perm)
        {
            Tree NewPermission = new Tree();
            NewPermission.addElement("MemberID", perm.MemberID);
            NewPermission.addElement("GroupID", perm.GroupID);
            managementDB.DeleteTree("[Members]", NewPermission, "where [MemberID]=@MemberID and [GroupID]=@GroupID");
            NewPermission.dispose();
        }

        static public DataTable getMembers(IntDatabase managementDB, BaseMember perm)
        {
            Tree allMembers = new Tree();
            string SQL = "select * from [Members] where [GroupID]=@GroupID;";
            allMembers.addElement("@GroupID", perm.GroupID);
            DataTable result = managementDB.ExecuteDynamic(SQL, allMembers);
            allMembers.dispose();
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

            tmp.addElement("ID", current.ID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("MemberID", current.MemberID);
            
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseMember");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }
    }
}
