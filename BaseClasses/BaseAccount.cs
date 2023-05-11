//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using System.Security.Cryptography;
using DatabaseAdapters;

namespace PhlozLib
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
            parms.addElement("@username", username);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
            parms.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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

            if (FatumLib.validateEmail(newAccount.AccountName))
            {
                String query = "select * from [Accounts] where EmailAddress=@accountname";

                Tree parms = new Tree();
                parms.addElement("@accountname", newAccount.AccountName);
                processors = managementDB.ExecuteDynamic(query, parms);
                parms.dispose();

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

            if (FatumLib.validateNoSymbols(search))
            {
                String query = "";
                if (search.Length > 0)
                {
                    query = "select * from [Accounts] where (EmailAddress glob @accountname OR FirstName glob @accountname OR LastName glob @accountname) Order By EmailAddress ASC limit 250;";
                    Tree parms = new Tree();
                    parms.addElement("@accountname", search);
                    processors = managementDB.ExecuteDynamic(query, parms);
                    parms.dispose();
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
                parms.addElement("@accountname", account);
                parms.addElement("@pwhash", pwhash);
                processors = managementDB.ExecuteDynamic(query, parms);
                parms.dispose();

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
                parms.addElement("@accountname", account);
                parms.addElement("@pwhash", pwhash);
                processors = managementDB.ExecuteDynamic(query, parms);
                parms.dispose();

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
                data.addElement("AccountName", account.AccountName);
                data.addElement("DisplayName", account.DisplayName);
                data.addElement("AccountEnabled", account.AccountEnabled);
                data.addElement("EmailAddress", account.EmailAddress);
                data.addElement("FirstName", account.FirstName);
                data.addElement("LastName", account.LastName);
                data.addElement("Validated", account.Validated);
                data.addElement("MustChangePassword", account.MustChangePassword);
                data.addElement("Retired", account.Retired);
                data.addElement("LockedOut", account.LockedOut);
                data.addElement("PasswordExpires", account.PasswordExpires);
                data.addElement("_PasswordExpires", "BIGINT");
                data.addElement("PasswordHash", account.PasswordHash);
                data.addElement("CredentialID", account.CredentialID);
                data.addElement("GroupID", account.GroupID);
                data.addElement("Role", account.Role);
                data.addElement("IconURL", account.IconURL);
                data.addElement("ParameterID", account.ParameterID);
                data.addElement("PhoneNumber", account.PhoneNumber);
                data.addElement("LastLogin", account.LastLogin);
                data.addElement("*@UniqueID", account.UniqueID);
                managementDB.UpdateTree("Accounts", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewAccount = new Tree();
                NewAccount.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewAccount.addElement("_DateAdded", "BIGINT");
                NewAccount.addElement("AccountName", account.AccountName);
                NewAccount.addElement("DisplayName", account.DisplayName);
                NewAccount.addElement("AccountEnabled", account.AccountEnabled);
                NewAccount.addElement("EmailAddress", account.EmailAddress);
                NewAccount.addElement("FirstName", account.FirstName);
                NewAccount.addElement("LastName", account.LastName);
                NewAccount.addElement("Validated", account.Validated);
                NewAccount.addElement("MustChangePassword", account.MustChangePassword);
                NewAccount.addElement("Retired", account.Retired);
                NewAccount.addElement("LockedOut", account.LockedOut);
                NewAccount.addElement("PasswordExpires", account.PasswordExpires);
                NewAccount.addElement("_PasswordExpires", "BIGINT");
                NewAccount.addElement("PasswordHash", account.PasswordHash);
                NewAccount.addElement("CredentialID", account.CredentialID);
                account.UniqueID = "U" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewAccount.addElement("UniqueID", account.UniqueID);
                NewAccount.addElement("GroupID", account.GroupID);
                NewAccount.addElement("Role", account.Role);
                NewAccount.addElement("ParameterID", account.ParameterID);
                NewAccount.addElement("LastLogin", account.LastLogin);
                NewAccount.addElement("IconURL", account.LastLogin);
                NewAccount.addElement("PhoneNumber", account.PhoneNumber);
                managementDB.InsertTree("Accounts", NewAccount);
                NewAccount.dispose();
            }

            loadAccount(managementDB, account);
        }

        static public void removeAccountByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Accounts] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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

            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("AccountName", current.AccountName);
            tmp.addElement("DisplayName", current.DisplayName);
            tmp.addElement("AccountEnabled", current.AccountEnabled);
            tmp.addElement("EmailAddress", current.EmailAddress);
            tmp.addElement("FirstName", current.FirstName);
            tmp.addElement("LastName", current.LastName);
            tmp.addElement("Validated", current.Validated);
            tmp.addElement("MustChangePassword", current.MustChangePassword);
            tmp.addElement("Retired", current.Retired);
            tmp.addElement("LockedOut", current.LockedOut);
            tmp.addElement("PasswordExpires", current.PasswordExpires);
            tmp.addElement("PasswordHash", current.PasswordHash);
            tmp.addElement("CredentialID", current.CredentialID);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("IconURL", current.IconURL);
            tmp.addElement("ParameterID", current.ParameterID);
            tmp.addElement("LastLogin", current.LastLogin);
            tmp.addElement("PhoneNumber", current.PhoneNumber);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseAccount");
            tmp.dispose();
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
            string md5text = FatumLib.convertBytesTostring(md5hash.Hash);
            md5text += password;

            bytes = new byte[md5text.Length * sizeof(char)];
            System.Buffer.BlockCopy(md5text.ToCharArray(), 0, bytes, 0, bytes.Length);
            sha1hash.ComputeHash(bytes, 0, bytes.Length);

            string pwHash = FatumLib.convertBytesTostring(sha1hash.Hash);

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
            data.addElement("@username", username);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
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
            data.addElement("@displayname", displayname);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
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
            data.addElement("@uniqueid", "property");
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
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
 
                data.addElement("LastLogin", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                data.addElement("*@UniqueID", account.UniqueID);
                managementDB.UpdateTree("Accounts", data, "UniqueID=@UniqueID");
                data.dispose();
        }
    }
}
