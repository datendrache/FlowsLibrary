//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using FatumCore;
using System.IO;
using DatabaseAdapters;
using System.Net;

namespace PhlozLib
{
    public class BaseSource
    {
        public String DateAdded = "";
        public String Category = "";
        public String SourceName = "";
        public String Description = "";
        public String UniqueID = "";
        public Boolean Enabled = false;
        public String OwnerID = "";
        public String GroupID = "";
        public String ParameterID = "";
        public String InstanceID = "";
        public BaseParameter Parameter = null;

        public ArrayList Services = new ArrayList();

        ~BaseSource()
        {
            DateAdded = null;
            Category = null;
            SourceName = null;
            Description = null;
            UniqueID = null;
            OwnerID = null;
            GroupID = null;
            ParameterID = null;
            InstanceID = null;
            if (Parameter != null)
            {
                if (Parameter.ExtractedMetadata!=null)
                {
                    Parameter.ExtractedMetadata.dispose();
                    Parameter.ExtractedMetadata = null;
                    Parameter = null;
                }
            }
        }

        static public ArrayList loadSources(IntDatabase managementDB)
        {
            DataTable sources;
            String squery = "select * from [Sources];";

            sources = managementDB.Execute(squery);

            ArrayList tmpService = new ArrayList();
            foreach (DataRow row in sources.Rows)
            {
                BaseSource newSource = new BaseSource();
                newSource.DateAdded = row["DateAdded"].ToString();
                newSource.SourceName = row["SourceName"].ToString();
                newSource.UniqueID = row["UniqueID"].ToString();
                if (row["Enabled"].ToString().ToLower()=="true")
                {
                    newSource.Enabled = true;
                }
                else
                {
                    newSource.Enabled = false;
                }
                
                newSource.ParameterID = row["ParameterID"].ToString();
                newSource.OwnerID = row["OwnerID"].ToString();
                newSource.GroupID = row["GroupID"].ToString();
                newSource.Description = row["Description"].ToString();
                if (newSource.ParameterID.Length > 5)
                {
                    newSource.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newSource.ParameterID);
                }
                else
                {
                    newSource.Parameter = null;
                }
                tmpService.Add(newSource);
            }
            return tmpService;
        }

        static public DataTable loadFlowsBySource(IntDatabase managementDB, string SourceID)
        {
            DataTable flows;
            String squery = "select f.* from Sources as source join Services as Serv on serv.sourceid=source.uniqueid join Flows as f on f.serviceid=serv.uniqueid where source.Uniqueid=@SourceID;";
            Tree data = new Tree();
            data.addElement("@SourceID", SourceID);

            flows = managementDB.ExecuteDynamic(squery, data);
            data.dispose();
            return flows;
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Sources](" +
                    "[DateAdded] INTEGER NULL, " +
                    "[SourceName] TEXT NULL, " +
                    "[Description] TEXT NULL, " +
                    "[ParameterID] TEXT NULL, " +
                    "[Enabled] TEXT NULL, " +
                    "[OwnerID] TEXT NULL, " +
                    "[GroupID] TEXT NULL, " +
                    "[InstanceID] TEXT NULL, " +
                    "[UniqueID] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Sources](" +
                    "[DateAdded] BIGINT NULL, " +
                    "[SourceName] NVARCHAR(100) NULL, " +
                    "[Description] TEXT NULL, " +
                    "[ParameterID] VARCHAR(33) NULL, " +
                    "[Enabled] VARCHAR(10) NULL, " +
                    "[OwnerID] VARCHAR(33) NULL, " +
                    "[GroupID] VARCHAR(33) NULL, " +
                    "[InstanceID] VARCHAR(33) NULL, " +
                    "[UniqueID] VARCHAR(33) NULL);";
                    break;
            }
            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basesources ON Sources([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basesourcesinstances ON Sources([InstanceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basesources ON Sources([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basesourcesinstances ON Sources([InstanceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void updateSource(IntDatabase managementDB, BaseSource source)
        {
            if (source.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("ParameterID", source.ParameterID);
                data.addElement("Description", source.Description);
                data.addElement("Enabled", source.Enabled.ToString());
                data.addElement("OwnerID", source.OwnerID);
                data.addElement("GroupID", source.GroupID);
                data.addElement("*@UniqueID", source.UniqueID);
                managementDB.UpdateTree("[Sources]", data, "UniqueID=@UniqueID");
                data.dispose();
            }
            else
            {
                Tree parms = new Tree();
                parms.addElement("DateAdded", DateTime.Now.Ticks.ToString());
                parms.addElement("_DateAdded", "BIGINT");
                parms.addElement("SourceName", source.SourceName);
                parms.addElement("ParameterID", source.ParameterID);
                parms.addElement("Description", source.Description);
                source.UniqueID = "Q" + System.Guid.NewGuid().ToString().Replace("-", "");
                parms.addElement("UniqueID", source.UniqueID);
                parms.addElement("Enabled", source.Enabled.ToString());
                parms.addElement("OwnerID", source.OwnerID);
                parms.addElement("GroupID", source.GroupID);
                parms.addElement("InstanceID", source.InstanceID);
                managementDB.InsertTree("[Sources]", parms);
            }
        }

        static public BaseSource loadSourceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable sources;
            BaseSource result = null;
            String query = "select * from [Sources] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.addElement("@uid", uniqueid);
            sources = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in sources.Rows)
            {
                BaseSource newSource = new BaseSource();
                newSource.DateAdded = row["DateAdded"].ToString();
                newSource.SourceName= row["SourceName"].ToString();
                newSource.Description = row["Description"].ToString();
                newSource.ParameterID = row["ParameterID"].ToString();
                newSource.OwnerID = row["OwnerID"].ToString();
                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newSource.Enabled = true;
                }
                else
                {
                    newSource.Enabled = false;
                }
                newSource.UniqueID = row["UniqueID"].ToString();
                newSource.InstanceID = row["InstanceID"].ToString();
                newSource.GroupID = row["GroupID"].ToString();
                if (newSource.ParameterID.Length > 5)
                {
                    newSource.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newSource.ParameterID);
                }
                else
                {
                    newSource.Parameter = null;
                }
                result = newSource;
            }
            return result;
        }

        static public void removeSourcesByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Sources] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public BaseSource loadSourceByName(IntDatabase managementDB, string sourcename)
        {
            DataTable sources;
            BaseSource result = null;

            String query = "select * from [Sources] where [SourceName]=@sourcename;";
            
            Tree parms = new Tree();
            parms.addElement("@sourcename", sourcename);
            sources = managementDB.ExecuteDynamic(query, parms);
            parms.dispose();

            foreach (DataRow row in sources.Rows)
            {
                BaseSource newSource = new BaseSource();
                newSource.DateAdded = row["DateAdded"].ToString();
                newSource.SourceName = row["SourceName"].ToString();
                newSource.Description = row["Description"].ToString();
                newSource.ParameterID = row["ParameterID"].ToString();
                newSource.OwnerID = row["OwnerID"].ToString();
                if (row["Enabled"].ToString().ToLower() == "true")
                {
                    newSource.Enabled = true;
                }
                else
                {
                    newSource.Enabled = false;
                }
                newSource.UniqueID = row["UniqueID"].ToString();
                newSource.InstanceID = row["InstanceID"].ToString();
                newSource.GroupID = row["GroupID"].ToString();
                if (newSource.ParameterID.Length > 5)
                {
                    newSource.Parameter = BaseParameter.loadParameterByUniqueID(managementDB, newSource.ParameterID);
                }
                else
                {
                    newSource.Parameter = null;
                }
                result = newSource;
            }
            return result;
        }

        static public string getXML(BaseSource current)
        {
            string result = "";
            Tree tmp = new Tree();

            tmp.addElement("DateAdded", current.DateAdded.ToString());
            tmp.addElement("Category", current.Category);
            tmp.addElement("SourceName", current.SourceName);
            tmp.addElement("ParameterID", current.ParameterID);
            tmp.addElement("Description", current.Description);
            tmp.addElement("Enabled", current.Enabled.ToString());
            tmp.addElement("UniqueID", current.UniqueID);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("GroupID", current.GroupID);
            tmp.addElement("InstanceID", current.InstanceID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseSource");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        static public DataTable getSources(IntDatabase managementDB)
        {
            DataTable services;
            String squery = "select * from [Sources];";
            services = managementDB.Execute(squery);
            return services;
        }

        static public DataTable getSourceByName(IntDatabase managementDB, string SourceName)
        {
            String squery = "select * from [Sources] where [SourceName]=@sourcename;";
            Tree data = new Tree();
            data.addElement("@sourcename", SourceName);
            DataTable services = managementDB.ExecuteDynamic(squery, data);
            data.dispose();
            return services;
        }

        public ReceiverInterface ConditionalStart(CollectionState State, BaseFlow current)
        {
            ReceiverInterface result = null;
            IntDatabase managementDB = State.managementDB;

            if (current.ParentService.Enabled)
            {
                if (current.ParentService.ParentSource.Enabled)
                {
                    switch (current.ParentService.ParentSource.SourceName)
                    {
                        case "UDP Syslog":
                            {
                                int udpport = -1;
                                try
                                {
                                    int.TryParse(current.ParentService.Parameter.ExtractedMetadata.getElement("Port"), out udpport);
                                    if (udpport == 0)
                                    {
                                        udpport = 514;
                                    }
                                }
                                catch (Exception)
                                {
                                    udpport = 514;
                                }

                                string tmpip = current.Parameter.ExtractedMetadata.getElement("Server");

                                if (tmpip != "")
                                {
                                    Boolean launchReceiver = true;

                                    try
                                    {
                                        current.meta_ipaddress = IPAddress.Parse(tmpip);

                                        // Check for existing receiver with this port...

                                        foreach (BaseSource currentSource in State.Sources)
                                        {
                                            if (currentSource.SourceName== "UDP Syslog")
                                            {
                                                if (currentSource.Enabled)
                                                {
                                                    foreach (BaseService currentService in currentSource.Services)
                                                    {
                                                        if (currentService.Enabled)
                                                        {
                                                            foreach (ReceiverInterface recv in currentService.Receivers)
                                                            {
                                                                RvrUDP tmpRev = (RvrUDP)recv;
                                                                if (tmpRev.port == udpport)
                                                                {
                                                                    launchReceiver = false;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }                                        
                                    }
                                    catch (Exception xyz)
                                    {
                                        launchReceiver = false;
                                        current.Suspended = true;
                                    }

                                    if (launchReceiver)
                                    {
                                        RvrUDP newUDPReceiver = new RvrUDP(State, udpport);

                                        newUDPReceiver.setServiceID(current.ServiceID);
                                        lock (current.ParentService.Receivers.SyncRoot)
                                        {
                                            current.ParentService.Receivers.Add(newUDPReceiver);
                                        }
                                        result = newUDPReceiver;
                                    }
                                }
                            }
                            break;

                        case "Twitter Source":
                            {
                                RvrTwitter newTwitterReceiver = new RvrTwitter(State.fatumConfig, State);
                                newTwitterReceiver.twitService = current.ParentService;
                                newTwitterReceiver.setServiceID(current.ServiceID);

                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newTwitterReceiver);
                                }
                                result = newTwitterReceiver;
                            }
                            break;
                        case "API":
                            {
                                RvrAPI newAPIReceiver = new RvrAPI(State.fatumConfig, State);
                                newAPIReceiver.apiService = current.ParentService;
                                newAPIReceiver.setServiceID(current.ServiceID);

                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newAPIReceiver);
                                }
                                result = newAPIReceiver;
                            }
                            break;
                        case "TCP XML Source":
                            {
                                int tcpport = -1;
                                int.TryParse(current.Parameter.ExtractedMetadata.getElement("Port"), out tcpport);
                                RvrTCPXML newTCPXMLReceiver = new RvrTCPXML(State, tcpport);
                                newTCPXMLReceiver.LocalIpAddress = current.Parameter.ExtractedMetadata.getElement("Server");
                                newTCPXMLReceiver.setServiceID(current.ServiceID);
                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newTCPXMLReceiver);
                                }
                                result = newTCPXMLReceiver;
                            }
                            break;

                        case "Facebook":
                            {
                                RvrFacebook newFacebookReceiver = new RvrFacebook(State.fatumConfig, State);
                                newFacebookReceiver.setServiceID(current.ServiceID);
                                newFacebookReceiver.bindFlow(current);
                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newFacebookReceiver);
                                }
                                result = newFacebookReceiver;
                            }
                            break;

                        case "RSS Source":
                            {
                                RvrRSS newRSSReceiver = new RvrRSS(State.fatumConfig, State);
                                newRSSReceiver.ServiceID = current.ParentService.UniqueID;
                                newRSSReceiver.setServiceID(current.ServiceID);
                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newRSSReceiver);
                                }
                                result = newRSSReceiver;
                            }
                            break;

                        case "HTTP Source":
                            {
                                RvrHTTP newHTTPReceiver = new RvrHTTP(State.fatumConfig, State);
                                newHTTPReceiver.setServiceID(current.ServiceID);
                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newHTTPReceiver);
                                }
                                newHTTPReceiver.Start();
                            }
                            break;

                        case "WMI Event Source":
                            {
                                RvrWindowsWMI newWMIEventReceiver = new RvrWindowsWMI(State.fatumConfig, State);
                                newWMIEventReceiver.currentFlow = current;
                                newWMIEventReceiver.setServiceID(current.ServiceID);
                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newWMIEventReceiver);
                                }
                                result = newWMIEventReceiver;
                            }
                            break;

                        case "Email Source":
                            {
                                RvrEmail newEmailReceiver = new RvrEmail(State.fatumConfig, State);
                                newEmailReceiver.setServiceID(current.ServiceID);
                                newEmailReceiver.bindFlow(current);
                                lock (current.ParentService.Receivers.SyncRoot)
                                {
                                    current.ParentService.Receivers.Add(newEmailReceiver);
                                }
                                result = newEmailReceiver;
                            }
                            break;
                    }
                }
            }
            return result;
        }
    }
}
