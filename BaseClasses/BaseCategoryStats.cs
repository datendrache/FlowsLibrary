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
                result = FatumLib.UriXmlToTree(URI, TreeDataAccess.WriteTreeToXmlString(Criteria, "Criteria"));
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
