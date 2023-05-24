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
using System.Net;
using Proliferation.Fatum;
using Proliferation.FatumAnalytics;
using System.Data.SQLite;
using DatabaseAdapters;

namespace Proliferation.Flows
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
                        string ipaddress = newFlows.Parameter.ExtractedMetadata.GetElement("Server");
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
                    data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                    data.AddElement("_DateAdded", "BIGINT");
                    data.AddElement("FlowName", flow.FlowName);
                    data.AddElement("ServiceID", flow.ServiceID);
                    data.AddElement("ProcessingEnabled", flow.ProcessingEnabled.ToString());
                    flow.UniqueID = "F" + System.Guid.NewGuid().ToString().Replace("-", "");
                    data.AddElement("UniqueID", flow.UniqueID);
                    data.AddElement("Interval", flow.Interval.ToString());
                    data.AddElement("IndexString", flow.IndexString.ToString());
                    data.AddElement("RetainDocuments", flow.RetainDocuments.ToString());
                    data.AddElement("CollectionMethod", flow.CollectionMethod);
                    data.AddElement("OwnerID", flow.OwnerID);
                    data.AddElement("GroupID", flow.GroupID);
                    data.AddElement("CredentialID", flow.CredentialID);
                    data.AddElement("ParameterID", flow.ParameterID);
                    data.AddElement("ControlState", flow.ControlState);
                    data.AddElement("RuleGroupID", flow.RuleGroupID);
                    data.AddElement("Parsing", flow.Parsing);
                    data.AddElement("Origin", flow.Origin);
                    data.AddElement("Description", flow.Description);
                    data.AddElement("Enabled", flow.Enabled.ToString());
                    managementDB.InsertTree("[Flows]", data);
                    data.Dispose();
                }
                catch (Exception xyz)
                {
                    System.Console.Out.WriteLine("BaseFlow- Error creating new flow." + xyz.Message + ", " + xyz.StackTrace);
                }
            }
            else
            {
                Tree data = new Tree();
                data.AddElement("ProcessingEnabled", flow.ProcessingEnabled.ToString());
                data.AddElement("Description", flow.Description);
                data.AddElement("FlowName", flow.FlowName);
                data.AddElement("Interval", flow.Interval.ToString());
                data.AddElement("IndexString", flow.IndexString.ToString());
                data.AddElement("CollectionMethod", flow.CollectionMethod);
                data.AddElement("RetainDocuments", flow.RetainDocuments.ToString());
                data.AddElement("OwnerID", flow.OwnerID);
                data.AddElement("GroupID", flow.GroupID);
                data.AddElement("CredentialID", flow.CredentialID);
                data.AddElement("ControlState", flow.ControlState);
                data.AddElement("RuleGroupID", flow.RuleGroupID);
                data.AddElement("ParameterID", flow.ParameterID);
                data.AddElement("Parsing", flow.Parsing);
                data.AddElement("Origin", flow.Origin);
                data.AddElement("Enabled", flow.Enabled.ToString());
                data.AddElement("*@UniqueID", flow.UniqueID);
                managementDB.UpdateTree("[Flows]", data, "[UniqueID]=@UniqueID");
                data.Dispose();
            }
        }

        static public void removeFlowsByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Flows] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void addFlow(IntDatabase managementDB, Tree description)
        {
            Tree data = new Tree();
            data.AddElement("ProcessingEnabled", description.GetElement("ProcessingEnabled"));
            data.AddElement("Description", description.GetElement("Description"));
            data.AddElement("FlowName", description.GetElement("FlowName"));
            data.AddElement("Interval", description.GetElement("Interval"));
            data.AddElement("IndexString", description.GetElement("IndexString"));
            data.AddElement("CollectionMethod", description.GetElement("TimeString"));
            data.AddElement("RetainDocuments", description.GetElement("RetainDocuments"));
            data.AddElement("OwnerID", description.GetElement("OwnerID"));
            data.AddElement("GroupID", description.GetElement("GroupID"));
            data.AddElement("CredentialID", description.GetElement("CredentialID"));
            data.AddElement("ControlState", description.GetElement("ControlState"));
            data.AddElement("RuleGroupID", description.GetElement("RuleGroupID"));
            data.AddElement("ParameterID", description.GetElement("ParameterID"));
            data.AddElement("Parsing", description.GetElement("Parsing"));
            data.AddElement("Origin", description.GetElement("Origin"));
            data.AddElement("UniqueID", description.GetElement("UniqueID"));
            data.AddElement("Enabled", description.GetElement("Enabled"));
            managementDB.InsertTree("[Flows]", data);
            data.Dispose();
        }

        static public string locateID(CollectionState State, BaseFlow flow)
        {
            string result = "";
            DataTable flows;
            String query = "select [ID] from [Flows] WHERE [ServiceID]=@serviceid;";

            Tree parms = new Tree();
            parms.AddElement("@serviceid", flow.ServiceID);

            flows = State.managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
            TreeDataAccess.WriteXML(outs, tmp, "BaseFlow");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseFlow current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("FlowName", current.FlowName);
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("ServiceID", current.ServiceID);
            tmp.AddElement("ProcessingEnabled", current.ProcessingEnabled.ToString());
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("RetainDocuments", current.RetainDocuments.ToString());
            tmp.AddElement("Interval", current.Interval.ToString());
            tmp.AddElement("IndexString", current.IndexString.ToString());
            tmp.AddElement("CollectionMethod", current.CollectionMethod);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("ParameterID", current.ParameterID);
            tmp.AddElement("CredentialID", current.CredentialID);
            tmp.AddElement("ControlState", current.ControlState);
            tmp.AddElement("RuleGroupID", current.RuleGroupID);
            tmp.AddElement("Parsing", current.Parsing);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("Enabled", current.Enabled.ToString());
            return tmp;
        }

        public BaseFlow(Tree XML)
        {
            FlowName = XML.GetElement("FlowName");
            DateAdded = XML.GetElement("DateAdded");
            ServiceID = XML.GetElement("ServiceID");
            string processing = XML.GetElement("ProcessingEnabled");
            if (processing.ToLower()=="true")
            {
                ProcessingEnabled = true;
            }
            else
            {
                ProcessingEnabled = false;
            }

            Description = XML.GetElement("Description");
            string retain = XML.GetElement("RetainDocuments");
            if (retain.ToLower() == "true")
            {
                RetainDocuments = true;
            }
            else
            {
                RetainDocuments = false;
            }

            string tmpinterval = XML.GetElement("Internal");
            if (tmpinterval=="")
            {
                Interval = 60;
            }
            else
            {
                Interval = int.Parse(tmpinterval);
            }
            
            string index = XML.GetElement("IndexString");
            if (index.ToLower() == "true")
            {
                IndexString = true;
            }
            else
            {
                IndexString = false;
            }
            CollectionMethod = XML.GetElement("CollectionMethod");
            CredentialID = XML.GetElement("CredentialID");
            ParameterID = XML.GetElement("ParameterID");
            ControlState = XML.GetElement("ControlState");
            Parsing = XML.GetElement("Parsing");
            Origin = XML.GetElement("Origin");
            RuleGroupID = XML.GetElement("RuleGroupID");

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
                    string ipaddress = Parameter.ExtractedMetadata.GetElement("Server");
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

            string tmpbool = XML.GetElement("Enabled").ToLower();
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
            data.AddElement("@instanceid", InstanceID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
            return dt;
        }

        public static DataTable getFlowListRemovingByInstance(IntDatabase managementDB, string InstanceID)
        {
            string SQL = "select f.[UniqueID], f.[ControlState] from [Flows] as f join [Services] as svc on f.ServiceID=svc.UniqueID join [Sources] as so on svc.SourceID=so.UniqueID where so.InstanceID=@instanceid and (f.ControlState = 'removing' or f.ControlState = 'transferring');";
            Tree data = new Tree();
            data.AddElement("@instanceid", InstanceID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
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
            parms.AddElement("@uid", uniqueid);
            flows = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
                        string ipaddress = newFlow.Parameter.ExtractedMetadata.GetElement("Server");
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
            data.AddElement("@FlowID", FlowID);
            managementDB.ExecuteDynamic(SQL, data);
            data.Dispose();
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
                        string ipaddress = newFlow.Parameter.ExtractedMetadata.GetElement("Server");
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
