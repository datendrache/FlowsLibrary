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
using DatabaseAdapters;
using Proliferation.Flows.SearchCore;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Proliferation.Fatum;

namespace Proliferation.Flows
{
    public class BaseQueryHost
    {
        public int timeout = 120;  // 2 minute default
        public string InstanceID = "";
        public string InstanceHost = "";
        public string OwnerID = "";
        public string UniqueID = "";
        public string SearchID = "";
        public Tree Criteria = null;
        public Tree Result;
        public DateTime StartTime = DateTime.MinValue;
        public DateTime EndTime = DateTime.MinValue;
        public DateTime DateAdded = DateTime.MinValue;
        public Boolean running = false;
        public String Status = "";
        IntDatabase managementDB = null;

        public BaseQueryHost(IntDatabase mgrDB)
        {
            managementDB = mgrDB;
        }

        public BaseQueryHost()
        {
            managementDB = null;
        }

        public void PerformQuery()
        {
            Thread query = new Thread(new ThreadStart(Run));
            query.Start();
        }

        private void Run()
        {
            if (managementDB != null)
            {
                running = true;
                StartTime = DateTime.Now;
                updateQueryHost(managementDB, this);
                string QueryURI = Criteria.GetElement("QueryURI");

                try
                {
                    if (QueryURI != "")
                    {
                        Result = FatumLib.UriXmlToTree(FatumLib.FromSafeString(QueryURI), TreeDataAccess.WriteTreeToXmlString(Criteria, "Criteria"));

                        if (Result != null)
                        {
                            Status = "Success";
                            EndTime = DateTime.Now;
                            running = false;
                            updateQueryHost(managementDB, this);
                        }
                        else
                        {
                            Result = new Tree();
                            EndTime = DateTime.Now;
                            Status = "Failed: No data returned from server.";
                            running = false;
                            updateQueryHost(managementDB, this);
                        }
                    }
                }
                catch (Exception xyz)
                {
                    Status = "Aborted. Message: " + xyz.Message;
                }

                EndTime = DateTime.Now;
                if (Result == null) Result = new Tree();
                running = false;
                Status = "Success";
                updateQueryHost(managementDB, this);
            }
            else
            {
                Status = "Improperly formed Query: missing management database feed";
                running = false;
                EndTime = DateTime.Now;
                if (Result == null) Result = new Tree();
                updateQueryHost(managementDB, this);
            }

            if (Criteria != null)  // Because of multithreading, this has to be disposed here...
            {
                Criteria.Dispose();
                Criteria = null;
            }
        }

        static public void removeQueryHostByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [QueryHosts] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void removeQueryHostBySearchID(IntDatabase managementDB, string searchid)
        {
            String squery = "delete from [QueryHosts] where [searchID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", searchid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        public static void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [QueryHosts] ( " +
                        "[DateAdded] INTEGER NULL, " +
                        "[Start] INTEGER NULL, " +
                        "[End] INTEGER NULL, " +
                        "[InstanceID] TEXT NULL, " +
                        "[SearchID] TEXT NULL, " +
                        "[QueryURI] TEXT NULL, " +
                        "[Status] TEXT NULL, " +
                        "[OwnerID] TEXT NULL, " +
                        "[UniqueID] TEXT NULL, " +
                        "[Result] TEXT NULL, " +
                        "[Running] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [QueryHosts] (" +
                        "[DateAdded] BIGINT NULL, " +
                        "[Start] BIGINT NULL, " +
                        "[End] BIGINT NULL, " +
                        "[InstanceID] VARCHAR(33) NULL, " +
                        "[SearchID] VARCHAR(33) NULL, " +
                        "[QueryURI] NVARCHAR(MAX) NULL, " +
                        "[Status] NVARCHAR(MAX) NULL, " +
                        "[OwnerID] VARCHAR(33) NULL, " +
                        "[UniqueID] VARCHAR(33) NULL, " +
                        "[Result] TEXT NULL, " +
                        "[Running] VARCHAR(6) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public void updateQueryHost(IntDatabase managementDB, BaseQueryHost queryhost)
        {
            if (queryhost.UniqueID != "")
            {
                try
                {
                    Tree data = new Tree();
                    data.AddElement("End", queryhost.EndTime.Ticks.ToString());
                    data.AddElement("_End", "BIGINT");
                    data.AddElement("Status", queryhost.Status);
                    data.AddElement("Running", queryhost.running.ToString());

                    string scrambled = TreeDataAccess.WriteTreeToXmlString(queryhost.Result, "QueryReply");

                    data.AddElement("Result", scrambled);
                    data.AddElement("*@UniqueID", queryhost.UniqueID);

                    managementDB.UpdateTree("[QueryHosts]", data, "UniqueID=@UniqueID");
                    data.Dispose();
                }
                catch (Exception yz)
                {
                    int a = 1;
                }
            }
            else
            {
                Tree data = new Tree();
                queryhost.DateAdded = DateTime.Now;
                data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.AddElement("_DateAdded", "BIGINT");
                data.AddElement("Start", DateTime.Now.Ticks.ToString());
                data.AddElement("_Start", "BIGINT");
                data.AddElement("End", DateTime.MinValue.Ticks.ToString());
                data.AddElement("_End", "BIGINT");

                data.AddElement("InstanceID", queryhost.InstanceID);
                if (queryhost.UniqueID == "")
                {
                    queryhost.UniqueID = "O" + System.Guid.NewGuid().ToString().Replace("-", "");
                }
                data.AddElement("UniqueID", queryhost.UniqueID);
                data.AddElement("SearchID", queryhost.SearchID);
                data.AddElement("OwnerID", queryhost.OwnerID);
                data.AddElement("Status", "Initiating Connection");
                data.AddElement("Running", "true");
                data.AddElement("Result", "");
                managementDB.InsertTree("[QueryHosts]", data);
                data.Dispose();
            }
        }

        public static DataTable getQueryHostsBySearchID(IntDatabase managementDB, string SearchID)
        {
            string SQL = "select [UniqueID] from [BaseQueryHost] where [SearchID]='" + SearchID + "';";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        public static BaseQueryHost loadQueryHostByUniqueID(IntDatabase managementDB, string queryHostID)
        {
            BaseQueryHost queryhost = new BaseQueryHost();
            string SQL = "select * from [QueryHosts] where [uniqueid]=@uniqueid;";
            Tree parameters = new Tree();
            parameters.AddElement("@uniqueid", queryHostID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parameters);
            DataRow dr = null;

            if (dt.Rows.Count > 0)
            {
                dr = dt.Rows[0];
            }

            if (dr != null)
            {
                queryhost.DateAdded = new DateTime(Convert.ToInt64(dr["DateAdded"]));
                queryhost.StartTime = new DateTime(Convert.ToInt64(dr["StartTime"]));
                queryhost.EndTime = new DateTime(Convert.ToInt64(dr["EndTime"]));
                queryhost.InstanceID = dr["InstanceID"].ToString();
                queryhost.UniqueID = dr["UniqueID"].ToString();
                queryhost.SearchID = dr["SearchID"].ToString();
                queryhost.OwnerID = dr["OwnerID"].ToString();
                queryhost.Status = dr["Status"].ToString();
                string tmp = dr["Running"].ToString().ToLower();
                if (tmp == "true")
                {
                    queryhost.running = true;
                }
                else
                {
                    queryhost.running = false;
                }
                queryhost.Result = XMLTree.ReadXmlFromString(FatumLib.Unscramble(dr["Result"].ToString(), queryhost.OwnerID));
                return queryhost;
            }
            else
            {
                return null;
            }
        }

        public static void performQuery(SearchManager searchManager, SearchRequest Request, int maxCount)
        {
            if (searchManager!=null)
            {
                DateTime StartValue = DateTime.Now.AddHours(-1);
                DateTime EndValue = DateTime.Now;
                ArrayList queryTasks = new ArrayList();

                long searchCount = maxCount;
                Tree documents = new Tree();
                Tree flows = Request.Query.FindNode("Flows");
                string StartTime = Request.Query.GetElement("StartTime");
                string EndTime = Request.Query.GetElement("EndTime");

                if (StartTime == "")
                {
                    StartValue = new DateTime(long.Parse(StartTime));
                }

                if (EndTime == "")
                {
                    EndValue = new DateTime(long.Parse(EndTime));
                }

                try
                {

                    foreach (BaseFlowDB currentDB in searchManager.Databases)
                    {
                        if (currentDB.Database != null)
                        {
                            Boolean checkThisDatabase = false;
                            Boolean inDateRange = false;

                            if (currentDB.year >= StartValue.Year)
                            {
                                if (currentDB.month >= StartValue.Month)
                                {
                                    if (currentDB.day >= StartValue.Day)
                                    {
                                        inDateRange = true;
                                    }
                                }
                            }

                            if (inDateRange)
                            {
                                if (currentDB.year <= EndValue.Year)
                                {
                                    if (currentDB.month <= EndValue.Month)
                                    {
                                        if (currentDB.day <= EndValue.Day)
                                        {
                                            inDateRange = true;
                                        }
                                        else
                                        {
                                            inDateRange = false;
                                        }
                                    }
                                    else
                                    {
                                        inDateRange = false;
                                    }
                                }
                                else
                                {
                                    inDateRange = false;
                                }
                            }

                            if (inDateRange)
                            {
                                if (flows != null)
                                {
                                    Boolean performSQLSearch = true;

                                    if (flows.leafnames.Count > 0)
                                    {
                                        lock (flows.tree)
                                        {
                                            for (int i = 0; i < flows.tree.Count; i++)
                                            {
                                                string identifier = flows.leafnames[i].ToString();
                                                string name = flows.GetElement(identifier);
                                                if (identifier.ToLower() == "all")
                                                {
                                                    checkThisDatabase = true;
                                                    break;
                                                }
                                                if (identifier == currentDB.FlowID)
                                                {
                                                    checkThisDatabase = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (checkThisDatabase)
                                    {
                                        string startticks = Request.Query.GetElement("StartTime");
                                        string endticks = Request.Query.GetElement("EndTime");
                                        string LuceneQuerySyntax = "";

                                        if (startticks == "")
                                        {
                                            startticks = DateTime.MinValue.Ticks.ToString();
                                        }

                                        if (endticks == "")
                                        {
                                            endticks = DateTime.MaxValue.Ticks.ToString();
                                        }

                                        //  Lucene Search
                                        //DateTime LuceneStart = DateTime.Now;

                                        if (currentDB.IndexDirectory != null)
                                        {
                                            if (currentDB.IndexDirectory != "")
                                            {
                                                LuceneQuerySyntax = Request.Query.GetElement("LuceneSyntax");
                                                if (LuceneQuerySyntax != "")
                                                {

                                                }
                                            }
                                        }

                                        if (performSQLSearch)
                                        {
                                            if (currentDB != null)
                                            {
                                                SearchThreadDetails std = new SearchThreadDetails();
                                                std.searchterms = Request.Query.GetElement("Terms");
                                                std.maxResults = maxCount;
                                                std.LuceneQuerySyntax = LuceneQuerySyntax;
                                                std.Terms = Request.Query.FindNode("Terms");
                                                std.label = FatumLib.FromSafeString(Request.Query.GetElement("Label"));
                                                std.category = FatumLib.FromSafeString(Request.Query.GetElement("Category"));
                                                std.startticks = startticks;
                                                std.endticks = endticks;
                                                std.currentDB = currentDB;
                                                std.documents = documents;
                                                std.searchCount = searchCount;
                                                string regexcheck = Request.Query.GetElement("Regex");
                                                if (regexcheck.ToLower() == "true")
                                                {
                                                    std.regex = true;
                                                }
                                                Thread newThread = new Thread(new ParameterizedThreadStart(BaseQueryHost.process));
                                                newThread.Start(std);
                                                queryTasks.Add(newThread);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception xyz)
                {
                    int i = 0;
                }

                Boolean finished = false;

                do
                {
                    Thread.Sleep(100);
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
                Request.Result.AddNode(documents, "Documents");
            }
            else
            {
                Tree documents = new Tree();
                Request.Result.AddNode(documents, "Documents");
            }
        }

        static private string LuceneSearch(int MAX_SEARCH_ROWS, BaseFlowDB currentDB, string luceneSearchSyntax)
        {
            string result = "";

            try
            {
                Lucene.Net.Store.Directory IndexDirectory = Lucene.Net.Store.FSDirectory.Open(new System.IO.DirectoryInfo(currentDB.IndexDirectory));
                IndexReader reader = DirectoryReader.Open(IndexDirectory, true);
                Lucene.Net.Search.IndexSearcher searcher = new Lucene.Net.Search.IndexSearcher(reader);
                Lucene.Net.Analysis.Analyzer analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

                Lucene.Net.Search.Query query = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, reader.GetFieldNames(IndexReader.FieldOption.ALL).ToArray(), analyzer).Parse(luceneSearchSyntax);
                TopScoreDocCollector collector = TopScoreDocCollector.Create(MAX_SEARCH_ROWS, true);
                var sort = new Sort(new SortField("Time", SortField.LONG, true));
                var filter = new QueryWrapperFilter(query);
                searcher.Search(query, collector);
                TopDocs results = collector.TopDocs();

                foreach (ScoreDoc currentDoc in results.ScoreDocs)
                {
                    Document doc = searcher.Doc(currentDoc.Doc);
                    string content = doc.GetFieldable("ID").StringValue;
                    if (content != null)
                    {
                        if (result == "")
                        {
                            result = content.ToString();
                        }
                        else
                        {
                            result += "," + content.ToString();
                        }
                    }
                }
            }
            catch (Exception xyz)
            {
                int abc = 1;
            }
            return result;
        }

        public static void process(Object details)
        {
            //  Detailed Search
            SearchThreadDetails std = (SearchThreadDetails) details;

            if (std.currentDB != null)
            {
                try
                {
                    string SearchWhereClause = "";
                    Boolean performSQLSearch = true;
                    if (std.LuceneQuerySyntax!="")
                    {
                        string SearchList = LuceneSearch(std.maxResults, std.currentDB, std.LuceneQuerySyntax);
                        if (SearchList == "")
                        {
                            performSQLSearch = false;
                        }
                        SearchWhereClause = "[ID] IN (" + SearchList + ")";
                    }
                    
                    if (performSQLSearch)
                    {
                        string searchterms = std.searchterms;
                        Tree parms = new Tree();
                        string query = "select * from [documents] where ([Received] >= @startticks and [Received] <= @endticks) ";

                        if (SearchWhereClause != "")
                        {
                            query += "and " + SearchWhereClause + " ";
                        }

                        string termswhere = "";

                        Tree terms = std.Terms;
                        if (terms != null)
                        {
                            lock (terms.tree)
                            {
                                for (int i = 0; i < terms.tree.Count; i++)
                                {
                                    string name = terms.leafnames[i].ToString();
                                    string value = FatumLib.FromSafeString(terms.GetElement(name));
                                    string tmp = "";

                                    if (std.regex)
                                    {
                                        tmp = " and [document] REGEXP @term " + i.ToString();
                                        parms.AddElement("@term" + i.ToString(), value);
                                    }
                                    else
                                    {
                                        tmp = " and [document] like @term " + i.ToString();
                                        parms.AddElement("@term" + i.ToString(), "%" + value + "%");
                                    }

                                    if (termswhere == "")
                                    {
                                        termswhere = tmp;
                                    }
                                    else
                                    {
                                        termswhere = " and " + tmp;
                                    }
                                }
                            }
                        }
                        if (termswhere != "") query += termswhere;

                        if (std.label != "")
                        {
                            string tmp = " [label]=@label COLLATE NOCASE ";
                            parms.AddElement("@label", tmp);
                            if (query != "")
                            {
                                query += " and " + tmp;
                            }
                            else
                            {
                                query = " where " + tmp;
                            }
                        }

                        if (std.category != "")
                        {
                            string tmp = "[category]=@category COLLATE NOCASE ";
                            parms.AddElement("@category", tmp);
                            if (query != "")
                            {
                                query += " and " + tmp;
                            }
                            else
                            {
                                query = " where " + tmp;
                            }
                        }

                        parms.AddElement("@startticks", std.startticks);
                        parms.AddElement("@endticks", std.endticks);
                        query += " order by [Received] desc limit " + std.searchCount.ToString() + ";";

                        try
                        {
                            DataTable newTable = std.currentDB.Database.ExecuteDynamic(query, parms);

                            foreach (DataRow currentrow in newTable.Rows)
                            {
                                Tree rowinfo = new Tree();
                                rowinfo.AddElement("Received", currentrow["Received"].ToString());
                                rowinfo.AddElement("Label", FatumLib.ToSafeString(currentrow["Label"].ToString()));
                                rowinfo.AddElement("Category", FatumLib.ToSafeString(currentrow["Category"].ToString()));
                                rowinfo.AddElement("ID", currentrow["ID"].ToString());
                                rowinfo.AddElement("FlowID", std.currentDB.FlowID);
                                rowinfo.AddElement("Metadata", FatumLib.ToSafeString(currentrow["Metadata"].ToString()));
                                rowinfo.AddElement("Document", FatumLib.ToSafeString(currentrow["Document"].ToString()));
                                std.documents.AddNode(rowinfo, std.currentDB.FlowID);
                            }
                        }
                        catch (Exception xz)
                        {
                            int x = 0;
                        }
                    }
                }
                catch (Exception xyz)
                {
                    int x = 0;
                }
            } 
        }
    }
}
