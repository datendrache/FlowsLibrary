using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FatumCore;

namespace PhlozLib
{
    public class RenderItem
    {
        public Tree currentDocument = null;
        public DataTable mimes = null;

        public long received = 0;
        public string flowid = "";
        public string label = "";
        public string category = "";
        public string document = "";
        public string metadata = "";
        public string body = "";

        ~RenderItem()
        {
            currentDocument = null;
        }
    }
}
