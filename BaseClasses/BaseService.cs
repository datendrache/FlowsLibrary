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

using System.Collections;
using System.Data;
using Proliferation.Fatum;
using DatabaseAdapters;

namespace Proliferation.Flows
{
    public class BaseService
    {
        public string DateAdded = "";
        public string ServiceName = "";
        public string ServiceType = "";
        public string ServiceSubtype = "";
        public string CredentialID = "";
        public string ParameterID = "";
        public string OwnerID = "";
        public string UniqueID = "";
        public string Description = "";
        public string SourceID = "";
        public string GroupID = "";
        public string Origin = "";
        public Boolean Enabled = false;
        public string DefaultRuleGroup = "";

        public BaseSource ParentSource;
        public BaseCredential Credentials;
        public BaseParameter Parameter;

        public ArrayList Flows = new ArrayList();
        public ArrayList Receivers = new ArrayList();

        public BaseService()
        {

        }

        ~BaseService()
        {
            DateAdded = null;
            ServiceName = null;
            ServiceType = null;
            ServiceSubtype = null;
            CredentialID = null;
            ParameterID = null;
            OwnerID = null;
            UniqueID = null;
            Description = null;
            SourceID = null;
            GroupID = null;
            ParentSource = null;
            Credentials = null;
            Parameter = null;
            Origin = null;
            DefaultRuleGroup = null;
        }

        static public ArrayList loadServices(IntDatabase managementDB, ArrayList sourceList)
        {
            ArrayList tmpServices = new ArrayList();

            foreach (BaseSource currentSource in sourceList)
            {
                String squery = "select * from [Services] where [SourceID]='" + currentSource.UniqueID + "';";
                DataTable services = managementDB.Execute(squery);

                foreach (DataRow row in services.Rows)
                {
                    BaseService newService = new BaseService();
                    newService.DateAdded = row["DateAdded"].ToString();
                    newService.ServiceName = row["ServiceName"].ToString();
                    newService.ServiceType = row["ServiceType"].ToString();
                    newService.ServiceSubtype = row["ServiceSubtype"].ToString();
                    newService.Description = row["Description"].ToString();
                    newService.CredentialID = row["CredentialID"].ToString();
                    newService.ParameterID = row["ParameterID"].ToString();
                    newService.UniqueID = row["UniqueID"].ToString();
                    newService.OwnerID = row["OwnerID"].ToString();
                    newService.GroupID = row["GroupID"].ToString();
                    newService.SourceID = row["SourceID"].ToString();
                    newService.Origin = row["Origin"].ToString();
                    newService.DefaultRuleGroup = row["DefaultRuleGroup"].ToString();
                    if (row["Enabled"].ToString().ToLower() == "true")
                    {
                        newService.Enabled = true;
                    }
                    else
                    {
                        newService.Enabled = false;
                    }
                    newService.ParentSource = currentSource;

                    if (newService.CredentialID.Length > 5)
                    {
                        newService.Credentials = BaseCredential.loadCredentialByUniqueID(managementDB, newService.CredentialID);
                    }
                    else
                    {
                        newService.Credentials = null;
                    }

                    if (newService.ParameterID.Length > 5)
                    {
                        newService.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newService.ParameterID);
                    }
                    else
                    {
                        newService.Parameter = null;
                    }
                    tmpServices.Add(newService);
                }
            }
            return tmpServices;
        }

        static public DataTable getServices(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Services];";
            services = managementDB.Execute(squery);
            return services;
        }

        static public DataTable getServicesBySource(IntDatabase managementDB, string sourceid)
        {
            String squery = "select * from [Services] where [SourceID]=@sourceid;";
            Tree data = new Tree();
            data.AddElement("@sourceid", sourceid);
            DataTable services = managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
            return services;
        }

        static public void updateService(IntDatabase managementDB, BaseService service)
        {
            if (service.UniqueID != "")
            {
                Tree data = new Tree();
                data.AddElement("CredentialID", service.CredentialID);
                data.AddElement("ParameterID", service.ParameterID);
                data.AddElement("ServiceName", service.ServiceName);
                data.AddElement("OwnerID", service.OwnerID);
                data.AddElement("GroupID", service.GroupID);
                data.AddElement("Description", service.Description);
                data.AddElement("SourceID", service.SourceID);
                data.AddElement("Origin", service.Origin);
                data.AddElement("Enabled", service.Enabled.ToString());
                data.AddElement("DefaultRuleGroup", service.DefaultRuleGroup);
                data.AddElement("*@UniqueID", service.UniqueID);
                managementDB.UpdateTree("[Services]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree data = new Tree();
                data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.AddElement("_DateAdded", "BIGINT");
                data.AddElement("ServiceType", service.ServiceType);
                data.AddElement("ServiceName", service.ServiceName);
                data.AddElement("ServiceSubtype", service.ServiceSubtype);
                service.UniqueID = "S" + System.Guid.NewGuid().ToString().Replace("-", "");
                data.AddElement("CredentialID", service.CredentialID);
                data.AddElement("ParameterID", service.ParameterID);
                data.AddElement("UniqueID", service.UniqueID);
                data.AddElement("OwnerID", service.OwnerID);
                data.AddElement("GroupID", service.GroupID);
                data.AddElement("Description", service.Description);
                data.AddElement("SourceID", service.SourceID);
                data.AddElement("Origin", service.Origin);
                data.AddElement("Enabled", service.Enabled.ToString());
                data.AddElement("DefaultRuleGroup", service.DefaultRuleGroup);
                managementDB.InsertTree("[Services]", data);
                data.Dispose();
            }
        }

        static public void removeServiceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Services] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public void addService(IntDatabase managementDB, Tree description)
        {
            Tree data = new Tree();
            data.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
            data.AddElement("_DateAdded", "BIGINT");
            data.AddElement("ServiceType", description.GetElement("ServiceType"));
            data.AddElement("ServiceName", description.GetElement("ServiceName"));
            data.AddElement("ServiceSubtype", description.GetElement("ServiceSubtype"));
            data.AddElement("CredentialID", description.GetElement("CredentialID"));
            data.AddElement("ParameterID", description.GetElement("ParameterID"));
            data.AddElement("UniqueID", description.GetElement("UniqueID"));
            data.AddElement("OwnerID", description.GetElement("OwnerID"));
            data.AddElement("GroupID", description.GetElement("GroupID"));
            data.AddElement("Description", description.GetElement("Description"));
            data.AddElement("SourceID", description.GetElement("SourceID"));
            data.AddElement("Origin", description.GetElement("Origin"));
            data.AddElement("DefaultRuleGroup", description.GetElement("DefaultRuleGroup"));
            data.AddElement("Enabled", description.GetElement("Enabled"));
            managementDB.InsertTree("[Services]", data);
            data.Dispose();
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Services](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[ServiceName] TEXT NULL, " +
                    "[ServiceType] TEXT NULL, " +
                    "[ServiceSubtype] TEXT NULL, " +
                    "[CredentialID] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[SourceID] TEXT NULL, " +
                    "[Origin] TEXT NULL, " +
                    "[Enabled] TEXT NULL, " +
                    "[DefaultRuleGroup] TEXT NULL, " +
                    "[Description] TEXT NULL );";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Services](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[ServiceName] NVARCHAR(100) NULL, " +
                    "[ServiceType] NVARCHAR(100) NULL, " +
                    "[ServiceSubtype] NVARCHAR(100) NULL, " +
                    "[CredentialID] VARCHAR(33) NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[SourceID] VARCHAR(33) NULL, " +
                    "[Origin] VARCHAR(33) NULL, " +
                    "[Enabled] VARCHAR(6) NULL, " +
                    "[DefaultRuleGroup] VARCHAR(33) NULL, " +
                    "[Description] NVARCHAR(MAX) NULL );";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_baseservices ON Services([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baseservicessource ON Services([SourceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_baseservices ON Services([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_baseservicessource ON Services([SourceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public string getXML(BaseService current)
        {
            string result = "";
            Tree tmp = getTree(current);
            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseService");
            tmp.Dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseService current)
        {
            Tree tmp = new Tree();
            tmp.AddElement("DateAdded", current.DateAdded);
            tmp.AddElement("ServiceName", current.ServiceName);
            tmp.AddElement("ServiceType", current.ServiceType);
            tmp.AddElement("ServiceSubtype", current.ServiceSubtype);
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("CredentialID", current.CredentialID);
            tmp.AddElement("ParameterID", current.CredentialID);
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("SourceID", current.SourceID);
            tmp.AddElement("Origin", current.Origin);
            tmp.AddElement("DefaultRuleGroup", current.DefaultRuleGroup);
            tmp.AddElement("Enabled", current.Enabled.ToString());
            return tmp;
        }

        static public BaseService loadServiceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable processors;
            BaseService result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Services] where [UniqueID]=@uid limit 1;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select TOP (1) * from [Services] where [UniqueID]=@uid;";
                    break;
            }

            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseService newService = new BaseService();
                newService.DateAdded = row["DateAdded"].ToString();
                newService.ServiceName = row["ServiceName"].ToString();
                newService.ServiceType = row["ServiceType"].ToString();
                newService.ServiceSubtype = row["ServiceSubtype"].ToString();
                newService.CredentialID = row["CredentialID"].ToString();
                newService.ParameterID = row["ParameterID"].ToString();
                newService.UniqueID = row["UniqueID"].ToString();
                newService.OwnerID = row["OwnerID"].ToString();
                newService.GroupID = row["GroupID"].ToString();
                newService.SourceID = row["SourceID"].ToString();
                newService.Origin = row["Origin"].ToString();
                newService.Description = row["Description"].ToString();
                newService.DefaultRuleGroup = row["DefaultRuleGroup"].ToString();
                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newService.Enabled = true;
                }
                else
                {
                    newService.Enabled = false;
                }
                if (newService.ParameterID.Length > 5)
                {
                    newService.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newService.ParameterID);
                }
                else
                {
                    newService.Parameter = null;
                }
                result = newService;
            }
            return result;
        }

        static public BaseService loadServiceByName(IntDatabase managementDB, string servicename)
        {
            DataTable processors;
            BaseService result = null;

            String query = "";
            switch (managementDB.getDatabaseType())
            {
                case DatabaseSoftware.SQLite:
                    query = "select * from [Services] where [ServiceName]=@servicename limit 1;";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    query = "select TOP (1) * from [Services] where [ServiceName]=@servicename;";
                    break;
            }

            Tree parms = new Tree();
            parms.AddElement("@servicename", servicename);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

            foreach (DataRow row in processors.Rows)
            {
                BaseService newService = new BaseService();
                newService.DateAdded = row["DateAdded"].ToString();
                newService.ServiceName = row["ServiceName"].ToString();
                newService.ServiceType = row["ServiceType"].ToString();
                newService.ServiceSubtype = row["ServiceSubtype"].ToString();
                newService.CredentialID = row["CredentialID"].ToString();
                newService.ParameterID = row["ParameterID"].ToString();
                newService.UniqueID = row["UniqueID"].ToString();
                newService.OwnerID = row["OwnerID"].ToString();
                newService.GroupID = row["GroupID"].ToString();
                newService.SourceID = row["SourceID"].ToString();
                newService.Origin = row["Origin"].ToString();
                newService.Description = row["Description"].ToString();
                newService.DefaultRuleGroup = row["DefaultRuleGroup"].ToString();
                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newService.Enabled = true;
                }
                else
                {
                    newService.Enabled = false;
                }
                if (newService.ParameterID.Length > 5)
                {
                    newService.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newService.ParameterID);
                }
                else
                {
                    newService.Parameter = null;
                }
                result = newService;
            }
            return result;
        }

        public BaseService(Tree XML)
        {
            ServiceName = XML.GetElement("ServiceName");
            ServiceType = XML.GetElement("ServiceType");
            ServiceSubtype = XML.GetElement("ServiceSubtype");
            CredentialID = XML.GetElement("CredentialID");
            ParameterID = XML.GetElement("ParameterID");
            Description = XML.GetElement("Description");
            Origin = XML.GetElement("Origin");
            UniqueID = XML.GetElement("UniqueID");
            GroupID = XML.GetElement("GroupID");
            OwnerID = XML.GetElement("OwnerID");
            OwnerID = XML.GetElement("SourceID");
            DateAdded = XML.GetElement("DateAdded");
            DefaultRuleGroup = XML.GetElement("DefaultRuleGroup");

            if (XML.GetElement("Enabled").ToLower() == "true")
            {
                Enabled = true;
            }
            else
            {
                Enabled = false;
            }
        }

        static public ArrayList loadServicesBySource(IntDatabase managementDB, BaseSource Source)
        {
            ArrayList tmpServices = new ArrayList();

            String squery = "select * from [Services] where [SourceID]='" + Source.UniqueID + "';";
            DataTable services = managementDB.Execute(squery);

            foreach (DataRow row in services.Rows)
            {
                BaseService newService = new BaseService();
                newService.DateAdded = row["DateAdded"].ToString();
                newService.ServiceName = row["ServiceName"].ToString();
                newService.ServiceType = row["ServiceType"].ToString();
                newService.ServiceSubtype = row["ServiceSubtype"].ToString();
                newService.Description = row["Description"].ToString();
                newService.CredentialID = row["CredentialID"].ToString();
                newService.ParameterID = row["ParameterID"].ToString();
                newService.UniqueID = row["UniqueID"].ToString();
                newService.OwnerID = row["OwnerID"].ToString();
                newService.GroupID = row["GroupID"].ToString();
                newService.SourceID = row["SourceID"].ToString();
                newService.Origin = row["Origin"].ToString();
                newService.DefaultRuleGroup = row["DefaultRuleGroup"].ToString();
                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newService.Enabled = true;
                }
                else
                {
                    newService.Enabled = false;
                }
                newService.ParentSource = Source;

                if (newService.CredentialID.Length > 5)
                {
                    newService.Credentials = BaseCredential.loadCredentialByUniqueID(managementDB, newService.CredentialID);
                }
                else
                {
                    newService.Credentials = null;
                }

                if (newService.ParameterID.Length > 5)
                {
                    newService.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newService.ParameterID);
                }
                else
                {
                    newService.Parameter = null;
                }
                tmpServices.Add(newService);
            }
            return tmpServices;
        }

        static public void updateServicesBySource(CollectionState state, BaseSource Source)
        {
            ArrayList currentServices = new ArrayList();

            IntDatabase managementDB = state.managementDB;
            foreach (BaseService currentService in Source.Services)
            {
                currentServices.Add(currentService);    
            }

            String squery = "select * from [Services] where [SourceID]='" + Source.UniqueID + "';";
            DataTable services = managementDB.Execute(squery);

            foreach (DataRow row in services.Rows)
            {
                string serviceCheck = row["UniqueID"].ToString();
                Boolean isServiceActive = false;
                int servicePosition = 0;
                foreach (BaseService currentService in currentServices)
                {
                    if (currentService.UniqueID == serviceCheck)
                    {
                        isServiceActive = true;
                        break;
                    }
                    servicePosition++;
                }
                
                if (!isServiceActive)
                {
                    BaseService newService = new BaseService();
                    newService.DateAdded = row["DateAdded"].ToString();
                    newService.ServiceName = row["ServiceName"].ToString();
                    newService.ServiceType = row["ServiceType"].ToString();
                    newService.ServiceSubtype = row["ServiceSubtype"].ToString();
                    newService.Description = row["Description"].ToString();
                    newService.CredentialID = row["CredentialID"].ToString();
                    newService.ParameterID = row["ParameterID"].ToString();
                    newService.UniqueID = row["UniqueID"].ToString();
                    newService.OwnerID = row["OwnerID"].ToString();
                    newService.GroupID = row["GroupID"].ToString();
                    newService.SourceID = row["SourceID"].ToString();
                    newService.Origin = row["Origin"].ToString();
                    newService.DefaultRuleGroup = row["DefaultRuleGroup"].ToString();
                    if (row["Enabled"].ToString().ToLower() == "true")
                    {
                        newService.Enabled = true;
                    }
                    else
                    {
                        newService.Enabled = false;
                    }
                    newService.ParentSource = Source;

                    if (newService.CredentialID.Length > 5)
                    {
                        newService.Credentials = BaseCredential.loadCredentialByUniqueID(managementDB, newService.CredentialID);
                    }
                    else
                    {
                        newService.Credentials = null;
                    }

                    if (newService.ParameterID.Length > 5)
                    {
                        newService.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newService.ParameterID);
                    }
                    else
                    {
                        newService.Parameter = null;
                    }
                    Source.Services.Add(newService);

                    // CREATE RECEIVER AND ADD FEEDS TO IT!!!!!

                    // DOOOO THISSSS!!!!!!
                }
                else
                {
                    currentServices.RemoveAt(servicePosition);
                }
            }

            foreach (BaseService removedService in currentServices)
            {
                int position = 0;
                Boolean found = false;

                foreach (BaseService currentService in Source.Services)
                {
                    if (currentService.UniqueID==removedService.UniqueID)
                    {
                        // Okay, we found a service that was deleted, so we need to shut it down.

                        foreach (ReceiverInterface currentReceiver in currentService.Receivers)
                        {
                            currentReceiver.StartSuspend();
                            foreach (BaseFlow currentFlow in removedService.Flows)
                            {
                                currentReceiver.deregisterFlow(currentFlow);
                                currentFlow.Suspend();
                                currentFlow.documentDB.Close();
                                currentFlow.indexer.Close();
                            }
                            currentReceiver.Stop();
                        }
                    }

                    removedService.Flows.Clear();
                    position++;
                }

                if (found)
                {
                    Source.Services.RemoveAt(position);
                }
            }

            currentServices.Clear();
        }

        public void reloadFlow(CollectionState State, BaseFlow Flow)
        {
            foreach (BaseFlow currentFlow in Flows)
            {
                if (currentFlow.UniqueID == Flow.UniqueID)
                {
                    FlowReference newReference = new FlowReference(currentFlow, State, State.MasterReceiver.onDocumentReceived);
                    currentFlow.flowReference = newReference;

                    foreach (ReceiverInterface receiver in Receivers)
                    {

                    }
                }
            }
        }

        static public DataTable loadFlowsByService(IntDatabase managementDB, string ServiceID)
        {
            DataTable flows;
            String squery = "select f.* from Services as Serv join Flows as f on f.serviceid=serv.uniqueid where serv.Uniqueid=@ServiceID;";
            Tree data = new Tree();
            data.AddElement("@ServiceID", ServiceID);
            flows = managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
            return flows;
        }
    }
}
