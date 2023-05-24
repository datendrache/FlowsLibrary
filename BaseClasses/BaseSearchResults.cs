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

using DatabaseAdapters;
using Proliferation.Fatum;
using System.Collections;
using System.Data;

namespace Proliferation.Flows
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
                    Tree rowdata = XMLTree.ReadXmlFromString((string)row["Result"]);

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
                        documents.Dispose();
                    }
                    rowdata.Dispose();
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
            data.AddElement("@searchid", searchid);
            data.AddElement("@userid", userid);
            DataTable dt = managementDB.ExecuteDynamic(SQL, data);
            return dt;
        }

        public static void render(Object o)
        {
            RenderItem ri = (RenderItem)o;
            ri.received = long.Parse(ri.currentDocument.GetElement("Received"));
            ri.flowid = ri.currentDocument.GetElement("FlowID");
            ri.label = FatumLib.FromSafeString(ri.currentDocument.GetElement("Label"));
            ri.category = FatumLib.FromSafeString(ri.currentDocument.GetElement("Category"));
            ri.document = FatumLib.FromSafeString(ri.currentDocument.GetElement("Document"));
            ri.metadata = FatumLib.FromSafeString(ri.currentDocument.GetElement("Metadata"));
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
                    return DocumentDisplay.WMIDisplay.DocumentToHTML(document);
                    break;
                case "application/twitter":
                    return DocumentDisplay.TwitterDisplay.DocumentToHTML(document);
                    break;
                case "application/rss+xml":
                    return DocumentDisplay.RSSDisplay.DocumentToHTML(document);
                    break;
                case "application/syslog":
                case "regex":
                    return DocumentDisplay.SyslogDisplay.DocumentToHTML(document);
                    break;
                case "message/rfc822":
                    return DocumentDisplay.RFC822Display.DocumentToHTML(document);
                    break;
                default:
                    return document;
                    break;
            }
        }
    }
}
