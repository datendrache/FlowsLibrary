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
    public class BaseSearch
    {
        public DateTime DateAdded = DateTime.MinValue;
        public string InstanceID = "";
        public ArrayList InstanceQueries = new ArrayList();
        public Tree Criteria = null;
        public Tree Template = null;
        public string OwnerID = "";
        public string UniqueID = "";
        public string Status = "";

        public void Search(IntDatabase managementDB, Tree criteria)
        {
            InstanceQueries.Clear();

            InstanceID = criteria.GetElement("InstanceID");
            Template = criteria;
            OwnerID = criteria.GetElement("OwnerID");

            if (InstanceID=="All")
            {
                DataTable dt = BaseInstance.getInstanceSearchableList(managementDB);
                foreach (DataRow dr in dt.Rows)
                {
                    string Host = dr["Host"].ToString();
                    string ID = dr["UniqueID"].ToString();
                    BaseQueryHost newQueryHost = new BaseQueryHost(managementDB);
                    Tree searchCriteria = Template.Duplicate();
                    
                    searchCriteria.AddElement("ID", ID);
                    searchCriteria.AddElement("InstanceHost", Host);
                    searchCriteria.AddElement("OwnerID", OwnerID);
                    searchCriteria.AddElement("QueryURI", FatumLib.ToSafeString(getURI(Host)));
                    searchCriteria.AddElement("SearchID", UniqueID);
                    searchCriteria.AddElement("Auth", ID);  // Possibly will be replaced with a more advanced system later.
                    //searchCriteria.AddNode(Criteria, "Criteria");
                    newQueryHost.Criteria = searchCriteria;
                    newQueryHost.OwnerID = OwnerID;
                    newQueryHost.InstanceID = InstanceID;
                    newQueryHost.SearchID = UniqueID;
                    InstanceQueries.Add(newQueryHost);
                }
            }
            else
            {
                if (InstanceID.Substring(0, 1) == "G")
                {  // Searching a defined group of systems
                    DataTable dt = BaseInstance.getInstanceSearchableListByGroup(managementDB, InstanceID);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string Host = dr["Host"].ToString();
                        string ID = dr["UniqueID"].ToString();
                        BaseQueryHost newQueryHost = new BaseQueryHost(managementDB);
 
                        Tree searchCriteria = Template.Duplicate();
                        searchCriteria.AddElement("ID", ID);
                        searchCriteria.AddElement("InstanceHost", Host);
                        searchCriteria.AddElement("OwnerID", OwnerID);
                        searchCriteria.AddElement("QueryURI", FatumLib.ToSafeString(getURI(Host)));
                        searchCriteria.AddElement("SearchID", UniqueID);
                        searchCriteria.AddElement("Auth", ID);  // Possibly will be replaced with a more advanced system later.
                        //searchCriteria.AddNode(Criteria, "Criteria");
                        newQueryHost.Criteria = searchCriteria;
                        newQueryHost.OwnerID = OwnerID;
                        newQueryHost.InstanceID = InstanceID;
                        newQueryHost.SearchID = UniqueID;
                        InstanceQueries.Add(newQueryHost);
                    }
                }
                else
                {
                    if (InstanceID.Substring(0,1)=="I")
                    {
                        BaseQueryHost newQueryHost = new BaseQueryHost(managementDB);
                        BaseInstance searchInstance = BaseInstance.loadInstanceByUniqueID(managementDB, InstanceID);

                        Tree searchCriteria = Template.Duplicate();
                        searchCriteria.AddElement("ID", InstanceID);
                        searchCriteria.AddElement("InstanceHost", searchInstance.Host);
                        searchCriteria.AddElement("OwnerID", OwnerID);
                        searchCriteria.AddElement("QueryURI", FatumLib.ToSafeString(getURI(searchInstance.Host)));
                        searchCriteria.AddElement("SearchID", UniqueID);
                        searchCriteria.AddElement("Auth", searchInstance.Host);  // Possibly will be replaced with a more advanced system later.
                        //searchCriteria.AddNode(Criteria, "Criteria");
                        newQueryHost.Criteria = searchCriteria;
                        newQueryHost.OwnerID = OwnerID;
                        newQueryHost.InstanceID = InstanceID;
                        newQueryHost.SearchID = UniqueID;
                        InstanceQueries.Add(newQueryHost);
                    }
                }
            }

            foreach (BaseQueryHost current in InstanceQueries)
            {
                current.PerformQuery();
            }
        }

        private string getURI(string qhHost)
        {
            string queryString = "Search/";
            return ("https://" + qhHost + ":7777/" + queryString);
        }

        static public void removeSearchesByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Searches] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        public static void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Searches] ( " +
                        "[DateAdded] INTEGER NULL, " +
                        "[InstanceID] TEXT NULL, " +
                        "[Criteria] TEXT NULL, " +
                        "[Status] TEXT NULL, " +
                        "[OwnerID] TEXT NULL, " +
                        "[UniqueID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Searches] (" +
                        "[DateAdded] BIGINT NULL, " +
                        "[InstanceID] VARCHAR(33) NULL, " +
                        "[Criteria] NVARCHAR(MAX) NULL, " +
                        "[Status] VARCHAR(40) NULL, " +
                        "[OwnerID] VARCHAR(33) NULL, " +
                        "[UniqueID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);
        }

        static public void updateSearch(IntDatabase managementDB, BaseSearch search)
        {
            if (search.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("Status", search.Status);
                data.AddElement("*@UniqueID", search.UniqueID);
                managementDB.UpdateTree("[Searches]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree data = new Tree();
                search.DateAdded = DateTime.Now;
                data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.AddElement("_DateAdded", "BIGINT");
                data.AddElement("InstanceID", search.InstanceID);
                if (search.Criteria == null)
                {
                    search.Criteria = new Tree();
                }
                data.AddElement("Criteria", FatumLib.Scramble(TreeDataAccess.WriteTreeToXmlString(search.Criteria, "BaseSearch"), search.OwnerID));
                data.AddElement("Status", "Searching");
                data.AddElement("OwnerID", search.OwnerID);
                if (search.UniqueID == "")
                {
                    search.UniqueID = "W" + System.Guid.NewGuid().ToString().Replace("-", "");
                }
                data.AddElement("UniqueID", search.UniqueID);
                managementDB.InsertTree("[Searches]", data);
                data.Dispose();
            }
        }

        public static BaseSearch loadSearchesByUniqueID(IntDatabase managementDB, string searchID)
        {
            BaseSearch search = new BaseSearch();
            string SQL = "select * from [Searches] where [uniqueid]=@uniqueid;";
            Tree parameters = new Tree();
            parameters.AddElement("@uniqueid", searchID);
            DataTable dt = managementDB.ExecuteDynamic(SQL, parameters);
            DataRow dr = null;

            if (dt.Rows.Count > 0)
            {
                dr = dt.Rows[0];
            }

            if (dr != null)
            {
                search.DateAdded = new DateTime(Convert.ToInt64(dr["DateAdded"]));
                search.InstanceID = dr["InstanceID"].ToString();
                search.OwnerID = dr["OwnerID"].ToString();

                try
                {
                    search.Criteria = XMLTree.ReadXmlFromString(FatumLib.Unscramble(dr["Criteria"].ToString(), search.OwnerID));
                }
                catch (Exception xyz)
                {
                    search.Criteria = new Tree();
                }
                
                search.Status = dr["Status"].ToString();
                search.OwnerID = dr["OwnerID"].ToString();
                search.UniqueID = dr["UniqueID"].ToString();
                return search;
            }
            else
            {
                return null;
            }
        }

        public static DataTable getQueryResults(IntDatabase managementDB, string searchID)
        {
            string SQL = "select qh.* from Searches as search join queryhosts as qh on search.uniqueid = qh.SearchID where search.UniqueID=@searchuid and qh.Status='Success';";
            Tree data = new Tree();
            data.AddElement("@searchuid", searchID);
            return managementDB.ExecuteDynamic(SQL, data);
        }

        public static void redux(Tree documents, int maxresults)
        {
            documents.tree.Sort(new MessageSort());
            if (documents.tree.Count > maxresults)
            {
                documents.tree.RemoveRange(maxresults, (documents.tree.Count - maxresults));
                documents.leafnames.RemoveRange(maxresults, (documents.leafnames.Count - maxresults));
            }
        }
    }

    public class MessageSort : IComparer<Tree>
    {
        public int Compare(Tree x, Tree y)
        {
            Tree left = (Tree)x;
            Tree right = (Tree)y;

            // reverse the arguments
            return Comparer.Default.Compare(right.GetElement("Received"), left.GetElement("Received"));
        }
    }
}
