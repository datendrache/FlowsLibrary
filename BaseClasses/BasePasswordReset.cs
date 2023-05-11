//   Phloz
//   Copyright (C) 2003-2019 Eric Knight


using System;
using FatumCore;
using DatabaseAdapters;
using System.Data.Entity;
using System.Data;

namespace PhlozLib
{
    public class BasePasswordReset
    {
        public string AccountID = ""; 
        public string UniqueID = "";
        public DateTime timeout;

        static public void addReset(IntDatabase managementDB, BasePasswordReset reset)
        {
            string sql = "";
            sql = "INSERT INTO [PasswordResets] ([AccountID], [Timeout], [UniqueID]) VALUES (@AccountID, @Timeout, @UniqueID);";
            Tree NewReset = new Tree();
            NewReset.addElement("@AccountID", reset.AccountID);
            DateTime TimeoutLimit = DateTime.Now;
            TimeoutLimit.AddHours(1);
            NewReset.addElement("@Timeout", TimeoutLimit.Ticks.ToString());
            reset.UniqueID = "Z" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewReset.addElement("@UniqueID", reset.UniqueID);
            managementDB.ExecuteDynamic(sql, NewReset);
            NewReset.dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            configDB = "CREATE TABLE [PasswordResets](" +
            "[AccountID] VARCHAR(33) NULL, " +
            "[Timeout] BIGINT NULL, " +
            "[UniqueID] VARCHAR(33) NULL);";
            database.ExecuteNonQuery(configDB);
        }

        static public String checkCode(IntDatabase managementDB, string code)
        {
            string result = null;
            string sql = "select * from [PasswordResets] where UniqueID=@code;";
            Tree data = new Tree();
            data.addElement("@code", code);
            DataTable dt = managementDB.ExecuteDynamic(sql, data);
            

            if (dt.Rows.Count>0)
            {
                DataRow dr = dt.Rows[0];
                long ticks = Convert.ToInt64(dr["Timeout"]) + 36000000000;
                string accountid = dr["AccountID"].ToString();

                long adjusted = DateTime.Now.Ticks - ticks;
                if (adjusted < 0)
                {
                    Tree accountdata = new Tree();
                    accountdata.setElement("@account", accountid);
                    result = dr["AccountID"].ToString();
                    sql = "delete from [PasswordResets] where AccountID=@account;";
                    managementDB.ExecuteDynamic(sql, accountdata);
                    accountdata.dispose();
                }
                else
                {
                    Tree accountdata = new Tree();
                    accountdata.setElement("@account", accountid);
                    sql = "delete from [PasswordResets] where AccountID=@account;";
                    managementDB.ExecuteDynamic(sql, accountdata);
                    accountdata.dispose();
                }
            }

            data.dispose();
            return result;
        }
    }
}
