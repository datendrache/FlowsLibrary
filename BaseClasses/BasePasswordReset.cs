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

using Proliferation.Fatum;
using DatabaseAdapters;
using System.Data;

namespace Proliferation.Flows
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
            NewReset.AddElement("@AccountID", reset.AccountID);
            DateTime TimeoutLimit = DateTime.Now;
            TimeoutLimit.AddHours(1);
            NewReset.AddElement("@Timeout", TimeoutLimit.Ticks.ToString());
            reset.UniqueID = "Z" + System.Guid.NewGuid().ToString().Replace("-", "");
            NewReset.AddElement("@UniqueID", reset.UniqueID);
            managementDB.ExecuteDynamic(sql, NewReset);
            NewReset.Dispose();
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
            data.AddElement("@code", code);
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
                    accountdata.SetElement("@account", accountid);
                    result = dr["AccountID"].ToString();
                    sql = "delete from [PasswordResets] where AccountID=@account;";
                    managementDB.ExecuteDynamic(sql, accountdata);
                    accountdata.Dispose();
                }
                else
                {
                    Tree accountdata = new Tree();
                    accountdata.SetElement("@account", accountid);
                    sql = "delete from [PasswordResets] where AccountID=@account;";
                    managementDB.ExecuteDynamic(sql, accountdata);
                    accountdata.Dispose();
                }
            }

            data.Dispose();
            return result;
        }
    }
}
