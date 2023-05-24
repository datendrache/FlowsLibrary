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
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseReport
    {
        public string DateAdded = "";
        public string ReportName = "";
        public Tree ReportDesign = null;
        public string UniqueID = "";
        public string OwnerID = "";
        public string GroupID = "";
        public string Description = "";
        public string Origin = "";

        ~BaseReport()
        {
            DateAdded = null;
            ReportName = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            Description = null;
            Origin = null;
            if (ReportDesign!=null)
            {
                ReportDesign.Dispose();
                ReportDesign = null;
            }
        }

        static public void deleteReportByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Reports] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public ArrayList loadReports(IntDatabase managementDB)
        {
            DataTable processors;
            String query = "select * from [Reports];";
            processors = managementDB.Execute(query);

            ArrayList newReports = new ArrayList();

            foreach (DataRow row in processors.Rows)
            {
                BaseReport newReport = new BaseReport();
                newReport.DateAdded = row["DateAdded"].ToString();
                newReport.ReportName = row["ReportName"].ToString();
                string designstring = row["ReportDesign"].ToString();
                newReport.ReportDesign = XMLTree.ReadXmlFromString(designstring);
                newReport.UniqueID = row["UniqueID"].ToString();
                newReport.OwnerID = row["OwnerID"].ToString();
                newReport.GroupID = row["GroupID"].ToString();
                newReport.Description = row["description"].ToString();
                newReport.Origin = row["Origin"].ToString();
                newReports.Add(newReport);
            }

            return newReports;
        }


        static public void updateReport(IntDatabase managementDB, BaseReport report)
        {
            if (report.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("ReportName", report.ReportName);
                data.AddElement("ReportDesign", TreeDataAccess.WriteTreeToXmlString(report.ReportDesign, "Design"));
                data.AddElement("OwnerID", report.OwnerID);
                data.AddElement("GroupID", report.GroupID);
                data.AddElement("Description", report.Description);
                data.AddElement("*@UniqueID", report.UniqueID);
                managementDB.UpdateTree("[Reports]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree NewReport = new Tree();
                NewReport.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewReport.AddElement("ReportName", report.ReportName);
                NewReport.AddElement("ReportDesign", TreeDataAccess.WriteTreeToXmlString(report.ReportDesign, "Design"));
                report.UniqueID= "5" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewReport.AddElement("UniqueID", report.UniqueID);
                NewReport.AddElement("OwnerID", report.OwnerID);
                NewReport.AddElement("GroupID", report.GroupID);
                NewReport.AddElement("Description", report.Description);
                NewReport.AddElement("Origin", report.Origin);
                managementDB.InsertTree("Reports", NewReport);
                NewReport.Dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Reports](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[ReportName] TEXT NULL, " +
                    "[ReportDesign] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Description] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Reports](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[ReportName] NVARCHAR(100) NULL, " +
                    "[ReportDesign] VARCHAR(MAX) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33), " +
                    "[Origin] VARCHAR(33), " +
                    "[Description] NVARCHAR(MAX));";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

   
        public Tree toTree()
        {
            Tree result = new Tree();

            result.AddElement("DateAdded",DateAdded);
            result.AddElement("ReportName",ReportName);
            result.AddElement("ReportDesign", FatumLib.ToSafeString(TreeDataAccess.WriteTreeToXmlString(ReportDesign,"Report")));
            result.AddElement("UniqueID", UniqueID);
            result.AddElement("GroupID", GroupID);
            result.AddElement("OwnerID", OwnerID);
            result.AddElement("Description", Description);
            result.AddElement("Origin", Origin);
            return result;
        }

        public void fromTree(Tree settings, string NewOwner)
        {
            DateAdded = settings.GetElement("DateAdded");
            ReportName = settings.GetElement("ProcessName");
            string tmp = FatumLib.FromSafeString(settings.GetElement("ProcessCode"));
            ReportDesign = XMLTree.ReadXmlFromString(tmp);
            Origin = settings.GetElement("Origin");
            GroupID = settings.GetElement("GroupID");
            UniqueID = "5" + System.Guid.NewGuid().ToString().Replace("-", ""); 
            OwnerID = NewOwner;
            Description = settings.GetElement("Description");
        }

        static public string getXML(BaseReport current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.AddElement("DateAdded", current.DateAdded.ToString());
            tmp.AddElement("ReportName", current.ReportName);
            tmp.AddElement("ProcessCode", TreeDataAccess.WriteTreeToXmlString(current.ReportDesign,"Report"));
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("Description", current.Description);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseReport");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public DataTable getReportList(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Reports];";
            services = managementDB.Execute(squery);
            return services;
        }

        static public BaseReport getReportByUniqueID(IntDatabase managementDB, string uid)
        {
            DataTable processors;
            String query = "select * from [Reports] where UniqueID=@UniqueID;";
            Tree data = new Tree();
            data.AddElement("@UniqueID", uid);
            processors = managementDB.ExecuteDynamic(query,data);
            data.Dispose();

            if (processors.Rows.Count>0)
            {
                DataRow row = processors.Rows[0];
                BaseReport newReport = new BaseReport();
                newReport.DateAdded = row["DateAdded"].ToString();
                newReport.ReportName = row["ReportName"].ToString();
                newReport.ReportDesign = XMLTree.ReadXmlFromString(row["ReportDesign"].ToString());
                newReport.UniqueID = row["UniqueID"].ToString();
                newReport.OwnerID = row["OwnerID"].ToString();
                newReport.GroupID = row["GroupID"].ToString();
                newReport.Origin = row["Origin"].ToString();
                newReport.Description = row["Description"].ToString();
                return newReport;
            }
            else
            {
                return null;
            }
        }
    }
}
