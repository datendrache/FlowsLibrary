using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseAdapters;
using FatumCore;
using PhlozLib.SearchCore;
using Lucene.Net.Search;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using System.ServiceModel.Security;
using System.Web.UI.WebControls;
using TweetSharp;
using Fatum.FatumCore;

namespace PhlozLib
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
                string QueryURI = Criteria.getElement("QueryURI");

                try
                {
                    if (QueryURI != "")
                    {
                        Result = FatumLib.URIXMLtoTree(FatumLib.fromSafeString(QueryURI), TreeDataAccess.writeTreeToXMLString(Criteria, "Criteria"));

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
                Criteria.dispose();
                Criteria = null;
            }
        }

        static public void removeQueryHostByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [QueryHosts] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void removeQueryHostBySearchID(IntDatabase managementDB, string searchid)
        {
            String squery = "delete from [QueryHosts] where [searchID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", searchid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
                    data.addElement("End", queryhost.EndTime.Ticks.ToString());
                    data.addElement("_End", "BIGINT");
                    data.addElement("Status", queryhost.Status);
                    data.addElement("Running", queryhost.running.ToString());

                    string scrambled = TreeDataAccess.writeTreeToXMLString(queryhost.Result, "QueryReply");

                    data.addElement("Result", scrambled);
                    data.addElement("*@UniqueID", queryhost.UniqueID);

                    managementDB.UpdateTree("[QueryHosts]", data, "UniqueID=@UniqueID");
                    data.dispose();
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
                data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.addElement("_DateAdded", "BIGINT");
                data.addElement("Start", DateTime.Now.Ticks.ToString());
                data.addElement("_Start", "BIGINT");
                data.addElement("End", DateTime.MinValue.Ticks.ToString());
                data.addElement("_End", "BIGINT");

                data.addElement("InstanceID", queryhost.InstanceID);
                if (queryhost.UniqueID == "")
                {
                    queryhost.UniqueID = "O" + System.Guid.NewGuid().ToString().Replace("-", "");
                }
                data.addElement("UniqueID", queryhost.UniqueID);
                data.addElement("SearchID", queryhost.SearchID);
                data.addElement("OwnerID", queryhost.OwnerID);
                data.addElement("Status", "Initiating Connection");
                data.addElement("Running", "true");
                data.addElement("Result", "");
                managementDB.InsertTree("[QueryHosts]", data);
                data.dispose();
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
            parameters.addElement("@uniqueid", queryHostID);
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
                queryhost.Result = XMLTree.readXMLFromString(FatumLib.Unscramble(dr["Result"].ToString(), queryhost.OwnerID));
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
                Tree flows = Request.Query.findNode("Flows");
                string StartTime = Request.Query.getElement("StartTime");
                string EndTime = Request.Query.getElement("EndTime");

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
                                                string name = flows.getElement(identifier);
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
                                        string startticks = Request.Query.getElement("StartTime");
                                        string endticks = Request.Query.getElement("EndTime");
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
                                                LuceneQuerySyntax = Request.Query.getElement("LuceneSyntax");
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
                                                std.searchterms = Request.Query.getElement("Terms");
                                                std.maxResults = maxCount;
                                                std.LuceneQuerySyntax = LuceneQuerySyntax;
                                                std.Terms = Request.Query.findNode("Terms");
                                                std.label = FatumLib.fromSafeString(Request.Query.getElement("Label"));
                                                std.category = FatumLib.fromSafeString(Request.Query.getElement("Category"));
                                                std.startticks = startticks;
                                                std.endticks = endticks;
                                                std.currentDB = currentDB;
                                                std.documents = documents;
                                                std.searchCount = searchCount;
                                                string regexcheck = Request.Query.getElement("Regex");
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
                Request.Result.addNode(documents, "Documents");
            }
            else
            {
                Tree documents = new Tree();
                Request.Result.addNode(documents, "Documents");
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
                                    string value = FatumLib.fromSafeString(terms.getElement(name));
                                    string tmp = "";

                                    if (std.regex)
                                    {
                                        tmp = " and [document] REGEXP @term " + i.ToString();
                                        parms.addElement("@term" + i.ToString(), value);
                                    }
                                    else
                                    {
                                        tmp = " and [document] like @term " + i.ToString();
                                        parms.addElement("@term" + i.ToString(), "%" + value + "%");
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
                            parms.addElement("@label", tmp);
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
                            parms.addElement("@category", tmp);
                            if (query != "")
                            {
                                query += " and " + tmp;
                            }
                            else
                            {
                                query = " where " + tmp;
                            }
                        }

                        parms.addElement("@startticks", std.startticks);
                        parms.addElement("@endticks", std.endticks);
                        query += " order by [Received] desc limit " + std.searchCount.ToString() + ";";

                        try
                        {
                            DataTable newTable = std.currentDB.Database.ExecuteDynamic(query, parms);

                            foreach (DataRow currentrow in newTable.Rows)
                            {
                                Tree rowinfo = new Tree();
                                rowinfo.addElement("Received", currentrow["Received"].ToString());
                                rowinfo.addElement("Label", FatumLib.toSafeString(currentrow["Label"].ToString()));
                                rowinfo.addElement("Category", FatumLib.toSafeString(currentrow["Category"].ToString()));
                                rowinfo.addElement("ID", currentrow["ID"].ToString());
                                rowinfo.addElement("FlowID", std.currentDB.FlowID);
                                rowinfo.addElement("Metadata", FatumLib.toSafeString(currentrow["Metadata"].ToString()));
                                rowinfo.addElement("Document", FatumLib.toSafeString(currentrow["Document"].ToString()));
                                std.documents.addNode(rowinfo, std.currentDB.FlowID);
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
