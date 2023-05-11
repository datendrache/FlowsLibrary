using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data;
using FatumCore;
using DatabaseAdapters;

namespace PhlozLib.SearchCore
{
    public class SearchRequest
    {
        public Tree Query = null;
        public Tree Result = new Tree();

        ~SearchRequest()
        {
            if (Query!=null)
            {
                Query.dispose();
                Query = null;
            }
            
            if (Result !=null)
            {
                Result = null;
            }
        }
    }
}
