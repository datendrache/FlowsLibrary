//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using FatumCore;
using DatabaseAdapters;

namespace PhlozLib
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
            data.addElement("@sourceid", sourceid);
            DataTable services = managementDB.ExecuteDynamic(squery, data);
            data.dispose();
            return services;
        }

        static public void updateService(IntDatabase managementDB, BaseService service)
        {
            if (service.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("CredentialID", service.CredentialID);
                data.addElement("ParameterID", service.ParameterID);
                data.addElement("ServiceName", service.ServiceName);
                data.addElement("OwnerID", service.OwnerID);
                data.addElement("GroupID", service.GroupID);
                data.addElement("Description", service.Description);
                data.addElement("SourceID", service.SourceID);
                data.addElement("Origin", service.Origin);
                data.addElement("Enabled", service.Enabled.ToString());
                data.addElement("DefaultRuleGroup", service.DefaultRuleGroup);
                data.addElement("*@UniqueID", service.UniqueID);
                managementDB.UpdateTree("[Services]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree data = new Tree();
                data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                data.addElement("_DateAdded", "BIGINT");
                data.addElement("ServiceType", service.ServiceType);
                data.addElement("ServiceName", service.ServiceName);
                data.addElement("ServiceSubtype", service.ServiceSubtype);
                service.UniqueID = "S" + System.Guid.NewGuid().ToString().Replace("-", "");
                data.addElement("CredentialID", service.CredentialID);
                data.addElement("ParameterID", service.ParameterID);
                data.addElement("UniqueID", service.UniqueID);
                data.addElement("OwnerID", service.OwnerID);
                data.addElement("GroupID", service.GroupID);
                data.addElement("Description", service.Description);
                data.addElement("SourceID", service.SourceID);
                data.addElement("Origin", service.Origin);
                data.addElement("Enabled", service.Enabled.ToString());
                data.addElement("DefaultRuleGroup", service.DefaultRuleGroup);
                managementDB.InsertTree("[Services]", data);
                data.dispose();
            }
        }

        static public void removeServiceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Services] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void addService(IntDatabase managementDB, Tree description)
        {
            Tree data = new Tree();
            data.addElement("DateAdded", DateTime.Now.Ticks.ToString());
            data.addElement("_DateAdded", "BIGINT");
            data.addElement("ServiceType", description.getElement("ServiceType"));
            data.addElement("ServiceName", description.getElement("ServiceName"));
            data.addElement("ServiceSubtype", description.getElement("ServiceSubtype"));
            data.addElement("CredentialID", description.getElement("CredentialID"));
            data.addElement("ParameterID", description.getElement("ParameterID"));
            data.addElement("UniqueID", description.getElement("UniqueID"));
            data.addElement("OwnerID", description.getElement("OwnerID"));
            data.addElement("GroupID", description.getElement("GroupID"));
            data.addElement("Description", description.getElement("Description"));
            data.addElement("SourceID", description.getElement("SourceID"));
            data.addElement("Origin", description.getElement("Origin"));
            data.addElement("DefaultRuleGroup", description.getElement("DefaultRuleGroup"));
            data.addElement("Enabled", description.getElement("Enabled"));
            managementDB.InsertTree("[Services]", data);
            data.dispose();
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
            TreeDataAccess.writeXML(outs, tmp, "BaseService");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public Tree getTree(BaseService current)
        {
            Tree tmp = new Tree();
            tmp.addElement("DateAdded", current.DateAdded);
            tmp.addElement("ServiceName", current.ServiceName);
            tmp.addElement("ServiceType", current.ServiceType);
            tmp.addElement("ServiceSubtype", current.ServiceSubtype);
            tmp.addElement("Description", current.Description);
            tmp.addElement("CredentialID", current.CredentialID);
            tmp.addElement("ParameterID", current.CredentialID);
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("SourceID", current.SourceID);
            tmp.addElement("Origin", current.Origin);
            tmp.addElement("DefaultRuleGroup", current.DefaultRuleGroup);
            tmp.addElement("Enabled", current.Enabled.ToString());
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
            parms.addElement("@uid", uniqueid);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
            parms.addElement("@servicename", servicename);
            processors = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

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
            ServiceName = XML.getElement("ServiceName");
            ServiceType = XML.getElement("ServiceType");
            ServiceSubtype = XML.getElement("ServiceSubtype");
            CredentialID = XML.getElement("CredentialID");
            ParameterID = XML.getElement("ParameterID");
            Description = XML.getElement("Description");
            Origin = XML.getElement("Origin");
            UniqueID = XML.getElement("UniqueID");
            GroupID = XML.getElement("GroupID");
            OwnerID = XML.getElement("OwnerID");
            OwnerID = XML.getElement("SourceID");
            DateAdded = XML.getElement("DateAdded");
            DefaultRuleGroup = XML.getElement("DefaultRuleGroup");

            if (XML.getElement("Enabled").ToLower() == "true")
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
            data.addElement("@ServiceID", ServiceID);
            flows = managementDB.ExecuteDynamic(squery, data);
            data.dispose();
            return flows;
        }
    }
}
