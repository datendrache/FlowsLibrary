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
using System.Text.RegularExpressions;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseRule
    {
        public string DateAdded = "";
        public string RuleGroupID = "";
        public string RuleName = "";
        public string Regex = "";
        public int Position = -1;
        public string Enabled = "";
        public string DefaultCategory = "";
        public string DefaultLabel = "";
        public string ProcessingType = "";
        public int ProcessingTypeIndex = 0;
        public string ParameterID = "";
        public BaseParameter Parameter = null;
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Origin = "";

        public Regex RuleRegex = null;

        ~BaseRule()
        {
            DateAdded = null;
            RuleGroupID = null;
            RuleName = null;
            Regex = null;
            Enabled = null;
            DefaultCategory = null;
            DefaultLabel = null;
            ProcessingType = null;
            ParameterID = null;
            Parameter = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            RuleRegex = null;
            Origin = null;
        }
        
        static public ArrayList loadRules(CollectionState State)
        {
            return loadRules(State.managementDB);
        }

        static public ArrayList loadRules(IntDatabase managementDB)
        {
            DataTable rules;
            String query = "select * from [Rules] order by [Position] ASC;";
            rules = managementDB.Execute(query);

            ArrayList tmpRules = new ArrayList();

            foreach (DataRow row in rules.Rows)
            {
                BaseRule newRule = new BaseRule();
                newRule.DateAdded = row["DateAdded"].ToString();
                newRule.RuleGroupID = row["RuleGroupID"].ToString();
                newRule.RuleName = row["RuleName"].ToString();
                newRule.Regex = row["Regex"].ToString();
                newRule.DefaultCategory = row["DefaultCategory"].ToString();
                newRule.DefaultLabel = row["DefaultLabel"].ToString();
                newRule.Position = Convert.ToInt32(row["Position"]);
                newRule.ProcessingType = row["ProcessingType"].ToString();
                newRule.UniqueID = row["UniqueID"].ToString();
                newRule.OwnerID = row["OwnerID"].ToString();
                newRule.GroupID = row["GroupID"].ToString();
                newRule.Enabled = row["Enabled"].ToString();
                newRule.ParameterID = row["ParameterID"].ToString();
                newRule.Origin = row["Origin"].ToString();
                if (newRule.ParameterID.Length>10)
                {
                    newRule.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newRule.ParameterID);
                }

                try
                {
                    newRule.RuleRegex = new Regex(newRule.Regex, (RegexOptions.Compiled | RegexOptions.Multiline));
                }
                catch (Exception xyz)
                {
                    int xyzzy = 0; // Regex message will not compile properly. Generate an error!
                }
                tmpRules.Add(newRule);
            }
            return tmpRules;
        }

        static public ArrayList loadRules(CollectionState State, BaseFlow currentFlow)
        {
            DataTable rules;
            String query = "select Rules.* from Rules join Groups on Rules.RuleGroupID = Groups.UniqueID join [Flows] on Flows.RuleGroupID = Groups.UniqueID where Flows.UniqueID=@flowid ORDER BY [Position] ASC;";

            Tree parms = new Tree();
            parms.AddElement("@flowid", currentFlow.UniqueID);
            rules = State.managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            ArrayList tmpRules = new ArrayList();

            foreach (DataRow row in rules.Rows)
            {
                try
                {
                    BaseRule newRule = new BaseRule();
                    newRule.DateAdded = row["DateAdded"].ToString();
                    newRule.RuleGroupID = row["RuleGroupID"].ToString();
                    newRule.RuleName = row["RuleName"].ToString();
                    newRule.Regex = row["Regex"].ToString();
                    newRule.Position = Convert.ToInt32(row["Position"].ToString());
                    newRule.DefaultCategory = row["DefaultCategory"].ToString();
                    newRule.DefaultLabel = row["DefaultLabel"].ToString();
                    newRule.ProcessingType = row["ProcessingType"].ToString();
                    newRule.Enabled = row["Enabled"].ToString();
                    newRule.OwnerID = row["OwnerID"].ToString();
                    newRule.UniqueID = row["UniqueID"].ToString();
                    newRule.GroupID = row["GroupID"].ToString();
                    newRule.ParameterID = row["ParameterID"].ToString();
                    newRule.Origin = row["Origin"].ToString();
                    if (newRule.ParameterID.Length > 10)
                    {
                        newRule.Parameter = BaseParameter.loadParameterByUniqueID(State.managementDB, newRule.ParameterID);
                    }
                    try
                    {
                        newRule.RuleRegex = new Regex(newRule.Regex, (RegexOptions.Compiled | RegexOptions.Multiline));
                    }
                    catch (Exception xyz)
                    {
                        int xyzzy = 0; // Regex message will not compile properly. Generate an error!
                    }
                    tmpRules.Add(newRule);
                }
                catch (Exception xyz)
                {
                    //tmpRules.Sort();
                }
            }

            return tmpRules;
        }

        static public void updateRule(IntDatabase managementDB, BaseRule rule)
        {
            if (rule.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("RuleName", rule.RuleName);
                data.AddElement("Regex", rule.Regex);
                data.AddElement("Position", rule.Position.ToString());
                data.AddElement("Enabled", rule.Enabled);
                data.AddElement("DefaultLabel", rule.DefaultLabel);
                data.AddElement("DefaultCategory", rule.DefaultCategory);
                data.AddElement("ProcessingType", "Regex");
                data.AddElement("RuleGroupID", rule.RuleGroupID);
                data.AddElement("OwnerID", rule.OwnerID);
                data.AddElement("GroupID", rule.GroupID);
                data.AddElement("ParameterID", rule.ParameterID);
                data.AddElement("Origin", rule.Origin);
                data.AddElement("*@UniqueID", rule.UniqueID);
                managementDB.UpdateTree("[Rules]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree data = new Tree();
                data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.AddElement("_DateAdded", "BIGINT");
                data.AddElement("RuleGroupID", rule.RuleGroupID);
                data.AddElement("RuleName", rule.RuleName);
                data.AddElement("Regex", rule.Regex);
                data.AddElement("Position", rule.Position.ToString());
                data.AddElement("DefaultLabel", rule.DefaultLabel);
                data.AddElement("DefaultCategory", rule.DefaultCategory);
                data.AddElement("ProcessingType", rule.ProcessingType);
                rule.UniqueID = "R" + System.Guid.NewGuid().ToString().Replace("-", "");
                data.AddElement("UniqueID", rule.UniqueID);
                data.AddElement("OwnerID", rule.OwnerID);
                data.AddElement("GroupID", rule.GroupID);
                data.AddElement("Enabled", rule.Enabled);
                data.AddElement("ParameterID", rule.ParameterID);
                data.AddElement("Origin", rule.Origin);

                managementDB.InsertTree("[Rules]", data);
                data.Dispose();
            }

            try
            {
                rule.RuleRegex = new Regex(rule.Regex, (RegexOptions.Compiled | RegexOptions.Multiline));
            }
            catch (Exception xyz)
            {
                int xyzzy = 0; // Regex message will not compile properly. Generate an error!
            }
        }

        static public void removeRuleByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Rules] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void addRule(IntDatabase managementDB, Tree description)
        {
            Tree data = new Tree();
            data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            data.AddElement("_DateAdded", "BIGINT");
            data.AddElement("RuleGroupID", description.GetElement("RuleGroupID"));
            data.AddElement("RuleName", description.GetElement("RuleName"));
            data.AddElement("Regex", description.GetElement("Regex"));
            data.AddElement("Position", description.GetElement("Position"));
            data.AddElement("DefaultLabel", description.GetElement("DefaultLabel"));
            data.AddElement("DefaultCategory", description.GetElement("DefaultCategory"));
            data.AddElement("ProcessingType", description.GetElement("ProcessingType"));
            data.AddElement("UniqueID", description.GetElement("UniqueID"));
            data.AddElement("OwnerID", description.GetElement("OwnerID"));
            data.AddElement("GroupID", description.GetElement("GroupID"));
            data.AddElement("Enabled", description.GetElement("Enabled"));
            data.AddElement("ParameterID", description.GetElement("ParameterID"));
            data.AddElement("Origin", description.GetElement("Origin"));
            managementDB.InsertTree("[Rules]", data);
            data.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Rules](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[RuleGroupID] TEXT NULL, " +
                    "[RuleName] TEXT NULL, " +
                    "[Position] INTEGER NULL, " +
                    "[Enabled] TEXT NULL, " +
                    "[DefaultLabel] TEXT NULL, " +
                    "[DefaultCategory] TEXT NULL, " +
                    "[ProcessingType] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Regex] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Rules](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[RuleGroupID] VARCHAR(33) NULL, " +
                    "[RuleName] NVARCHAR(100) NULL, " +
                    "[Position] INTEGER NULL, " +
                    "[Enabled] VARCHAR(10) NULL, " +
                    "[DefaultLabel] NVARCHAR(100) NULL, " +
                    "[DefaultCategory] NVARCHAR(100) NULL, " +
                    "[ProcessingType] VARCHAR(100) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Regex] TEXT NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baserules ON Rules([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baserulesservice ON Rules([RuleGroupID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baserules ON Rules([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baserulesservice ON Rules([RuleGroupID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        public Tree toTree()
        {
            return getTree(this);
        }

        public static Tree getTree(BaseRule current)
        {
            Tree result = new Tree();

            result.AddElement("DateAdded", current.DateAdded);
            result.AddElement("RuleGroupID", current.RuleGroupID);
            result.AddElement("RuleName", current.RuleName);
            result.AddElement("Position", current.Position.ToString());
            result.AddElement("DefaultCategory", current.DefaultCategory);
            result.AddElement("DefaultLabel", current.DefaultLabel);
            result.AddElement("ProcessingType", current.ProcessingType);
            result.AddElement("Regex", FatumLib.ToSafeString(current.Regex));
            result.AddElement("OwnerID", current.OwnerID);
            result.AddElement("UniqueID", current.UniqueID);
            result.AddElement("GroupID", current.GroupID);
            result.AddElement("ParameterID", current.ParameterID);
            result.AddElement("Enabled", current.Enabled.ToString());
            result.AddElement("Origin", current.Origin);
            return result;
        }

        public void fromTree(Tree settings, string newOwnerID)
        {
            DateAdded = settings.GetElement("DateAdded");
            RuleGroupID = settings.GetElement("RuleGroupID");
            RuleName = settings.GetElement("RuleName");
            Position = 0; // Position also cleared, this value will adjusted
            DefaultCategory = settings.GetElement("DefaultCategory");
            DefaultLabel = settings.GetElement("DefaultLabel");
            ProcessingType = settings.GetElement("ProcessingType");
            ProcessingTypeIndex = getProcessingTypeIndex(ProcessingType);
            Regex = FatumLib.FromSafeString(settings.GetElement("Regex"));
            UniqueID = "R" + System.Guid.NewGuid().ToString().Replace("-", "");
            OwnerID = newOwnerID;
            Origin = settings.GetElement("Origin");

            if (settings.GetElement("Enabled").ToLower() == "true")
            {
                Enabled = "true";
            }
            else
            {
                Enabled = "false";
            }
        }


        static private int getProcessingTypeIndex(string PT)
        {
            int result = 0;
            switch (PT)
            {
                case "Regex": result = 1; break;
                case "XML": result = 2; break;
                case "JSON": result = 3; break;
                case "HTML": result = 4; break;
                case "Nothing": result = 0; break;
                default: result = -1; break;
            }
            return result;
        }
        
        static private string getProcessTypeByIndex(int PTIndex)
        {
            string result = "";

            switch (PTIndex)
            {
                case 0: result = "Nothing"; break;
                case 1: result = "Regex"; break;
                case 2: result = "XML"; break;
                case 3: result = "JSON"; break;
                case 4: result = "HTML"; break;
                default: result = "Error"; break;
            }
            return result;
        }

        static public void deleteRule(BaseRule current, IntDatabase database)
        {
            string SQL = "delete from [Rules] where [UniqueID]=@ruleid;";
            Tree parms = new Tree();
            parms.AddElement("@ruleid", current.UniqueID);
            database.ExecuteDynamic(SQL, parms);
            parms.Dispose();
        }


        static public string getXML(BaseRule current)
        {
            string result = "";
            Tree tmp = getTree(current);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseRule");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        public static DataTable getRuleListByRuleGroup(IntDatabase managementDB, string RuleGroupID)
        {
            string SQL = "select * from [Rules] where RuleGroupID=@RuleGroupID order by [Position] asc;";
            Tree data = new Tree();
            data.AddElement("@RuleGroupID", RuleGroupID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            return dt;
        }

        public static DataTable RuleCount(IntDatabase managementDB, string RuleGroupID)
        {
            string SQL = "select count(*) as [totalrules] from [Rules] where RuleGroupID=@RuleGroupID;";
            Tree data = new Tree();
            data.AddElement("@RuleGroupID", RuleGroupID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            return dt;
        }

        static public BaseRule loadRuleByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable rules;
            BaseRule result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Rules] where [UniqueID]=@uid;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select * from [Rules] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            rules = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in rules.Rows)
            {
                BaseRule newRule = new BaseRule();
                newRule.DateAdded = row["DateAdded"].ToString();
                newRule.RuleName = row["RuleName"].ToString();
                newRule.Position = Convert.ToInt32(row["Position"]);
                newRule.Regex = row["Regex"].ToString();
                newRule.Enabled = row["Enabled"].ToString();
                newRule.ParameterID = row["ParameterID"].ToString();
                newRule.UniqueID = row["UniqueID"].ToString();
                newRule.OwnerID = row["OwnerID"].ToString();
                newRule.GroupID = row["GroupID"].ToString();
                newRule.RuleGroupID = row["RuleGroupID"].ToString();
                newRule.DefaultCategory = row["DefaultCategory"].ToString();
                newRule.DefaultLabel = row["DefaultLabel"].ToString();
                newRule.ProcessingType = row["ProcessingType"].ToString();
                newRule.ProcessingTypeIndex = getProcessingTypeIndex(newRule.ProcessingType);
                newRule.Origin = row["Origin"].ToString();
                if (newRule.ParameterID.Length > 10)
                {
                    newRule.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newRule.ParameterID);
                }
                result = newRule;
            }
            return result;
        }

        public static Tree MatchRule(Regex expression, String document)
        {
            Tree result = null;
            Match match;

            match = expression.Match(document);
            if (match.Success)
            {
                result = new Tree();

                GroupCollection groups = match.Groups;
                foreach (string groupName in expression.GetGroupNames())
                {
                    if (groupName != "0")
                    {
                        result.AddElement(groupName,groups[groupName].Value);
                    }
                    else
                    {
                        result.AddElement("Global", groups[groupName].Value);
                    }
                }
            }
            return result;
        }

        public static void importRule(IntDatabase managementDB, Tree rule, string RuleGroupID, string ownerid, string groupid)
        {
            BaseRule importedRule = new BaseRule();
            importedRule.fromTree(rule, ownerid);
            importedRule.GroupID = groupid;
            importedRule.RuleGroupID = RuleGroupID;

            string importOrUpdate = "select [UniqueID] as ImportCheck from [Rules] where [UniqueID]=@uid and [RuleGroupID]=@RuleGroupID;";
            Tree data = new Tree();
            data.AddElement("@uid", importedRule.UniqueID);
            data.AddElement("@RuleGroupID", importedRule.RuleGroupID);

            DataTable dt = managementDB.ExecuteDynamic(importOrUpdate, data);
            data.Dispose();
            if (dt.Rows.Count>0)
            {
                // This is a rule UPDATE  (both UniqueID and RuleGroupID match an existing rule)

                BaseRule.updateRule(managementDB, importedRule);
            }
            else
            {
                // This is a rule IMPORT

                importedRule.UniqueID = "";
                string totalRulesSQL = "select count(*) as RuleCount from [Rules];";
                object tmp = managementDB.ExecuteScalar(totalRulesSQL);
                string convertstring;
                if (tmp!=null)
                {
                    importedRule.Position = Convert.ToInt32(tmp.ToString()) + 1;
                }
                else
                {
                    importedRule.Position = 0;
                }
                
                BaseRule.updateRule(managementDB, importedRule);
            }
        }
    }
}
