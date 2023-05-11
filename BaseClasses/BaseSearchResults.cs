using DatabaseAdapters;
using Fatum.FatumCore;
using FatumCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhlozLib
{
    public class BaseSearchResults
    {
        public static void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [SearchResults] ( " +
                        "[SearchID] TEXT NULL, " +
                        "[Position] INTEGER NULL, " +
                        "[UserID] TEXT NULL, " +
                        "[Received] INTEGER NULL, " +
                        "[FlowID] TEXT NULL, " +
                        "[Label] TEXT NULL, " +
                        "[Category] TEXT NULL, " +
                        "[Document] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [SearchResults] ( " +
                        "[SearchID] VARCHAR(33) NULL, " +
                        "[Position] INT NULL, " +
                        "[UserID] VARCHAR(33) NULL, " +
                        "[Received] BIGINT NULL, " +
                        "[FlowID] VARCHAR(33) NULL, " +
                        "[Label] NVARCHAR(100) NULL, " +
                        "[Category] NVARCHAR(100)NULL, " +
                        "[Document] TEXT NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        public static void storeSearch(IntDatabase managementDB, string SearchID, string UserID, DataTable documentTable)
        {
            try
            {
                DataTable searchResults = BaseSearch.getQueryResults(managementDB, SearchID);
                ArrayList queryTasks = new ArrayList();
                ArrayList rendered = new ArrayList();
                DataTable mimes = BaseFlow.getFlowListMimeOnly(managementDB);

                foreach (DataRow row in searchResults.Rows)
                {
                    Tree rowdata = XMLTree.readXMLFromString((string)row["Result"]);

                    if (rowdata.tree.Count>0)
                    {
                        Tree documents = (Tree)rowdata.tree[0];
                        foreach (Tree currentDocument in documents.tree)
                        {
                            RenderItem newItem = new RenderItem();
                            newItem.mimes = mimes;
                            rendered.Add(newItem);
                            newItem.currentDocument = currentDocument;
                            Thread newThread = new Thread(new ParameterizedThreadStart(render));
                            newThread.Start(newItem);
                            queryTasks.Add(newThread);
                        }

                        Boolean finished = false;
                        do
                        {
                            Thread.Sleep(10);
                            Boolean anythingrunning = false;

                            foreach (Thread current in queryTasks)
                            {
                                if (current.IsAlive)
                                {
                                    anythingrunning = true;
                                    break;
                                }
                            }
                            if (!anythingrunning) finished = true;
                        } while (!finished);

                        queryTasks.Clear();

                        foreach (RenderItem ri in rendered)
                        {
                            DataRow renderedrow = documentTable.NewRow();
                            renderedrow[0] = "";
                            renderedrow[1] = ri.received.ToString();
                            renderedrow[2] = ri.flowid;
                            renderedrow[3] = ri.category;
                            renderedrow[4] = ri.label;
                            renderedrow[5] = ri.document;
                            renderedrow[6] = ri.metadata;
                            renderedrow[7] = ri.body;

                            documentTable.Rows.Add(renderedrow);
                        }
                        documents.dispose();
                    }
                    rowdata.dispose();
                }


                BaseQueryHost.removeQueryHostBySearchID(managementDB, SearchID);
            }
            catch (Exception xyz)
            {
                int a = 1;
            }
        }

        public static DataTable fetchSearch(IntDatabase managementDB, string searchid, string userid)
        {
            string SQL = "select [Received], [FlowID], [Label], [Category], [Document] from [SearchResults] where [SearchID]=@searchid and [UserID]=@userid order by Received desc;";
            Tree data = new Tree();
            data.addElement("@searchid", searchid);
            data.addElement("@userid", userid);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            return dt;
        }

        public static void render(Object o)
        {
            RenderItem ri = (RenderItem)o;
            ri.received = long.Parse(ri.currentDocument.getElement("Received"));
            ri.flowid = ri.currentDocument.getElement("FlowID");
            ri.label = FatumLib.fromSafeString(ri.currentDocument.getElement("Label"));
            ri.category = FatumLib.fromSafeString(ri.currentDocument.getElement("Category"));
            ri.document = FatumLib.fromSafeString(ri.currentDocument.getElement("Document"));
            ri.metadata = FatumLib.fromSafeString(ri.currentDocument.getElement("Metadata"));
            ri.body = Populate(findParse(ri.mimes, ri.flowid), ri.document);
        }

        public static string findParse(DataTable mimes, string flowid)
        {
            string result = "text/ascii";
            foreach (DataRow row in mimes.Rows)
            {
               if (row[0].ToString()==flowid)
                {
                    return (row[1].ToString());
                }
            }
            return result;
        }

        public static string Populate(string parsing, string document)
        {
            switch (parsing.ToLower())
            {
                case "application/wmi":
                    return PhlozLib.DocumentDisplay.WMIDisplay.DocumentToHTML(document);
                    break;
                case "application/twitter":
                    return PhlozLib.DocumentDisplay.TwitterDisplay.DocumentToHTML(document);
                    break;
                case "application/rss+xml":
                    return PhlozLib.DocumentDisplay.RSSDisplay.DocumentToHTML(document);
                    break;
                case "application/syslog":
                case "regex":
                    return PhlozLib.DocumentDisplay.SyslogDisplay.DocumentToHTML(document);
                    break;
                case "message/rfc822":
                    return PhlozLib.DocumentDisplay.RFC822Display.DocumentToHTML(document);
                    break;
                default:
                    return document;
                    break;
            }
        }
    }
}
