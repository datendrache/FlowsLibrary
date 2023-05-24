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
using System.Net;

namespace Proliferation.Flows
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
                    Parameter.ExtractedMetadata.Dispose();
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
            data.AddElement("@SourceID", SourceID);

            flows = managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
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
                data.AddElement("ParameterID", source.ParameterID);
                data.AddElement("Description", source.Description);
                data.AddElement("Enabled", source.Enabled.ToString());
                data.AddElement("OwnerID", source.OwnerID);
                data.AddElement("GroupID", source.GroupID);
                data.AddElement("*@UniqueID", source.UniqueID);
                managementDB.UpdateTree("[Sources]", data, "UniqueID=@UniqueID");
                data.Dispose();
            }
            else
            {
                Tree parms = new Tree();
                parms.AddElement("DateAdded", DateTime.Now.Ticks.ToString());
                parms.AddElement("_DateAdded", "BIGINT");
                parms.AddElement("SourceName", source.SourceName);
                parms.AddElement("ParameterID", source.ParameterID);
                parms.AddElement("Description", source.Description);
                source.UniqueID = "Q" + System.Guid.NewGuid().ToString().Replace("-", "");
                parms.AddElement("UniqueID", source.UniqueID);
                parms.AddElement("Enabled", source.Enabled.ToString());
                parms.AddElement("OwnerID", source.OwnerID);
                parms.AddElement("GroupID", source.GroupID);
                parms.AddElement("InstanceID", source.InstanceID);
                managementDB.InsertTree("[Sources]", parms);
            }
        }

        static public BaseSource loadSourceByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            DataTable sources;
            BaseSource result = null;
            String query = "select * from [Sources] where [UniqueID]=@uid;";
            Tree parms = new Tree();
            parms.AddElement("@uid", uniqueid);
            sources = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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
            data.SetElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
        }

        static public BaseSource loadSourceByName(IntDatabase managementDB, string sourcename)
        {
            DataTable sources;
            BaseSource result = null;

            String query = "select * from [Sources] where [SourceName]=@sourcename;";
            
            Tree parms = new Tree();
            parms.AddElement("@sourcename", sourcename);
            sources = managementDB.ExecuteDynamic(query, parms);
            parms.Dispose();

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

            tmp.AddElement("DateAdded", current.DateAdded.ToString());
            tmp.AddElement("Category", current.Category);
            tmp.AddElement("SourceName", current.SourceName);
            tmp.AddElement("ParameterID", current.ParameterID);
            tmp.AddElement("Description", current.Description);
            tmp.AddElement("Enabled", current.Enabled.ToString());
            tmp.AddElement("UniqueID", current.UniqueID);
            tmp.AddElement("OwnerID", current.OwnerID);
            tmp.AddElement("GroupID", current.GroupID);
            tmp.AddElement("InstanceID", current.InstanceID);

            TextWriter outs = new StringWriter();
            TreeDataAccess.WriteXML(outs, tmp, "BaseSource");
            tmp.Dispose();
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
            data.AddElement("@sourcename", SourceName);
            DataTable services = managementDB.ExecuteDynamic(squery, data);
            data.Dispose();
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
                                    int.TryParse(current.ParentService.Parameter.ExtractedMetadata.GetElement("Port"), out udpport);
                                    if (udpport == 0)
                                    {
                                        udpport = 514;
                                    }
                                }
                                catch (Exception)
                                {
                                    udpport = 514;
                                }

                                string tmpip = current.Parameter.ExtractedMetadata.GetElement("Server");

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
                                int.TryParse(current.Parameter.ExtractedMetadata.GetElement("Port"), out tcpport);
                                RvrTCPXML newTCPXMLReceiver = new RvrTCPXML(State, tcpport);
                                newTCPXMLReceiver.LocalIpAddress = current.Parameter.ExtractedMetadata.GetElement("Server");
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
