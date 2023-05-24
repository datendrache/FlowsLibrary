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
using System.Data;
using Lucene.Net.Documents;
using DatabaseAdapters;
using System.Collections;

namespace Proliferation.Flows
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
                Metadata.Dispose();
                Metadata = null;
            }
        }

        public BaseDocument(CollectionState State, Tree info)
        {
            received = Convert.ToDateTime(info.GetElement("received"));
            ID = Convert.ToInt32(info.GetElement("ID"));
            Document = FatumLib.FromSafeString(info.GetElement("Document"));
            FlowID = info.GetElement("FlowID");
            assignedFlow = BaseFlow.locateCachedFlowByUniqueID(FlowID,State);
            Label = info.GetElement("Label");
            Category = info.GetElement("Category");
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
            received = Convert.ToDateTime(info.GetElement("received"));
            ID = Convert.ToInt32(info.GetElement("ID"));
            Document = FatumLib.FromSafeString(info.GetElement("Document"));
            FlowID = info.GetElement("FlowID");
            Label = info.GetElement("Label");
            Category = info.GetElement("Category");
            Metadata = info.FindNode("Metadata");
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
                    DocumentOut.AddElement("Received", DateTime.Now.Ticks.ToString());
                    DocumentOut.AddElement("_Received", "INTEGER");
                    DocumentOut.AddElement("Label", msg.Label);
                    DocumentOut.AddElement("ID", msg.ID.ToString());
                    DocumentOut.AddElement("Category", msg.Category);
                    if (msg.Metadata == null)
                    {
                        DocumentOut.AddElement("MetaData", "");
                    }
                    else
                    {
                        if (msg.Metadata.DEALLOCATED)
                        {
                            DocumentOut.AddElement("MetaData", "");
                        }
                        else
                        {
                            if (msg.Metadata.leafnames.Count > 0)
                            {
                                DocumentOut.AddElement("MetaData", TreeDataAccess.WriteTreeToXmlString(msg.Metadata, "Metadata"));
                            }
                            else
                            {
                                DocumentOut.AddElement("MetaData", "");
                            }
                        }
                    }
                    
                    DocumentOut.AddElement("Document", msg.Document);

                    msg.assignedFlow.documentDB.InsertTree("[Documents]", DocumentOut);
                    DocumentOut.Dispose();
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
                                string value = msg.Metadata.GetElement(leafname);
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

            information.SetElement("ID", ID.ToString());
            information.SetElement("Time", received.ToString());
            information.SetElement("Label", Label);
            information.SetElement("Category", Category);
            information.SetElement("Document", Document);
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

            if (Metadata!=null) Metadata.Dispose();
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
            parms.AddElement("@documentid", documentuid);
            DataTable table = database.ExecuteDynamic(SQL, parms);
            parms.Dispose();

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
                        result.Metadata = XMLTree.ReadXmlFromString(metadata);
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

            tmp.AddElement("ID", current.ID.ToString());
            tmp.AddElement("received", current.received.ToString());
            tmp.AddElement("Document", current.Document);
            tmp.AddElement("FlowID", current.FlowID);
            tmp.AddElement("Label", current.Label);
            tmp.AddElement("Category", current.Category);

            if (current.Metadata==null)
            {
                tmp.AddNode(new Tree(), "Metadata");
            }
            else
            {
                tmp.AddNode(current.Metadata.Duplicate(), "Metadata");
            }

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseDocument");
            tmp.Dispose();
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
                                msgOut[3] = TreeDataAccess.WriteTreeToXmlString(msg.Metadata, "Metadata");
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
