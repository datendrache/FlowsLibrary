using DatabaseAdapters;
using Fatum.FatumCore;
using FatumCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhlozLib
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

            InstanceID = criteria.getElement("InstanceID");
            Template = criteria;
            OwnerID = criteria.getElement("OwnerID");

            if (InstanceID=="All")
            {
                DataTable dt = BaseInstance.getInstanceSearchableList(managementDB);
                foreach (DataRow dr in dt.Rows)
                {
                    string Host = dr["Host"].ToString();
                    string ID = dr["UniqueID"].ToString();
                    BaseQueryHost newQueryHost = new BaseQueryHost(managementDB);
                    Tree searchCriteria = Template.Duplicate();
                    
                    searchCriteria.addElement("ID", ID);
                    searchCriteria.addElement("InstanceHost", Host);
                    searchCriteria.addElement("OwnerID", OwnerID);
                    searchCriteria.addElement("QueryURI", FatumLib.toSafeString(getURI(Host)));
                    searchCriteria.addElement("SearchID", UniqueID);
                    searchCriteria.addElement("Auth", ID);  // Possibly will be replaced with a more advanced system later.
                    //searchCriteria.addNode(Criteria, "Criteria");
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
                        searchCriteria.addElement("ID", ID);
                        searchCriteria.addElement("InstanceHost", Host);
                        searchCriteria.addElement("OwnerID", OwnerID);
                        searchCriteria.addElement("QueryURI", FatumLib.toSafeString(getURI(Host)));
                        searchCriteria.addElement("SearchID", UniqueID);
                        searchCriteria.addElement("Auth", ID);  // Possibly will be replaced with a more advanced system later.
                        //searchCriteria.addNode(Criteria, "Criteria");
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
                        searchCriteria.addElement("ID", InstanceID);
                        searchCriteria.addElement("InstanceHost", searchInstance.Host);
                        searchCriteria.addElement("OwnerID", OwnerID);
                        searchCriteria.addElement("QueryURI", FatumLib.toSafeString(getURI(searchInstance.Host)));
                        searchCriteria.addElement("SearchID", UniqueID);
                        searchCriteria.addElement("Auth", searchInstance.Host);  // Possibly will be replaced with a more advanced system later.
                        //searchCriteria.addNode(Criteria, "Criteria");
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
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
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
                data.addElement("Status", search.Status);
                data.addElement("*@UniqueID", search.UniqueID);
                managementDB.UpdateTree("[Searches]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree data = new Tree();
                search.DateAdded = DateTime.Now;
                data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.addElement("_DateAdded", "BIGINT");
                data.addElement("InstanceID", search.InstanceID);
                if (search.Criteria == null)
                {
                    search.Criteria = new Tree();
                }
                data.addElement("Criteria", FatumLib.Scramble(TreeDataAccess.writeTreeToXMLString(search.Criteria, "BaseSearch"), search.OwnerID));
                data.addElement("Status", "Searching");
                data.addElement("OwnerID", search.OwnerID);
                if (search.UniqueID == "")
                {
                    search.UniqueID = "W" + System.Guid.NewGuid().ToString().Replace("-", "");
                }
                data.addElement("UniqueID", search.UniqueID);
                managementDB.InsertTree("[Searches]", data);
                data.dispose();
            }
        }

        public static BaseSearch loadSearchesByUniqueID(IntDatabase managementDB, string searchID)
        {
            BaseSearch search = new BaseSearch();
            string SQL = "select * from [Searches] where [uniqueid]=@uniqueid;";
            Tree parameters = new Tree();
            parameters.addElement("@uniqueid", searchID);
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
                    search.Criteria = XMLTree.readXMLFromString(FatumLib.Unscramble(dr["Criteria"].ToString(), search.OwnerID));
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
            data.addElement("@searchuid", searchID);
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
            return Comparer.Default.Compare(right.getElement("Received"), left.getElement("Received"));
        }
    }
}
