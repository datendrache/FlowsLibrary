//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Data;
using System.Collections;
using FatumCore;

namespace PhlozLib
{    
    public class BaseCategoryStats
    {
        ArrayList categoryList = new ArrayList();

        ~BaseCategoryStats()
        {
            if (categoryList!=null)
            {
                categoryList.Clear();
                categoryList = null;
            }
        }

        public void recordDocument(BaseDocument document)
        {
            Boolean recorded = false;

            foreach (BaseLabel current in categoryList)
            {
                if (current.Label == document.Category)
                {
                    current.count++;
                    recorded = true;
                    break;
                }
            }

            if (!recorded)
            {
                BaseLabel newLabel = new BaseLabel();

                newLabel.Label = document.Category;
                newLabel.count = 1;
                categoryList.Add(newLabel);
            }
        }

        public void update()
        {
            lock (categoryList.SyncRoot)
            {
                ArrayList tmp = new ArrayList();

                foreach (BaseLabel current in categoryList)
                {
                    if (current.count == 0)
                    {
                        tmp.Add(current);       
                    }
                    else
                    {
                        current.count = 0;
                    }
                }

                foreach (BaseLabel current in tmp)
                {
                    categoryList.Remove(current);
                }
            }
        }


        public DataTable getStats()
        {
            DataTable newTable = new DataTable();

            newTable.Columns.Add("Category Type", typeof(string));
            newTable.Columns.Add("Count", typeof(int));

            foreach (BaseLabel current in categoryList)
            {
                if (current.Label == "")
                {
                    newTable.Rows.Add("<blank>", current.count);
                }
                else
                {
                    newTable.Rows.Add(current.Label, current.count);
                }
            }

            return newTable;
        }

        public static Tree getStatsFromInstance(string URI)
        {
            Tree result = null;
            try
            {
                Tree Criteria = new Tree();
                result = FatumLib.URIXMLtoTree(URI, TreeDataAccess.writeTreeToXMLString(Criteria, "Criteria"));
            }
            catch (Exception xyz)
            {
                int abc = 1;
                // Status = "Aborted. Message: " + xyz.Message;
            }
            return result;
        }
    }
}
