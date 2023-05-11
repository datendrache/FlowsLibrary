//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Threading;
using FatumCore;
using System.Data;
using System.IO;
using Lucene.Net.Documents;
using DatabaseAdapters;
using System.Collections;
using Fatum.FatumCore;

namespace PhlozLib
{
    public class BaseDocument
    {
        public DateTime received;
        public String Document = "";
        public String FlowID = "";
        public String Label = "";
        public String Category = "";
        public int ID;
        public BaseFlow assignedFlow;
        public Tree Metadata;
        public BaseRule triggeredRule = null;
        
        public BaseDocument(BaseFlow flow)
        {
            ID = flow.documentcount++;
            assignedFlow = flow;
            FlowID = flow.UniqueID;
            Metadata = new Tree();
        }
        

        ~BaseDocument()
        {
            Document = "";
            FlowID = "";
            Label = "";
            Category = "";
            triggeredRule = null;

            assignedFlow = null;
            if (Metadata != null)
            {
                Metadata.dispose();
                Metadata = null;
            }
        }

        public BaseDocument(CollectionState State, Tree info)
        {
            received = Convert.ToDateTime(info.getElement("received"));
            ID = Convert.ToInt32(info.getElement("ID"));
            Document = FatumLib.fromSafeString(info.getElement("Document"));
            FlowID = info.getElement("FlowID");
            assignedFlow = BaseFlow.locateCachedFlowByUniqueID(FlowID,State);
            Label = info.getElement("Label");
            Category = info.getElement("Category");
            Metadata = new Tree();
        }

        public BaseDocument(ArrayList documentinfo, CollectionState State)
        {
            received = DateTime.Now;
            Document = documentinfo[0].ToString();
            FlowID = documentinfo[1].ToString();
            assignedFlow = BaseFlow.locateCachedFlowByUniqueID(FlowID, State);
            Label = documentinfo[2].ToString();
            Category = documentinfo[3].ToString();
            Metadata = (Tree) documentinfo[4];
        }

        // ONLY USE IF THIS IS AN ENDPOINT -- IT DOES NOT CREATE A DOCUMENT WITH A MAPPED ASSIGNED FLOW POINTER 

        public BaseDocument(Tree info)
        {
            received = Convert.ToDateTime(info.getElement("received"));
            ID = Convert.ToInt32(info.getElement("ID"));
            Document = FatumLib.fromSafeString(info.getElement("Document"));
            FlowID = info.getElement("FlowID");
            Label = info.getElement("Label");
            Category = info.getElement("Category");
            Metadata = info.findNode("Metadata");
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Documents]( [ID] INTEGER NULL, " +
                    "[Received] INTEGER NULL," +
                    "[Label] TEXT NULL, " +
                    "[Category] TEXT NULL, " +
                    "[Metadata] TEXT NULL, " +
                    "[Document] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Documents]( [ID] INTEGER NULL, " +
                    "[Received] BIGINT NULL," +
                    "[Label] NVARCHAR(100) NULL, " +
                    "[Category] NVARCHAR(100) NULL, " +
                    "[Metadata] TEXT NULL, " +
                    "[Document] TEXT NULL );";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_documents ON Documents([Received]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_documents ON Documents([Received]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        public static void insertDocumentBulk(CollectionState State, BaseDocument document)
        {
            if (document != null)
            {
                if (document.assignedFlow != null)
                {
                    if (!document.assignedFlow.Suspended)
                    {
                        lock (document.assignedFlow.bulkLock)
                        {
                            lock (document.assignedFlow.bulkInsert)
                            {
                                try
                                {
                                    while (document.assignedFlow.bulkInsert == null)
                                    {
                                        Thread.Sleep(0);
                                    };
                                    document.assignedFlow.bulkInsert.AddLast(document);
                                }
                                catch (NullReferenceException xyz)
                                {
                                    document.assignedFlow.bulkInsert = new LinkedList<BaseDocument>();
                                    document.assignedFlow.bulkInsert.AddLast(document);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public static void insertDocument(BaseDocument msg)
        {
            // First insert into database
            string insert = "";

            try
            {
                if (msg.Document != null)
                {
                    Tree DocumentOut = new Tree();
                    DocumentOut.addElement("Received", DateTime.Now.Ticks.ToString());
                    DocumentOut.addElement("_Received", "INTEGER");
                    DocumentOut.addElement("Label", msg.Label);
                    DocumentOut.addElement("ID", msg.ID.ToString());
                    DocumentOut.addElement("Category", msg.Category);
                    if (msg.Metadata == null)
                    {
                        DocumentOut.addElement("MetaData", "");
                    }
                    else
                    {
                        if (msg.Metadata.DEALLOCATED)
                        {
                            DocumentOut.addElement("MetaData", "");
                        }
                        else
                        {
                            if (msg.Metadata.leafnames.Count > 0)
                            {
                                DocumentOut.addElement("MetaData", TreeDataAccess.writeTreeToXMLString(msg.Metadata, "Metadata"));
                            }
                            else
                            {
                                DocumentOut.addElement("MetaData", "");
                            }
                        }
                    }
                    
                    DocumentOut.addElement("Document", msg.Document);

                    msg.assignedFlow.documentDB.InsertTree("[Documents]", DocumentOut);
                    DocumentOut.dispose();
                }
                else
                {
                    string consoleMsg = msg.assignedFlow.UniqueID + ", " + msg.assignedFlow.FlowName + "\r\nDocument: " +
                       "Document is null value! (from BaseDocument.insertDocument())";
                    System.Console.Out.WriteLine(consoleMsg);
                }
            }
            catch (Exception xyz)
            {
                string consoleMsg = msg.assignedFlow.UniqueID + ", " + msg.assignedFlow.FlowName + "\r\nDocument: " +
                       "Error writing to database (from BaseDocument.insertDocument()): " + xyz.Message + "\r\n" + xyz.StackTrace + ": " + insert;
                System.Console.Out.WriteLine(consoleMsg);
            }
        }

        public static void indexDocument(BaseDocument msg)
        {
            try
            {
                Document newDoc = new Document();
                newDoc.Add(new NumericField("ID", Field.Store.YES, true).SetIntValue(msg.ID));
                newDoc.Add(new Field("FlowID", msg.FlowID, Field.Store.YES, Field.Index.ANALYZED));
                newDoc.Add(new Field("Category", msg.Category, Field.Store.YES, Field.Index.ANALYZED));
                newDoc.Add(new Field("Label", msg.Label, Field.Store.YES, Field.Index.ANALYZED));
                if (msg.Metadata != null)
                {
                    if (!msg.Metadata.DEALLOCATED)
                    {
                        foreach (string leafname in msg.Metadata.leafnames)
                        {
                            if (newDoc.Get(leafname) != "")
                            {
                                string value = msg.Metadata.getElement(leafname);
                                newDoc.Add(new Field(leafname, value, Field.Store.YES, Field.Index.ANALYZED));
                            }
                        }
                    }
                }
                if (msg.assignedFlow.indexer!=null)
                {
                    if (msg.assignedFlow.indexer.indexer!=null)
                    {
                        msg.assignedFlow.indexer.indexer.AddDocument(newDoc);
                    }
                }
            }
            catch (Exception xyz)
            {
                //System.Console.Out.WriteLine("");
            }
        }

        public Tree getMetadata()
        {
            Tree information = new Tree();

            information.setElement("ID", ID.ToString());
            information.setElement("Time", received.ToString());
            information.setElement("Label", Label);
            information.setElement("Category", Category);
            information.setElement("Document", Document);
            information.Value = "Metadata";

            return information;
        }

        public void dispose()
        {
            Document = null;
            FlowID = null;
            Label = null;
            Category = null;
            //hostaddress = null;

            assignedFlow = null;

            if (Metadata!=null) Metadata.dispose();
            Metadata = null;
            triggeredRule = null;
        }

        public BaseDocument copy()
        {
            BaseDocument result = new BaseDocument(assignedFlow);

            if (Document!=null) result.Document = Document;
            if (FlowID!=null) result.FlowID = FlowID;
            if (received!=null) result.received = received;
            if (result!=null) result.Label = Label;
            if (Category!=null) result.Category = Category;
            if (assignedFlow!=null) result.assignedFlow = assignedFlow;
            result.ID = assignedFlow.documentcount++;
            if (Metadata!=null) result.Metadata = Metadata.Duplicate();

            return result;
        }

        public static BaseDocument getDocument(CollectionState State, string FlowID, string DateString, string documentuid)
        {
            BaseDocument result = null;

            BaseFlow flow = BaseFlow.locateCachedFlowByUniqueID(FlowID, State);
            DateTime when = Convert.ToDateTime(DateString);

            SQLiteDatabase database = new SQLiteDatabase(flow.directoryPicker(flow.DatabaseDirectory,when));
            string SQL = "select * from [Documents] where [ID]=@documentid;";
            Tree parms = new Tree();
            parms.addElement("@documentid", documentuid);
            DataTable table = database.ExecuteDynamic(SQL, parms);
            parms.dispose();

            if (table.Rows.Count > 0)
            {
                var msg = table.Rows[0];
                string value = msg["ID"].ToString();
                long idnum = 0;
                long.TryParse(value, out idnum);
                result = new BaseDocument(BaseFlow.locateCachedFlowByUniqueID(msg["FlowID"].ToString(),State));
                long ticks = 0;
                long.TryParse(msg["Received"].ToString(), out ticks);
                result.received = new DateTime(ticks);
                result.Document = msg["Document"].ToString();
                result.FlowID = FlowID;
                result.Label = msg["Label"].ToString();
                result.Category = msg["Category"].ToString();
                string metadata = msg["Metadata"].ToString();
                if (metadata != null)
                {
                    if (metadata != "")
                    {
                        result.Metadata = XMLTree.readXMLFromString(metadata);
                    }
                    else
                    {
                        result.Metadata = new Tree();
                    }
                }
                else
                {
                    result.Metadata = new Tree();
                }
                
            }
            return result;
        }

        static public string getXML(BaseDocument current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("ID", current.ID.ToString());
            tmp.addElement("received", current.received.ToString());
            tmp.addElement("Document", current.Document);
            tmp.addElement("FlowID", current.FlowID);
            tmp.addElement("Label", current.Label);
            tmp.addElement("Category", current.Category);

            if (current.Metadata==null)
            {
                tmp.addNode(new Tree(), "Metadata");
            }
            else
            {
                tmp.addNode(current.Metadata.Duplicate(), "Metadata");
            }

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseDocument");
            tmp.dispose();
            result = outs.ToString();
            //result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            result = result.Substring(41, result.Length - 43);
            return result;
        }

        public static void insertPreparedDocument(BaseDocument msg)
        {
            // First insert into database
            string insert = "";

            try
            {
                if (msg.Document != null)
                {
                    string[] msgOut = new string[6];

                    msgOut[0] = DateTime.Now.Ticks.ToString();
                    msgOut[1] = msg.Label;
                    msgOut[2] = msg.Category;
                    
                    if (msg.Metadata == null)
                    {
                        msgOut[3] = "";
                    }
                    else
                    {
                        if (msg.Metadata.DEALLOCATED)
                        {
                            msgOut[3] = "";
                        }
                        else
                        {
                            if (msg.Metadata.leafnames.Count > 0)
                            {
                                msgOut[3] = TreeDataAccess.writeTreeToXMLString(msg.Metadata, "Metadata");
                            }
                            else
                            {
                                msgOut[3] = "";
                            }
                        } 
                    }
                    msgOut[4] = msg.ID.ToString();
                    msgOut[5] = msg.Document;
                    msg.assignedFlow.documentDB.InsertPreparedDocument(msgOut);
                }
                else
                {
                    int xzyz = 1;  // DOCUMENT IS NULL SHOULD NOT HAPPEN
                }
            }
            catch (Exception xyz)
            {
                int xtee = 1; // ERROR
            }
        }
    }
}
