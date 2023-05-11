//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Net;
using System.IO;
using FatumCore;
using FatumAnalytics;
using System.Data.SQLite;
using DatabaseAdapters;
using System.Threading;

namespace PhlozLib
{
    public class BaseFlow
    {
        public string FlowName = "";
        public string DateAdded = "";
        public string ServiceID = "";
        public Boolean ProcessingEnabled = false;
        public string Description = "";
        public Boolean RetainDocuments = false;
        public int Interval = 60;
        public Boolean IndexString = false;
        public string DatabaseDirectory = "";
        public string CollectionMethod = "";
        public string OwnerID = "";
        public string UniqueID = "";
        public string GroupID = "";
        public string ParameterID = "";
        public string CredentialID = "";
        public string ControlState = "";
        public string RuleGroupID = "";
        public int documentcount = 0;
        public string Parsing = "";
        public string Origin = "";
        public Boolean Enabled = false;

        public BaseFlowStatus FlowStatus = null;
        public FlowReference flowReference = null;
        public DateTime lastSync = DateTime.Now;
        public BaseService ParentService;

        //  The following are operational variables

        public IPAddress meta_ipaddress = null;
        public ArrayList Incoming = new ArrayList();
        public DateTime intervalTicks = DateTime.MinValue;
        public BaseIndex indexer = null;

        //  The following pertains to writing to the database

        public IntDatabase documentDB = null;
        public Boolean dbLock = false;
        public ArrayList buffer = new ArrayList();
        public String bulkLock = "";
        public LinkedList<BaseDocument> bulkInsert = new LinkedList<BaseDocument>();
        public Boolean locked = false;
        public Boolean Suspended = false;

        // Credential

        public BaseParameter Parameter = null;
        static long DocumentID = 0;
        public object ServiceObject = null;   // ServiceObject is a class that may be relevant to deregistering a flow

        public BaseFlow()
        {

        }

        ~BaseFlow()
        {
            ServiceObject = null;
            FlowName = null;
            DateAdded = null;
            ServiceID = null;
            Description = null;
            DatabaseDirectory = null;
            CollectionMethod = null;
            ControlState = null;
            OwnerID = null;
            UniqueID = null;
            GroupID = null;
            ParameterID = null;
            CredentialID = null;
            Parsing = null;
            Origin = null;
            RuleGroupID = null;
            if (Parameter!=null)
            {
                Parameter = null;
            }

            FlowStatus = null;
            ParentService = null;

            meta_ipaddress = null;
            if (Incoming != null)
            {
                Incoming.Clear();
                Incoming = null;
            }

            if (indexer !=null)
            {
                indexer.Close();
            }
            indexer = null;

            if (documentDB!=null)
            {
                if (documentDB.GetTransactionLockStatus())
                {
                    try
                    {
                        for (int i=0;i<10;i++)  // Let's be paranoid -- if the transaction doesn't clear in 10 seconds, we'll then close it ourselves.
                        {
                            System.Threading.Thread.Sleep(1000);
                            if (!documentDB.GetTransactionLockStatus())
                            {
                                i = 10;
                            }
                        }
                        documentDB.Commit();
                    }
                    catch (Exception)
                    {

                    }
                }
                documentDB.Close();
            }

            documentDB = null;
            if (buffer != null)
            {
                buffer.Clear();
                buffer = null;
            }

            bulkLock = "";
            if (bulkInsert != null)
            {
                bulkInsert.Clear();
                bulkInsert = null;
            }
        }

        public void Suspend()
        {
            Suspended = true;
        }

        public void Resume()
        {
            Suspended = false;
        }

        public void initializeDatabase(string directory, DateTime timeframe)
        {
            DatabaseDirectory = directory;
 
            string dbfile = directoryPicker(directory, timeframe) + "\\" + fileNamePicker() + ".s3db";

            if (!File.Exists(dbfile))
            {
                documentDB = new SQLiteDatabase(dbfile);
                BaseDocument.defaultSQL(documentDB, DatabaseSoftware.SQLite);
            }
            else
            {
                documentDB = new SQLiteDatabase(dbfile);
                documentcount = getCurrentDocumentCount(documentDB);
            }
        }

        public string fileNamePicker()
        {
            return (UniqueID.ToString());
        }

        public string directoryPicker(string baseDirectory, DateTime when)
        {
            return ( baseDirectory + "\\" + when.Year.ToString() + "-" +
                                          when.Month.ToString() + "-" +
                                          when.Day.ToString() + "\\" );
        }

        public void initializeIndex(string directory, DateTime when)
        {
            string indexDirectory = directoryPicker(directory, when) + "\\" + UniqueID;

            if (!Directory.Exists(indexDirectory))
            {
                Directory.CreateDirectory(indexDirectory);   
            }

            if (indexer!=null)
            {
                indexer.Close();
                indexer = null;
            }

            indexer = new BaseIndex(indexDirectory);
        }


        public void close(CollectionState State)
        {
            Suspended = true;
            Thread.Sleep(0);

            if (documentDB != null)
            {
                if (documentDB.GetTransactionLockStatus())
                {
                    try
                    {
                        documentDB.Commit();
                    }
                    catch (Exception)
                    {

                    }
                }
                try
                {
                    documentDB.Close();
                }
                catch (Exception)
                {

                }
            }

            if (bulkInsert != null)
            {
                if (bulkInsert.Count>0)
                {
                    BulkInsert(State, this);
                }
                bulkInsert.Clear();
                bulkInsert = null;
            }

            if (Incoming != null)
            {
                Incoming.Clear();
                Incoming = null;
            }

            if (buffer!=null)
            {
                buffer.Clear();
                buffer = null;
            }


            documentDB = null;

            if (indexer != null)
            {
                try
                {
                    indexer.Close();
                }
                catch (Exception)
                {

                }
            indexer = null;
            }

            if (Parameter != null)
            {
                Parameter = null;
            }
        }
    
        public static Boolean BulkInsert(CollectionState State, BaseFlow currentFlow)
        {
            Boolean didNothing = false;

            if (currentFlow.bulkInsert.Count > 0)
            {
                LinkedList<BaseDocument> tmplist = null;

                lock (currentFlow.bulkLock)
                {
                    lock (currentFlow.bulkInsert)
                    {
                        if (currentFlow.bulkInsert.Count > 0)
                        {
                            didNothing = false;
                            tmplist = currentFlow.bulkInsert;
                            currentFlow.bulkInsert = new LinkedList<BaseDocument>();
                        }

                        if (tmplist != null)
                        {
                            storeDocuments(State, currentFlow, tmplist);
                            currentFlow.lastSync = DateTime.Now;
                        }
                    }
                }
            }
            return didNothing;
        }

        private static void storeDocuments(CollectionState State, BaseFlow currentFlow, LinkedList<BaseDocument> tmplist)
        {
            lock (tmplist)
            {
                try
                {
                    Boolean successfulStart = currentFlow.documentDB.BeginTransaction();
                    Boolean indexFlow = false;

                    if (currentFlow.IndexString)
                    {
                        if (currentFlow.indexer != null)
                        {
                            indexFlow = true;
                        }
                    }

                    if (successfulStart)
                    {
                        Boolean successful = true;

                        foreach (var current in tmplist)
                        {
                            try
                            {
                                BaseDocument.insertPreparedDocument(current);
                                if (indexFlow) BaseDocument.indexDocument(current);
                                current.dispose();
                            }
                            catch (Exception xyz)
                            {
                                errorMessage("Master Stream Processor", "Warning", "Document excluded from transaction.  Error details: " + xyz.Message + " Stacktrace: " + xyz.StackTrace);
                            }
                        }

                        try
                        {
                            successful = currentFlow.documentDB.Commit();
                            if (!successful)
                            {
                                errorMessage("Master Stream Processor", "Warning", "DB Commit failed to process.");
                            }
                        }
                        catch (SQLiteException xz)
                        {
                            errorMessage("Master Stream Processor", "Warning", "DB Commit failed to process.  Error details: " + xz.Message + " Stacktrace: " + xz.StackTrace);
                        }

                        if (indexFlow)
                        {
                            try
                            {
                                currentFlow.indexer.indexer.Commit();
                            }
                            catch (Exception xyz)
                            {
                                errorMessage("Master Stream Processor", "Warning", "Indexing transaction failed to process.  Error details: " + xyz.Message + " Stacktrace: " + xyz.StackTrace);
                            }
                        }

                        if (successful == true)
                        {
                            tmplist.Clear();
                        }
                        else
                        {
                            // Serious issue going on...
                            currentFlow.documentDB.Rollback();
                            spoolToDisk(currentFlow, tmplist);
                        }
                    }
                    else
                    {
                        // Backlog...
                        spoolToDisk(currentFlow, tmplist);
                    }
                }
                catch (Exception xyz)
                {
                    errorMessage("Master Stream Processor", "Warning", "Outer Transaction failed to process.  Error details: " + xyz.Message + " Stacktrace: " + xyz.StackTrace);
                }
            }
        }

        public static void errorMessage(string errLabel, string errCategory, string errMessage)
        {

        }

        private static void spoolToDisk(BaseFlow flow, LinkedList<BaseDocument> documents)
        {
            lock (documents)
            {
                string directory = flow.documentDB.getDatabaseDirectory();
                if (directory != null)
                {
                    using (StreamWriter w = File.AppendText(directory + "\\" + flow.UniqueID + " OF.xml"))
                    {
                        foreach (BaseDocument current in documents)
                        {
                            if (current.Document != null)
                            {
                                string XML = BaseDocument.getXML(current);
                                w.Write(XML + "\r\n");
                            }
                        }
                    }
                    documents.Clear();
                }
            }
        }

        static public ArrayList loadFlows(CollectionState state, ArrayList serviceList)
        {
            DataTable flows;
            String query = "select * from [Flows];";
            flows = state.managementDB.Execute(query);
            
            ArrayList tmpFlows = new ArrayList();

            foreach (DataRow row in flows.Rows)
            {
                BaseFlow newFlows = new BaseFlow();
                
                newFlows.DateAdded = row["DateAdded"].ToString();
                newFlows.FlowName = row["FlowName"].ToString();
                newFlows.ServiceID = row["ServiceID"].ToString();
                newFlows.Parsing = row["Parsing"].ToString();
                newFlows.RuleGroupID = row["RuleGroupID"].ToString();

                string tmpbool = row["Enabled"].ToString().ToLower();
                if (tmpbool == "true")
                {
                    newFlows.Enabled = true;
                }
                else
                {
                    newFlows.Enabled = false;
                }

                string processing = row["ProcessingEnabled"].ToString();
                if (processing.ToLower()=="true")
                {
                    newFlows.ProcessingEnabled = true;
                }
                else
                {
                    newFlows.ProcessingEnabled = false;
                }
                newFlows.Description = row["Description"].ToString();
                string Retain = row["RetainDocuments"].ToString().ToLower();
                if (Retain=="true")
                {
                    newFlows.RetainDocuments = true;
                }
                else
                {
                    newFlows.RetainDocuments = false;
                }

                string tmpinterval = row["Interval"].ToString();
                if (tmpinterval!="")
                {
                    newFlows.Interval = int.Parse(tmpinterval);
                }
                else
                {
                    newFlows.Interval = 60;
                }
                string index = row["IndexString"].ToString();
                if (index.ToLower()=="true")
                {
                    newFlows.IndexString = true;
                }
                else
                {
                    newFlows.IndexString = false;
                }
                newFlows.CollectionMethod = row["CollectionMethod"].ToString();
                newFlows.CredentialID = row["CredentialID"].ToString();
                newFlows.ParameterID = row["ParameterID"].ToString();
                newFlows.OwnerID = row["OwnerID"].ToString();
                newFlows.GroupID = row["GroupID"].ToString();
                newFlows.UniqueID = row["UniqueID"].ToString();
                newFlows.ControlState = row["ControlState"].ToString();
                newFlows.Origin = row["Origin"].ToString();

                if (newFlows.Parameter != null)
                {
                    if (newFlows.Parameter.ExtractedMetadata != null)
                    {
                        string ipaddress = newFlows.Parameter.ExtractedMetadata.getElement("Server");
                        if (ipaddress != null)
                        {
                            if (ipaddress != "")
                            {
                                try
                                {
                                    newFlows.meta_ipaddress = IPAddress.Parse(ipaddress);
                                }
                                catch (Exception xyz)
                                {
                                    newFlows.meta_ipaddress = null;
                                }
                            }
                        }
                    }
                }


                newFlows.Parameter = BaseParameter.loadParameterByUniqueID(state.managementDB, newFlows.ParameterID);

                if (newFlows.Interval == 0)
                {
                    newFlows.Interval = 120;  // Default is 2 minute polls
                }

                newFlows.intervalTicks = DateTime.MinValue;  // While we are at it, let's prime the pump for an immediate poll.
                BaseFlowStatus.loadBaseFlowStatus(state, newFlows);

                foreach (BaseService current in serviceList)
                {
                    if (current.UniqueID == newFlows.ServiceID)
                    {
                        newFlows.ParentService = current;
                    }
                }

                if (newFlows.ParentService == null)
                {
                    System.Console.Out.WriteLine("ERROR:  No parent Service found! Flow " + newFlows.UniqueID + ", Name [" + newFlows.FlowName + "]");
                    newFlows.ProcessingEnabled = false;
                    System.Console.Out.WriteLine("ERROR:  Automatically Disabled Flow " + newFlows.UniqueID + ", Name [" + newFlows.FlowName + "]");
                    BaseFlow.updateFlow(state.managementDB,newFlows);
                }

                tmpFlows.Add(newFlows);
            }
            return tmpFlows;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Flows](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[ServiceID] TEXT NULL, " +
                    "[FlowName] TEXT NULL, " +
                    "[ProcessingEnabled] TEXT NULL, " +
                    "[Interval] TEXT NULL, " +
                    "[IndexString] TEXT NULL, " +
                    "[RetainDocuments] TEXT NULL, " +
                    "[CollectionMethod] TEXT NULL, "+
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[CredentialID] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[ControlState] TEXT NULL, " +
                    "[RuleGroupID] TEXT NULL, "+
                    "[Parsing] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Enabled] TEXT NULL, " +
                    "[Description] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Flows](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[UniqueID] VARCHAR(50) NULL, " +
                    "[ServiceID] VARCHAR(33) NULL, " +
                    "[FlowName] NVARCHAR(100) NULL, " +
                    "[ProcessingEnabled] VARCHAR(10) NULL, " +
                    "[Interval] VARCHAR(22) NULL, " +
                    "[IndexString] VARCHAR(100) NULL, " +
                    "[RetainDocuments] VARCHAR(10) NULL, " +
                    "[CollectionMethod] VARCHAR(100) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[CredentialID] VARCHAR(33) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[ControlState] VARCHAR(20) NULL, " +
                    "[RuleGroupID] VARCHAR(20) NULL, " +
                    "[Parsing] VARCHAR(100) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Enabled] VARCHAR(10) NULL, " +
                    "[Description] NVARCHAR(2048) NULL );";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseflows ON flows([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseflows ON flows([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public BaseFlow locateFlowByName(string flowname, CollectionState state)
        {
            foreach (BaseSource currentSource in state.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.FlowName==flowname)
                        {
                            return currentFlow;
                        }
                    }
                }
            }
            return null;
        }

        static public int countActiveFlows(CollectionState state)
        {
            int flowtotal = 0;

            foreach (BaseSource currentSource in state.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        flowtotal++;
                    }
                }
            }
            return flowtotal;
        }

        static public BaseFlow locateCachedFlowByUniqueID(string flowid, CollectionState state)
        {
            foreach (BaseSource currentSource in state.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.UniqueID == flowid)
                        {
                            return currentFlow;
                        }
                    }
                }
            }
            return null;
        }

        static public void updateFlowPosition(CollectionState State, BaseFlow flow, long position)
        {
            if (flow.FlowStatus!=null)
            {
                flow.FlowStatus.FlowPosition = position;
                flow.FlowStatus.updateFlowPosition(State);
            }
        }

         static public void updateFlow(IntDatabase managementDB, BaseFlow flow)
        {
            if (flow.UniqueID == "")
            {
                try
                {
                    flow.DateAdded = DateTime.Now.Ticks.ToString();
                    Tree data = new Tree();
                    data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                    data.addElement("_DateAdded", "BIGINT");
                    data.addElement("FlowName", flow.FlowName);
                    data.addElement("ServiceID", flow.ServiceID);
                    data.addElement("ProcessingEnabled", flow.ProcessingEnabled.ToString());
                    flow.UniqueID = "F" + System.Guid.NewGuid().ToString().Replace("-", "");
                    data.addElement("UniqueID", flow.UniqueID);
                    data.addElement("Interval", flow.Interval.ToString());
                    data.addElement("IndexString", flow.IndexString.ToString());
                    data.addElement("RetainDocuments", flow.RetainDocuments.ToString());
                    data.addElement("CollectionMethod", flow.CollectionMethod);
                    data.addElement("OwnerID", flow.OwnerID);
                    data.addElement("GroupID", flow.GroupID);
                    data.addElement("CredentialID", flow.CredentialID);
                    data.addElement("ParameterID", flow.ParameterID);
                    data.addElement("ControlState", flow.ControlState);
                    data.addElement("RuleGroupID", flow.RuleGroupID);
                    data.addElement("Parsing", flow.Parsing);
                    data.addElement("Origin", flow.Origin);
                    data.addElement("Description", flow.Description);
                    data.addElement("Enabled", flow.Enabled.ToString());
                    managementDB.InsertTree("[Flows]", data);
                    data.dispose();
                }
                catch (Exception xyz)
                {
                    System.Console.Out.WriteLine("BaseFlow- Error creating new flow." + xyz.Message + ", " + xyz.StackTrace);
                }
            }
            else
            {
                Tree data = new Tree();
                data.addElement("ProcessingEnabled", flow.ProcessingEnabled.ToString());
                data.addElement("Description", flow.Description);
                data.addElement("FlowName", flow.FlowName);
                data.addElement("Interval", flow.Interval.ToString());
                data.addElement("IndexString", flow.IndexString.ToString());
                data.addElement("CollectionMethod", flow.CollectionMethod);
                data.addElement("RetainDocuments", flow.RetainDocuments.ToString());
                data.addElement("OwnerID", flow.OwnerID);
                data.addElement("GroupID", flow.GroupID);
                data.addElement("CredentialID", flow.CredentialID);
                data.addElement("ControlState", flow.ControlState);
                data.addElement("RuleGroupID", flow.RuleGroupID);
                data.addElement("ParameterID", flow.ParameterID);
                data.addElement("Parsing", flow.Parsing);
                data.addElement("Origin", flow.Origin);
                data.addElement("Enabled", flow.Enabled.ToString());
                data.addElement("*@UniqueID", flow.UniqueID);
                managementDB.UpdateTree("[Flows]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
        }

        static public void removeFlowsByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Flows] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void addFlow(IntDatabase managementDB, Tree description)
        {
            Tree data = new Tree();
            data.addElement("ProcessingEnabled", description.getElement("ProcessingEnabled"));
            data.addElement("Description", description.getElement("Description"));
            data.addElement("FlowName", description.getElement("FlowName"));
            data.addElement("Interval", description.getElement("Interval"));
            data.addElement("IndexString", description.getElement("IndexString"));
            data.addElement("CollectionMethod", description.getElement("TimeString"));
            data.addElement("RetainDocuments", description.getElement("RetainDocuments"));
            data.addElement("OwnerID", description.getElement("OwnerID"));
            data.addElement("GroupID", description.getElement("GroupID"));
            data.addElement("CredentialID", description.getElement("CredentialID"));
            data.addElement("ControlState", description.getElement("ControlState"));
            data.addElement("RuleGroupID", description.getElement("RuleGroupID"));
            data.addElement("ParameterID", description.getElement("ParameterID"));
            data.addElement("Parsing", description.getElement("Parsing"));
            data.addElement("Origin", description.getElement("Origin"));
            data.addElement("UniqueID", description.getElement("UniqueID"));
            data.addElement("Enabled", description.getElement("Enabled"));
            managementDB.InsertTree("[Flows]", data);
            data.dispose();
        }

        static public string locateID(CollectionState State, BaseFlow flow)
        {
            string result = "";
            DataTable flows;
            String query = "select [ID] from [Flows] WHERE [ServiceID]=@serviceid;";

            Tree parms = new Tree();
            parms.addElement("@serviceid", flow.ServiceID);

            flows = State.managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            ArrayList tmpFlows = new ArrayList();

            foreach (DataRow row in flows.Rows)
            {
                BaseFlow newDevice = new BaseFlow();
                result = row["ID"].ToString();    
            }

            return result;
        }

        static public string getXML(BaseFlow current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseFlow");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseFlow current)
        {
            Tree tmp = new Tree();
            tmp.addElement("FlowName", current.FlowName);
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("ServiceID", current.ServiceID);
            tmp.addElement("ProcessingEnabled", current.ProcessingEnabled.ToString());
            tmp.addElement("Description", current.Description);
            tmp.addElement("RetainDocuments", current.RetainDocuments.ToString());
            tmp.addElement("Interval", current.Interval.ToString());
            tmp.addElement("IndexString", current.IndexString.ToString());
            tmp.addElement("CollectionMethod", current.CollectionMethod);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("ParameterID", current.ParameterID);
            tmp.addElement("CredentialID", current.CredentialID);
            tmp.addElement("ControlState", current.ControlState);
            tmp.addElement("RuleGroupID", current.RuleGroupID);
            tmp.addElement("Parsing", current.Parsing);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("Enabled", current.Enabled.ToString());
            return tmp;
        }

        public BaseFlow(Tree XML)
        {
            FlowName = XML.getElement("FlowName");
            DateAdded = XML.getElement("DateAdded");
            ServiceID = XML.getElement("ServiceID");
            string processing = XML.getElement("ProcessingEnabled");
            if (processing.ToLower()=="true")
            {
                ProcessingEnabled = true;
            }
            else
            {
                ProcessingEnabled = false;
            }

            Description = XML.getElement("Description");
            string retain = XML.getElement("RetainDocuments");
            if (retain.ToLower() == "true")
            {
                RetainDocuments = true;
            }
            else
            {
                RetainDocuments = false;
            }

            string tmpinterval = XML.getElement("Internal");
            if (tmpinterval=="")
            {
                Interval = 60;
            }
            else
            {
                Interval = int.Parse(tmpinterval);
            }
            
            string index = XML.getElement("IndexString");
            if (index.ToLower() == "true")
            {
                IndexString = true;
            }
            else
            {
                IndexString = false;
            }
            CollectionMethod = XML.getElement("CollectionMethod");
            CredentialID = XML.getElement("CredentialID");
            ParameterID = XML.getElement("ParameterID");
            ControlState = XML.getElement("ControlState");
            Parsing = XML.getElement("Parsing");
            Origin = XML.getElement("Origin");
            RuleGroupID = XML.getElement("RuleGroupID");

            if (processingEnabledCheck(this))
            {
                Suspended = false;
            }
            else
            {
                Suspended = true;
            }

            if (Parameter != null)
            {
                if (Parameter.ExtractedMetadata != null)
                {
                    string ipaddress = Parameter.ExtractedMetadata.getElement("Server");
                    if (ipaddress != null)
                    {
                        if (ipaddress != "")
                        {
                            try
                            {
                                meta_ipaddress = IPAddress.Parse(ipaddress);
                            }
                            catch (Exception xyz)
                            {
                                meta_ipaddress = null;
                            }
                        }
                    }
                }
            }

            string tmpbool = XML.getElement("Enabled").ToLower();
            if (tmpbool == "true")
            {
                Enabled = true;
            }
            else
            {
                Enabled = false;
            }
        }

        public void loadDocumentID()
        {
            if (documentDB != null)
            {
                object tmp = documentDB.ExecuteScalar("select max([ID]) FROM [Documents];");
                string maxdocuments;
                if (tmp!=null)
                {
                    maxdocuments = tmp.ToString();
                }
                    else
                {
                    maxdocuments = "0";
                }
                long value = 0;
                long.TryParse(maxdocuments, out value);
                DocumentID = value;
            }
            else
            {
                DocumentID = 1;  // We are going to assume that this is because this flow has just now been created.
            }
        }

        public static Boolean processingEnabledCheck(BaseFlow flow)
        {
            Boolean result = false;

            if (flow.ProcessingEnabled)
            {
                switch (flow.ControlState.ToLower())
                {
                    case "removing":
                    case "archiving":
                    case "transferring":
                    case "deleted":
                    case "relinquished":
                        result = false;
                        break;
                    case "ready":
                    case "requested":
                    case "updated":
                        result = true;
                        break;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        public static DataTable getFlowList(IntDatabase managementDB)
        {
            string SQL = "select * from [Flows];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getFlowListMimeOnly(IntDatabase managementDB)
        {
            string SQL = "select [UniqueID], [Parsing] from [Flows];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getFlowListUpdatesByInstance(IntDatabase managementDB, string InstanceID)
        {
            string SQL = "select f.[UniqueID], f.[ProcessingEnabled], f.[Interval], f.[IndexString], f.[CredentialID], f.[ParameterID], f.[Parsing] from [Flows] as f join [Services] as svc on f.ServiceID=svc.UniqueID join [Sources] as so on svc.SourceID=so.UniqueID where so.InstanceID=@instanceid and f.ControlState = 'updated';";
            Tree data = new Tree();
            data.addElement("@instanceid", InstanceID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
            return dt;
        }

        public static DataTable getFlowListRemovingByInstance(IntDatabase managementDB, string InstanceID)
        {
            string SQL = "select f.[UniqueID], f.[ControlState] from [Flows] as f join [Services] as svc on f.ServiceID=svc.UniqueID join [Sources] as so on svc.SourceID=so.UniqueID where so.InstanceID=@instanceid and (f.ControlState = 'removing' or f.ControlState = 'transferring');";
            Tree data = new Tree();
            data.addElement("@instanceid", InstanceID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
            return dt;
        }

        public static DataTable getFlowListCompact(IntDatabase managementDB)
        {
            string SQL = "select [FlowName], [UniqueID] from [Flows];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static DataTable getFlowListWithOrigins(IntDatabase managementDB)
        {
            string SQL = "select f.*, srv.SourceID, sor.InstanceID from [Flows] as f join Services as srv on f.ServiceID=srv.UniqueID join Sources as sor on srv.SourceID=sor.UniqueID;";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        static public BaseFlow loadFlowByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable flows;
            BaseFlow result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Flows] where [UniqueID]=@uid limit 1;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select TOP (1) * from [Flows] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            flows = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            if (flows.Rows.Count > 0)
            {
                BaseFlow newFlow = new BaseFlow();
                DataRow row = flows.Rows[0];

                newFlow.DateAdded = row["DateAdded"].ToString();
                newFlow.FlowName = row["FlowName"].ToString();
                newFlow.ServiceID = row["ServiceID"].ToString();
                newFlow.RuleGroupID = row["RuleGroupID"].ToString();

                string processing = row["ProcessingEnabled"].ToString();
                if (processing.ToLower()=="true")
                {
                    newFlow.ProcessingEnabled = true;
                }
                else
                {
                    newFlow.ProcessingEnabled = false;
                }
                newFlow.Parsing = row["Parsing"].ToString();
                newFlow.Origin = row["Origin"].ToString();
                newFlow.Description = row["Description"].ToString();
                string retain = row["RetainDocuments"].ToString();

                string tmpbool = row["Enabled"].ToString().ToLower();
                if (tmpbool == "true")
                {
                    newFlow.Enabled = true;
                }
                else
                {
                    newFlow.Enabled = false;
                }

                if (retain.ToLower() == "true")
                {
                    newFlow.RetainDocuments = true;
                }
                else
                {
                    newFlow.RetainDocuments = false;
                }

                string tmpinterval = row["Interval"].ToString();
                if (tmpinterval=="")
                {
                    newFlow.Interval = 60;
                }
                else
                {
                    newFlow.Interval = int.Parse(tmpinterval);
                }
                string index = row["IndexString"].ToString();
                if (index.ToLower() == "true")
                {
                    newFlow.IndexString = true;
                }
                else
                {
                    newFlow.IndexString = false;
                }
                newFlow.CollectionMethod = row["CollectionMethod"].ToString();
                newFlow.CredentialID = row["CredentialID"].ToString();
                newFlow.ParameterID = row["ParameterID"].ToString();
                newFlow.OwnerID = row["OwnerID"].ToString();
                newFlow.GroupID = row["GroupID"].ToString();
                newFlow.UniqueID = row["UniqueID"].ToString();
                newFlow.ControlState = row["ControlState"].ToString();

                newFlow.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newFlow.ParameterID);

                if (newFlow.Parameter != null)
                {
                    if (newFlow.Parameter.ExtractedMetadata != null)
                    {
                        string ipaddress = newFlow.Parameter.ExtractedMetadata.getElement("Server");
                        if (ipaddress != null)
                        {
                            if (ipaddress != "")
                            {
                                try
                                {
                                    newFlow.meta_ipaddress = IPAddress.Parse(ipaddress);
                                }
                                catch (Exception xyz)
                                {
                                    newFlow.meta_ipaddress = null;
                                }
                            }
                        }
                    }
                }

                if (newFlow.Interval == 0)
                {
                    newFlow.Interval = 60;  // Default is 2 minute polls
                }
                newFlow.intervalTicks = DateTime.MinValue;  // While we are at it, let's prime the pump for an immediate poll.
                newFlow.ParentService = BaseService.loadServiceByUniqueID(managementDB, newFlow.ServiceID);

                if (newFlow.ParentService == null)
                {
                    System.Console.Out.WriteLine("ERROR:  No parent Service found! Flow " + newFlow.UniqueID + ", Name [" + newFlow.FlowName + "]");
                    newFlow.ProcessingEnabled = false;
                    System.Console.Out.WriteLine("ERROR:  Automatically Disabled Flow " + newFlow.UniqueID + ", Name [" + newFlow.FlowName + "]");
                    BaseFlow.updateFlow(managementDB, newFlow);
                }

                result = newFlow;
            }
            return result;
        }

        static public void deleteFlow(IntDatabase managementDB, string FlowID)
        {
            string SQL = "delete from [Flows] where UniqueID=@FlowID;";
            Tree data = new Tree();
            data.addElement("@FlowID", FlowID);
            managementDB.ExecuteDynamic(SQL, data);
            data.dispose();
        }

        static private int getCurrentDocumentCount(IntDatabase currentDB)
        {
            string SQL = "select count(*) from [Documents];";
            object tmp = currentDB.ExecuteScalar(SQL);
            string documentCount;

            if (tmp!=null)
            {
                documentCount = tmp.ToString();
            }
            else
            {
                documentCount = "0";
            }
            
            try
            {
                int value = int.Parse(documentCount);
                return (value);
            }
            catch (Exception xyz)
            {
                int xxx = 0;
            }
            return 0;
        }

        public void updateFlowWithChanges()
        {
            
        }

        public void pauseFlow()
        {

        }

        public void resumeFlow()
        {

        }

        static public ArrayList loadFlowsByServiceEnabledOnly(CollectionState state, BaseService Service)
        {
            DataTable flows;
            String query = "select * from [Flows] where [ServiceID]='" + Service.UniqueID + "' and [Enabled]='true' COLLATE NOCASE;";

            switch (state.managementDB.getDatabaseSoftware())
            {
                case 1: // MSSQL
                    query = "select * from [Flows] where [ServiceID]='" + Service.UniqueID + "' and [Enabled]='true';";
                    break;
                case 2: // SQLITE
                    query = "select * from [Flows] where [ServiceID]='" + Service.UniqueID + "' and [Enabled]='true' COLLATE NOCASE;";
                    break;
            }

            flows = state.managementDB.Execute(query);

            ArrayList tmpFlows = new ArrayList();

            foreach (DataRow row in flows.Rows)
            {
                BaseFlow newFlow = new BaseFlow();
                newFlow.DateAdded = row["DateAdded"].ToString();
                newFlow.FlowName = row["FlowName"].ToString();
                newFlow.ServiceID = row["ServiceID"].ToString();
                newFlow.Parsing = row["Parsing"].ToString();
                newFlow.RuleGroupID = row["RuleGroupID"].ToString();
                string processing = row["ProcessingEnabled"].ToString();
                if (processing.ToLower() == "true")
                {
                    newFlow.ProcessingEnabled = true;
                }
                else
                {
                    newFlow.ProcessingEnabled = false;
                }

                newFlow.Description = row["Description"].ToString();
                string Retain = row["RetainDocuments"].ToString().ToLower();
                if (Retain == "true")
                {
                    newFlow.RetainDocuments = true;
                }
                else
                {
                    newFlow.RetainDocuments = false;
                }

                string tmpbool = row["Enabled"].ToString().ToLower();
                if (tmpbool == "true")
                {
                    newFlow.Enabled = true;
                }
                else
                {
                    newFlow.Enabled = false;
                }

                string tmpinterval = row["Interval"].ToString();
                if (tmpinterval != "")
                {
                    newFlow.Interval = int.Parse(tmpinterval);
                }
                else
                {
                    newFlow.Interval = 60;
                }
                string index = row["IndexString"].ToString();
                if (index.ToLower() == "true")
                {
                    newFlow.IndexString = true;
                }
                else
                {
                    newFlow.IndexString = false;
                }
                newFlow.CollectionMethod = row["CollectionMethod"].ToString();
                newFlow.CredentialID = row["CredentialID"].ToString();
                newFlow.ParameterID = row["ParameterID"].ToString();
                newFlow.OwnerID = row["OwnerID"].ToString();
                newFlow.GroupID = row["GroupID"].ToString();
                newFlow.UniqueID = row["UniqueID"].ToString();
                newFlow.ControlState = row["ControlState"].ToString();
                newFlow.Origin = row["Origin"].ToString();
                newFlow.Parameter = BaseParameter.loadParameterByUniqueID(state.managementDB, newFlow.ParameterID);
                if (newFlow.Parameter!=null)
                {
                    if (newFlow.Parameter.ExtractedMetadata != null)
                    {
                        string ipaddress = newFlow.Parameter.ExtractedMetadata.getElement("Server");
                        if (ipaddress != null)
                        {
                            if (ipaddress != "")
                            {
                                try
                                {
                                    newFlow.meta_ipaddress = IPAddress.Parse(ipaddress);
                                }
                                catch (Exception xyz)
                                {
                                    newFlow.meta_ipaddress = null;
                                }
                            }
                        }
                    }
                }

                if (newFlow.Interval == 0)
                {
                    newFlow.Interval = 120;  // Default is 2 minute polls
                }

                newFlow.intervalTicks = DateTime.MinValue;  // While we are at it, let's prime the pump for an immediate poll.
                BaseFlowStatus.loadBaseFlowStatus(state, newFlow);
                newFlow.ParentService = Service;
                tmpFlows.Add(newFlow);
            }
            return tmpFlows;
        }

        public void reload(CollectionState State)
        {
            BaseFlow tmpFlow = BaseFlow.loadFlowByUniqueID(State.managementDB, UniqueID);
            FlowName = tmpFlow.FlowName;
            ProcessingEnabled = tmpFlow.ProcessingEnabled;
            Description = tmpFlow.Description;
            RetainDocuments = tmpFlow.RetainDocuments;
            Interval = tmpFlow.Interval;
            IndexString = tmpFlow.IndexString;
            CollectionMethod = tmpFlow.CollectionMethod;
            ParameterID = tmpFlow.ParameterID;          
            CredentialID = tmpFlow.CredentialID;
            RuleGroupID = tmpFlow.RuleGroupID;
            Parsing = tmpFlow.Parsing;
            Origin = tmpFlow.Origin;
            Enabled = tmpFlow.Enabled;
            ControlState = "ready";
            BaseFlow.updateFlow(State.managementDB, this);
            ParentService.reloadFlow(State, this);
        }

        public static void enableFlow(CollectionState State, BaseFlow currentFlow)
        {
            if (currentFlow.ControlState == "updated")
            {
                currentFlow.ControlState = "ready";
                BaseFlow.updateFlow(State.managementDB, currentFlow);
            }

            if (currentFlow.Enabled)
            {
                string dbdirectory = State.config.GetProperty("DocumentDatabaseDirectory");
                if (currentFlow.RetainDocuments)
                {
                    currentFlow.initializeDatabase(dbdirectory, DateTime.Now);
                }
                if (currentFlow.IndexString)
                {
                    currentFlow.initializeIndex(dbdirectory, DateTime.Now);
                }

                currentFlow.loadDocumentID();
                if (currentFlow.FlowStatus==null) BaseFlowStatus.loadBaseFlowStatus(State, currentFlow);
                FlowReference newReference = new FlowReference(currentFlow, State, State.MasterReceiver.onDocumentReceived);
                currentFlow.flowReference = newReference;

                ReceiverInterface currentReceiver = State.MasterReceiver.locateReceiver(State, currentFlow);

                if (currentReceiver == null)
                {
                    State.MasterReceiver.activateReceiver(State, currentFlow);
                }
                else
                {
                    currentReceiver.registerFlow(currentFlow);
                }
                if (State.searchSystem!=null)
                {
                    State.searchSystem.updateDatabases();
                }
            }
        }

        public static void stopFlow(CollectionState State, BaseFlow flow)
        {

        }
    }
}
