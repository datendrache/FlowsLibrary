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
using System.Security.Cryptography;
using DatabaseAdapters;
using Proliferation.Fatum;

namespace Proliferation.Flows
{
    public class BaseAccount
    {
        public string DateAdded = "";
        public string DisplayName = "";
        public string AccountName = "";
        public string AccountEnabled = "";
        public string EmailAddress = "";
        public string FirstName = "";
        public string LastName = "";
        public string Validated = "";
        public string MustChangePassword = "";
        public string Retired = "";
        public string LockedOut = "";
        public string PasswordExpires = "";
        public string PasswordHash = "";
        public string UniqueID = "";
        public string LastLogin = "";
        public string GroupID = "";
        public string CredentialID = "";
        public string Role = "";
        public string IconURL = "";
        public string ParameterID = "";
        public string PhoneNumber = "";

        ~BaseAccount()
        {
        DisplayName = null;
        DateAdded = null;
        AccountName = null;
        AccountEnabled = null;
        EmailAddress = null;
        FirstName = null;;
        LastName = null;
        Validated = null;
        MustChangePassword = null;;
        Retired = null;
        LockedOut = null;
        PasswordExpires = null;
        PasswordHash = null;
        UniqueID = null;
        LastLogin = null;
        GroupID = null;
        CredentialID = null;
        Role = null;
        IconURL = null;
        ParameterID = null;
            PhoneNumber = null;
    }

        static public BaseAccount loadAccountByUsername(IntDatabase managementDB, string username)
        {
            DataTable processors;
            BaseAccount result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Accounts] where AccountName=@username limit 1;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select TOP (1) * from [Accounts] where AccountName=@username;";
                    break;
            }

            Tree parms = new Tree();
            parms.AddElement("@username", username);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseAccount newAccount = new BaseAccount();
                newAccount.DateAdded = row["DateAdded"].ToString();
                newAccount.DisplayName = row["DisplayName"].ToString();
                newAccount.AccountName = row["accountname"].ToString();
                newAccount.AccountEnabled = row["AccountEnabled"].ToString();
                newAccount.EmailAddress = row["EmailAddress"].ToString();
                newAccount.FirstName = row["FirstName"].ToString();
                newAccount.LastName = row["LastName"].ToString();
                newAccount.Validated = row["Validated"].ToString();
                newAccount.MustChangePassword = row["MustChangePassword"].ToString();
                newAccount.Retired = row["Retired"].ToString();
                newAccount.LockedOut = row["LockedOut"].ToString();
                newAccount.PasswordExpires = row["PasswordExpires"].ToString();
                newAccount.PasswordHash = row["PasswordHash"].ToString();
                newAccount.CredentialID = row["CredentialID"].ToString();
                newAccount.UniqueID = row["UniqueID"].ToString();
                newAccount.GroupID = row["GroupID"].ToString();
                newAccount.LastLogin = row["LastLogin"].ToString();
                newAccount.Role = row["Role"].ToString();
                newAccount.ParameterID = row["ParameterID"].ToString();
                newAccount.IconURL = row["IconURL"].ToString();
                newAccount.PhoneNumber = row["PhoneNumber"].ToString();
                result = newAccount;

            }
            return result;
        }

        static public BaseAccount loadAccountByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            BaseAccount result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Accounts] where UniqueID=@uid limit 1;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select TOP (1) * from [Accounts] where UniqueID=@uid;";
                    break;
            }
            
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseAccount newAccount = new BaseAccount();
                newAccount.DateAdded = row["DateAdded"].ToString();
                newAccount.DisplayName = row["DisplayName"].ToString();
                newAccount.AccountName = row["accountname"].ToString();
                newAccount.AccountEnabled = row["AccountEnabled"].ToString();
                newAccount.EmailAddress = row["EmailAddress"].ToString();
                newAccount.FirstName = row["FirstName"].ToString();
                newAccount.LastName = row["LastName"].ToString();
                newAccount.Validated = row["Validated"].ToString();
                newAccount.MustChangePassword = row["MustChangePassword"].ToString();
                newAccount.Retired = row["Retired"].ToString();
                newAccount.LockedOut = row["LockedOut"].ToString();
                newAccount.PasswordExpires = row["PasswordExpires"].ToString();
                newAccount.PasswordHash = row["PasswordHash"].ToString();
                newAccount.CredentialID = row["CredentialID"].ToString();
                newAccount.UniqueID = row["UniqueID"].ToString();
                newAccount.GroupID = row["GroupID"].ToString();
                newAccount.LastLogin = row["LastLogin"].ToString();
                newAccount.Role = row["Role"].ToString();
                newAccount.ParameterID = row["ParameterID"].ToString();
                newAccount.IconURL = row["IconURL"].ToString();
                newAccount.PhoneNumber = row["PhoneNumber"].ToString();
                result = newAccount;
            }
            return result;
        }

        static public void loadAccount(IntDatabase managementDB, BaseAccount newAccount)
        {
            DataTable processors;

            if (FatumLib.ValidateEmail(newAccount.AccountName))
            {
                String query = "select * from [Accounts] where EmailAddress=@accountname";

                Tree parms = new Tree();
                parms.AddElement("@accountname", newAccount.AccountName);
                processors = managementDB.ExecuteDynamic(query, parms);
                parms.Dispose();

                foreach (DataRow row in processors.Rows)
                {
                    newAccount.DateAdded = row["DateAdded"].ToString();
                    newAccount.DisplayName = row["DisplayName"].ToString();
                    newAccount.AccountName = row["accountname"].ToString();
                    newAccount.AccountEnabled = row["AccountEnabled"].ToString();
                    newAccount.EmailAddress = row["EmailAddress"].ToString();
                    newAccount.FirstName = row["FirstName"].ToString();
                    newAccount.LastName = row["LastName"].ToString();
                    newAccount.Validated = row["Validated"].ToString();
                    newAccount.MustChangePassword = row["MustChangePassword"].ToString();
                    newAccount.Retired = row["Retired"].ToString();
                    newAccount.LockedOut = row["LockedOut"].ToString();
                    newAccount.PasswordExpires = row["PasswordExpires"].ToString();
                    newAccount.PasswordHash = row["PasswordHash"].ToString();
                    newAccount.CredentialID = row["CredentialID"].ToString();
                    newAccount.UniqueID = row["UniqueID"].ToString();
                    newAccount.GroupID = row["GroupID"].ToString();
                    newAccount.LastLogin = row["LastLogin"].ToString();
                    newAccount.Role = row["Role"].ToString();
                    newAccount.ParameterID = row["ParameterID"].ToString();
                    newAccount.IconURL = row["IconURL"].ToString();
                    newAccount.PhoneNumber = row["PhoneNumber"].ToString();
                }
            }
        }

        static public ArrayList searchAccounts(IntDatabase managementDB, string search)
        {

            DataTable processors;
            ArrayList tmpProcessors = new ArrayList();

            if (FatumLib.ValidateNoSymbols(search))
            {
                String query = "";
                if (search.Length > 0)
                {
                    query = "select * from [Accounts] where (EmailAddress glob @accountname OR FirstName glob @accountname OR LastName glob @accountname) Order By EmailAddress ASC limit 250;";
                    Tree parms = new Tree();
                    parms.AddElement("@accountname", search);
                    processors = managementDB.ExecuteDynamic(query, parms);
                    parms.Dispose();
                }
                else
                {
                    query = "select * from [Accounts] Order By EmailAddress ASC limit 250;";
                    processors = managementDB.Execute(query);
                }

                foreach (DataRow row in processors.Rows)
                {
                    BaseAccount newAccount = new BaseAccount();
                    newAccount.DateAdded = row["DateAdded"].ToString();
                    newAccount.DisplayName = row["DisplayName"].ToString();
                    newAccount.AccountName = row["AccountName"].ToString();
                    newAccount.AccountEnabled = row["AccountEnabled"].ToString();
                    newAccount.EmailAddress = row["EmailAddress"].ToString();
                    newAccount.FirstName = row["FirstName"].ToString();
                    newAccount.LastName = row["LastName"].ToString();
                    newAccount.Validated = row["Validated"].ToString();
                    newAccount.MustChangePassword = row["MustChangePassword"].ToString();
                    newAccount.Retired = row["Retired"].ToString();
                    newAccount.LockedOut = row["LockedOut"].ToString();
                    newAccount.PasswordExpires = row["PasswordExpires"].ToString();
                    newAccount.PasswordHash = row["PasswordHash"].ToString();
                    newAccount.CredentialID = row["CredentialID"].ToString();
                    newAccount.UniqueID = row["UniqueID"].ToString();
                    newAccount.GroupID = row["GroupID"].ToString();
                    newAccount.LastLogin = row["LastLogin"].ToString();
                    newAccount.ParameterID = row["ParameterID"].ToString();
                    newAccount.IconURL = row["IconURL"].ToString();
                    newAccount.PhoneNumber = row["PhoneNumber"].ToString();
                    tmpProcessors.Add(newAccount);
                }
            }
            return tmpProcessors;
        }

        static public Boolean Authenticate(IntDatabase managementDB, string account, string pwhash)
        {
            Boolean result = false;
            DataTable processors;

            if (SecurityInputSanitizer.SafetyCheck(SecurityInputSanitizer.USERNAME, account))
            {
                String query = "select * from [Accounts] where [AccountName]=@accountname AND [PasswordHash]=@pwhash and [Retired]='false' and [AccountEnabled]='true' and [Validated]='true';";
                Tree parms = new Tree();
                parms.AddElement("@accountname", account);
                parms.AddElement("@pwhash", pwhash);
                processors = managementDB.ExecuteDynamic(query, parms);
                parms.Dispose();

                if (processors.Rows.Count>0)
                {
                    result = true;
                }
            }
            return result;
        }

        static public Boolean Authenticate(IntDatabase managementDB, string account, string pwhash, string instanceid)
        {
            Boolean result = false;
            DataTable processors;

            if (SecurityInputSanitizer.SafetyCheck(SecurityInputSanitizer.USERNAME, account))
            {
                String query = "select * from [Accounts] where [AccountName]=@accountname AND [PasswordHash]=@pwhash;";
                Tree parms = new Tree();
                parms.AddElement("@accountname", account);
                parms.AddElement("@pwhash", pwhash);
                processors = managementDB.ExecuteDynamic(query, parms);
                parms.Dispose();

                if (processors.Rows.Count > 0)
                {
                    BaseInstance instanceCheck = BaseInstance.loadInstanceByUniqueID(managementDB, instanceid);
                    if (instanceCheck != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        static public void updateAccount(IntDatabase managementDB, BaseAccount account)
        {
            if (account.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("AccountName", account.AccountName);
                data.AddElement("DisplayName", account.DisplayName);
                data.AddElement("AccountEnabled", account.AccountEnabled);
                data.AddElement("EmailAddress", account.EmailAddress);
                data.AddElement("FirstName", account.FirstName);
                data.AddElement("LastName", account.LastName);
                data.AddElement("Validated", account.Validated);
                data.AddElement("MustChangePassword", account.MustChangePassword);
                data.AddElement("Retired", account.Retired);
                data.AddElement("LockedOut", account.LockedOut);
                data.AddElement("PasswordExpires", account.PasswordExpires);
                data.AddElement("_PasswordExpires", "BIGINT");
                data.AddElement("PasswordHash", account.PasswordHash);
                data.AddElement("CredentialID", account.CredentialID);
                data.AddElement("GroupID", account.GroupID);
                data.AddElement("Role", account.Role);
                data.AddElement("IconURL", account.IconURL);
                data.AddElement("ParameterID", account.ParameterID);
                data.AddElement("PhoneNumber", account.PhoneNumber);
                data.AddElement("LastLogin", account.LastLogin);
                data.AddElement("*@UniqueID", account.UniqueID);
                managementDB.UpdateTree("Accounts", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewAccount = new Tree();
                NewAccount.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewAccount.AddElement("_DateAdded", "BIGINT");
                NewAccount.AddElement("AccountName", account.AccountName);
                NewAccount.AddElement("DisplayName", account.DisplayName);
                NewAccount.AddElement("AccountEnabled", account.AccountEnabled);
                NewAccount.AddElement("EmailAddress", account.EmailAddress);
                NewAccount.AddElement("FirstName", account.FirstName);
                NewAccount.AddElement("LastName", account.LastName);
                NewAccount.AddElement("Validated", account.Validated);
                NewAccount.AddElement("MustChangePassword", account.MustChangePassword);
                NewAccount.AddElement("Retired", account.Retired);
                NewAccount.AddElement("LockedOut", account.LockedOut);
                NewAccount.AddElement("PasswordExpires", account.PasswordExpires);
                NewAccount.AddElement("_PasswordExpires", "BIGINT");
                NewAccount.AddElement("PasswordHash", account.PasswordHash);
                NewAccount.AddElement("CredentialID", account.CredentialID);
                account.UniqueID = "U" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewAccount.AddElement("UniqueID", account.UniqueID);
                NewAccount.AddElement("GroupID", account.GroupID);
                NewAccount.AddElement("Role", account.Role);
                NewAccount.AddElement("ParameterID", account.ParameterID);
                NewAccount.AddElement("LastLogin", account.LastLogin);
                NewAccount.AddElement("IconURL", account.LastLogin);
                NewAccount.AddElement("PhoneNumber", account.PhoneNumber);
                managementDB.InsertTree("Accounts", NewAccount);
                NewAccount.Dispose();
            }

            loadAccount(managementDB, account);
        }

        static public void removeAccountByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Accounts] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Accounts]( " +
                    "[DateAdded] INTEGER NULL, " +
                    "[AccountName] TEXT NULL, " +
                    "[DisplayName] TEXT NULL, " +
                    "[EmailAddress] TEXT NULL, " +
                    "[FirstName] TEXT NULL, " +
                    "[LastName] TEXT NULL, " +
                    "[Validated] TEXT NULL, " +
                    "[MustChangePassword] TEXT NULL, " +
                    "[Retired] TEXT NULL, " +
                    "[LockedOut] TEXT NULL, " +
                    "[PasswordExpires] INTEGER NULL, " +
                    "[PasswordHash] TEXT NULL, " +
                    "[CredentialID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[Role] TEXT NULL, " +
                    "[LastLogin] TEXT NULL, " +
                    "[IconURL] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[PhoneNumber] TEXT NULL, " +
                    "[AccountEnabled] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Accounts](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[AccountName] NVARCHAR(128) NULL, " +
                    "[DisplayName] NVARCHAR(50) NULL, " +
                    "[EmailAddress] NVARCHAR(128) NULL, " +
                    "[FirstName] NVARCHAR(60) NULL, " +
                    "[LastName] NVARCHAR(60) NULL, " +
                    "[Validated] VARCHAR(10) NULL, " +
                    "[MustChangePassword] VARCHAR(10) NULL, " +
                    "[Retired] VARCHAR(10) NULL, " +
                    "[LockedOut] VARCHAR(10) NULL, " +
                    "[PasswordExpires] BIGINT NULL, " +
                    "[PasswordHash] VARCHAR(128) NULL, " +
                    "[CredentialID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[Role] NVARCHAR(15) NULL, " +
                    "[LastLogin] TEXT NULL, " +
                    "[IconURL] NVARCHAR(300) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[PhoneNumber] VARCHAR(40) NULL, " +
                    "[AccountEnabled] VARCHAR(10) NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseaccount ON Accounts([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseaccount ON Accounts([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getXML(BaseAccount current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("AccountName", current.AccountName);
            tmp.AddElement("DisplayName", current.DisplayName);
            tmp.AddElement("AccountEnabled", current.AccountEnabled);
            tmp.AddElement("EmailAddress", current.EmailAddress);
            tmp.AddElement("FirstName", current.FirstName);
            tmp.AddElement("LastName", current.LastName);
            tmp.AddElement("Validated", current.Validated);
            tmp.AddElement("MustChangePassword", current.MustChangePassword);
            tmp.AddElement("Retired", current.Retired);
            tmp.AddElement("LockedOut", current.LockedOut);
            tmp.AddElement("PasswordExpires", current.PasswordExpires);
            tmp.AddElement("PasswordHash", current.PasswordHash);
            tmp.AddElement("CredentialID", current.CredentialID);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("IconURL", current.IconURL);
            tmp.AddElement("ParameterID", current.ParameterID);
            tmp.AddElement("LastLogin", current.LastLogin);
            tmp.AddElement("PhoneNumber", current.PhoneNumber);
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseAccount");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public string getPasswordHash(string username, string password)
        {
            byte[] bytes = new byte[password.Length * sizeof(char)];
            System.Buffer.BlockCopy((username + password).ToCharArray(), 0, bytes, 0, bytes.Length);

            MD5CryptoServiceProvider md5hash = new MD5CryptoServiceProvider();
            SHA1CryptoServiceProvider sha1hash = new SHA1CryptoServiceProvider();

            md5hash.Initialize();
            sha1hash.Initialize();

            md5hash.ComputeHash(bytes, 0, bytes.Length);
            string md5text = FatumLib.ConvertBytesTostring(md5hash.Hash);
            md5text += password;

            bytes = new byte[md5text.Length * sizeof(char)];
            System.Buffer.BlockCopy(md5text.ToCharArray(), 0, bytes, 0, bytes.Length);
            sha1hash.ComputeHash(bytes, 0, bytes.Length);

            string pwHash = FatumLib.ConvertBytesTostring(sha1hash.Hash);

            return pwHash;
        }

        static public Boolean passwordStrengthCheck(string password)
        {
            Boolean result = true;

            // TODO:  Add some elementary password strength checker


            return result;
        }

        public static DataTable getUserList(IntDatabase managementDB)
        {
            string SQL = "select * from [Accounts];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static Boolean checkIfAccountExists(IntDatabase managementDB, string username)
        {
            string SQL = "select * from [Accounts] where AccountName=@username;";
            Tree data = new Tree();
            data.AddElement("@username", username);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean checkIfDisplayNameExists(IntDatabase managementDB, string displayname)
        {
            string SQL = "select * from [Accounts] where DisplayName=@displayname;";
            Tree data = new Tree();
            data.AddElement("@displayname", displayname);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean isAccountAdministrator(IntDatabase managementDB, BaseAccount account)
        {
            string groupID = BaseProperty.getProperty(managementDB, "", "Global");

            string SQL = "select * from [Groups] where [uniqueID]=@uniqueid";
            Tree data = new Tree();
            data.AddElement("@uniqueid", "property");
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public void updateLastLogin(IntDatabase managementDB, BaseAccount account)
        {
                Tree data = new Tree();
 
                data.AddElement("LastLogin", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                data.AddElement("*@UniqueID", account.UniqueID);
                managementDB.UpdateTree("Accounts", data, "UniqueID=@UniqueID");
                data.Dispose();
        }
    }
}
