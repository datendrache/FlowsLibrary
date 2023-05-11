using DatabaseAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatumCore;

namespace PhlozLib.SearchCore
{
    public class SearchThreadDetails
    {
        public string searchterms = "";
        private string SearchWhereClause = "";
        public string LuceneQuerySyntax = "";
        public Tree Terms = null;
        public string label = "";
        public string category = "";
        public string startticks = "";
        public string endticks = "";
        public long searchCount = 0;
        public BaseFlowDB currentDB = null;
        public Tree documents = null;
        public Boolean regex = false;
        public int maxResults = 500;

        //public string running = "true";

        ~SearchThreadDetails()
        {
            searchterms = null;
            SearchWhereClause = null;
            Terms = null;
            label = null;
            category = null;
            startticks = null;
            endticks = null;
            currentDB = null;
            documents = null;
        }
    }
}
