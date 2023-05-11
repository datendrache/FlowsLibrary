//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using DatabaseAdapters;
using Fatum.FatumCore;

namespace PhlozLib
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
                ReportDesign.dispose();
                ReportDesign = null;
            }
        }

        static public void deleteReportByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Reports] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
                newReport.ReportDesign = XMLTree.readXMLFromString(designstring);
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
                data.addElement("ReportName", report.ReportName);
                data.addElement("ReportDesign", TreeDataAccess.writeTreeToXMLString(report.ReportDesign, "Design"));
                data.addElement("OwnerID", report.OwnerID);
                data.addElement("GroupID", report.GroupID);
                data.addElement("Description", report.Description);
                data.addElement("*@UniqueID", report.UniqueID);
                managementDB.UpdateTree("[Reports]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree NewReport = new Tree();
                NewReport.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                NewReport.addElement("ReportName", report.ReportName);
                NewReport.addElement("ReportDesign", TreeDataAccess.writeTreeToXMLString(report.ReportDesign, "Design"));
                report.UniqueID= "5" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewReport.addElement("UniqueID", report.UniqueID);
                NewReport.addElement("OwnerID", report.OwnerID);
                NewReport.addElement("GroupID", report.GroupID);
                NewReport.addElement("Description", report.Description);
                NewReport.addElement("Origin", report.Origin);
                managementDB.InsertTree("Reports", NewReport);
                NewReport.dispose();
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

            result.addElement("DateAdded",DateAdded);
            result.addElement("ReportName",ReportName);
            result.addElement("ReportDesign", FatumLib.toSafeString(TreeDataAccess.writeTreeToXMLString(ReportDesign,"Report")));
            result.addElement("UniqueID", UniqueID);
            result.addElement("GroupID", GroupID);
            result.addElement("OwnerID", OwnerID);
            result.addElement("Description", Description);
            result.addElement("Origin", Origin);
            return result;
        }

        public void fromTree(Tree settings, string NewOwner)
        {
            DateAdded = settings.getElement("DateAdded");
            ReportName = settings.getElement("ProcessName");
            string tmp = FatumLib.fromSafeString(settings.getElement("ProcessCode"));
            ReportDesign = XMLTree.readXMLFromString(tmp);
            Origin = settings.getElement("Origin");
            GroupID = settings.getElement("GroupID");
            UniqueID = "5" + System.Guid.NewGuid().ToString().Replace("-", ""); 
            OwnerID = NewOwner;
            Description = settings.getElement("Description");
        }

        static public string getXML(BaseReport current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("DateAdded", current.DateAdded.ToString());
            tmp.addElement("ReportName", current.ReportName);
            tmp.addElement("ProcessCode", TreeDataAccess.writeTreeToXMLString(current.ReportDesign,"Report"));
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("Description", current.Description);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseReport");
            tmp.dispose();
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
            data.addElement("@UniqueID", uid);
            processors = managementDB.ExecuteDynamic(query,data);
            data.dispose();

            if (processors.Rows.Count>0)
            {
                DataRow row = processors.Rows[0];
                BaseReport newReport = new BaseReport();
                newReport.DateAdded = row["DateAdded"].ToString();
                newReport.ReportName = row["ReportName"].ToString();
                newReport.ReportDesign = XMLTree.readXMLFromString(row["ReportDesign"].ToString());
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
