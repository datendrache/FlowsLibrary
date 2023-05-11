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
    public class BaseAuthenticate
    {
        public string DateAdded = "";
        public string AccountID = "";
        public string Hash = "";

        ~BaseAuthenticate()
        {
            DateAdded = null;
            AccountID = null;
            Hash = null;
        }

        static public ArrayList loadMatchingAuthentications(IntDatabase managementDB, string Hash)
        {
            DataTable authEntries;
            String query = "select * from [Authentications] where [Hash]=@hash;";
            Tree data = new Tree();
            data.setElement("@hash", Hash);
            authEntries = managementDB.ExecuteDynamic(query,data);
            data.dispose();

            ArrayList matchingAuths = new ArrayList();
            foreach (DataRow row in authEntries.Rows)
            {
                BaseAuthenticate newLink = new BaseAuthenticate();
                newLink.DateAdded = row["DateAdded"].ToString();
                newLink.AccountID = row["AccountID"].ToString();
                newLink.Hash = row["Hash"].ToString();
                matchingAuths.Add(newLink);
            }

            return matchingAuths;
        }

        static public void addAuthentication(IntDatabase managementDB, BaseAuthenticate authInfo)
        {
                string sql = "INSERT INTO [Authentications] ([DateAdded], [AccountID], [Hash]) VALUES (@DateAdded, @AccountID, @Hash);";       
                Tree newAuthenticate = new Tree();
                newAuthenticate.addElement("@DateAdded", DateTime.Now.Ticks.ToString());
                newAuthenticate.addElement("@AccountID", authInfo.AccountID);
                newAuthenticate.addElement("@Hash", authInfo.Hash);
                managementDB.ExecuteDynamic(sql, newAuthenticate);
                newAuthenticate.dispose();
        }

        static public void removeAuthenticateByAccount(IntDatabase managementDB, string accountid)
        {
            String squery = "delete from [Authentications] where [AccountID]=@accountid;";
            Tree data = new Tree();
            data.setElement("@accountid", accountid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Authentications](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[AccountID] TEXT NULL, " +
                    "[Hash] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Authentications](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[AccountID] VARCHAR(33) NULL, " +
                    "[Hash] VARCHAR(52) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }
    }
}
