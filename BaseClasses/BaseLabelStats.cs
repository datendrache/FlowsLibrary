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

using System.Data;
using System.Collections;
using Proliferation.Fatum;

namespace Proliferation.Flows
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
                    stats.AddElement("blank", current.count.ToString());
                }
                else
                {
                    stats.AddElement(current.Label, current.count.ToString());
                }
            }

            return stats;
        }
    }
}
