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

using System.Collections;
using System.Data;
using DatabaseAdapters;
using Proliferation.Fatum;

namespace Proliferation.Flows
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
            data.SetElement("@hash", Hash);
            authEntries = managementDB.ExecuteDynamic(query,data);
            data.Dispose();

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
                newAuthenticate.AddElement("@DateAdded", DateTime.Now.Ticks.ToString());
                newAuthenticate.AddElement("@AccountID", authInfo.AccountID);
                newAuthenticate.AddElement("@Hash", authInfo.Hash);
                managementDB.ExecuteDynamic(sql, newAuthenticate);
                newAuthenticate.Dispose();
        }

        static public void removeAuthenticateByAccount(IntDatabase managementDB, string accountid)
        {
            String squery = "delete from [Authentications] where [AccountID]=@accountid;";
            Tree data = new Tree();
            data.SetElement("@accountid", accountid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
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
