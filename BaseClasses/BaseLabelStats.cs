//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Data;
using System.Collections;
using FatumCore;

namespace PhlozLib
{    
    public class BaseLabelStats
    {
        ArrayList labelList = new ArrayList();

        ~BaseLabelStats()
        {
            if (labelList!=null)
            {
                labelList.Clear();
                labelList = null;
            }
        }

        public void recordDocument(BaseDocument document)
        {
            Boolean recorded = false;

            foreach (BaseLabel current in labelList)
            {
                if (current.Label == document.Label)
                {
                    current.count++;
                    recorded = true;
                    break;
                }
            }

            if (!recorded)
            {
                BaseLabel newLabel = new BaseLabel();

                newLabel.Label = document.Label;
                newLabel.count = 1;
                labelList.Add(newLabel);
            }
        }

        public void update()
        {
            lock (labelList.SyncRoot)
            {
                ArrayList tmp = new ArrayList();

                foreach (BaseLabel current in labelList)
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
                    labelList.Remove(current);
                }
            }
        }


        public DataTable getStats()
        {
            DataTable newTable = new DataTable();

            newTable.Columns.Add("Label Type", typeof(string));
            newTable.Columns.Add("Count", typeof(int));

            foreach (BaseLabel current in labelList)
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

        public Tree getStatsXML()
        {
            Tree stats = new Tree();

            foreach (BaseLabel current in labelList)
            {
                if (current.Label == "")
                {
                    stats.addElement("blank", current.count.ToString());
                }
                else
                {
                    stats.addElement(current.Label, current.count.ToString());
                }
            }

            return stats;
        }
    }
}
