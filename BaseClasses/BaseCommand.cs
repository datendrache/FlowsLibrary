//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using FatumCore;
using DatabaseAdapters;
using System.ServiceModel.Dispatcher;
using System.Management.Automation.Runspaces;
using PhlozLanguages;
using Microsoft.Exchange.WebServices.Data;
using PhlozLib.SearchCore;
using Fatum.FatumCore;

namespace PhlozLib
{
    public class BaseCommand
    {
        public string OwnerID = "";
        public string Source = "";
        public Tree Command = null;
        public string Metadata = "";
        public string UniqueID = "";
        public long Issued = DateTime.Now.Ticks;
        public string Status = "";
        public string Message = "";
        public string Name = "";
        public string InstanceID = "";

        public BaseCommand()
        {

        }

        ~BaseCommand()
        {
            if (Command != null)
            {
                Command.dispose();
                Command = null;
            }
            OwnerID = null;
            Source = null;
            Metadata = null;
            UniqueID = null;
            Status = null;
            Message = null;
            Name = "";
            InstanceID = "";
        }

        static public ArrayList loadCommands(IntDatabase managementDB)
        {
            DataTable commands;
            String query = "select * from [Commands];";
            commands = managementDB.Execute(query);

            ArrayList tmpCommand = new ArrayList();

            foreach (DataRow row in commands.Rows)
            {
                BaseCommand newCommand = new BaseCommand();

                newCommand.Metadata = row["Metadata"].ToString();
                newCommand.UniqueID = row["UniqueID"].ToString();
                newCommand.OwnerID = row["OwnerID"].ToString();
                newCommand.Source = row["Source"].ToString();
                newCommand.Issued = Convert.ToInt64(row["Issued"]);
                newCommand.Status = row["Status"].ToString();
                newCommand.Message = row["Message"].ToString();
                newCommand.InstanceID = row["InstanceID"].ToString();
                newCommand.Name = row["Name"].ToString();
                try
                {
                    newCommand.Command = XMLTree.readXMLFromString(newCommand.Metadata);
                }
                catch (Exception)
                {

                }
                tmpCommand.Add(newCommand);
            }
            return tmpCommand;
        }

        static public ArrayList loadPendingCommands(IntDatabase managementDB, string instanceid)
        {
            DataTable commands;
            String query = "select * from [Commands] where [InstanceID]=@instanceid and [Status]='Pending';";
            Tree data = new Tree();
            data.addElement("@instanceid", instanceid);
            commands = managementDB.ExecuteDynamic(query, data);
            data.dispose();

            ArrayList tmpCommand = new ArrayList();

            foreach (DataRow row in commands.Rows)
            {
                BaseCommand newCommand = new BaseCommand();

                newCommand.Metadata = row["Metadata"].ToString();
                newCommand.UniqueID = row["UniqueID"].ToString();
                newCommand.OwnerID = row["OwnerID"].ToString();
                newCommand.Source = row["Source"].ToString();
                newCommand.Issued = Convert.ToInt64(row["Issued"]);
                newCommand.Status = row["Status"].ToString();
                newCommand.Message = row["Message"].ToString();
                newCommand.InstanceID = row["InstanceID"].ToString();
                newCommand.Name = row["Name"].ToString();
                try
                {
                    newCommand.Command = XMLTree.readXMLFromString(newCommand.Metadata);
                }
                catch (Exception)
                {

                }
                tmpCommand.Add(newCommand);
            }
            return tmpCommand;
        }
        static public Boolean pendingCommandCheck(IntDatabase managementDB, string instanceid)
        {
            DataTable commands;
            String query = "select count(*) from [Commands] where [InstanceID]=@instanceid and [Status]='Pending';";
            Tree data = new Tree();
            data.addElement("@instanceid", instanceid);
            commands = managementDB.ExecuteDynamic(query, data);
            data.dispose();

            if (commands.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public Boolean clearCommandsByInstance(IntDatabase managementDB, string instanceid)
        {
            DataTable commands;
            String query = "delete from [Commands] where [InstanceID]=@instanceid;";
            Tree data = new Tree();
            data.addElement("@instanceid", instanceid);
            commands = managementDB.ExecuteDynamic(query, data);
            data.dispose();

            if (commands.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        static public void removeCommandByUniqueID(IntDatabase managementDB, string uniqueid)
        {
            String squery = "delete from [Commands] where [UniqueID]=@uniqueid;";
            Tree data = new Tree();
            data.setElement("@uniqueid", uniqueid);
            managementDB.ExecuteDynamic(squery, data);
            data.dispose();
        }

        static public void updateCommand(IntDatabase managementDB, BaseCommand command)
        {
            if (command.UniqueID != "")
            {
                Tree data = new Tree();
                data.addElement("Status", command.Status);
                data.addElement("Message", command.Message);
                data.addElement("*@UniqueID", command.UniqueID);
                managementDB.UpdateTree("[Commands]", data, "[UniqueID]=@UniqueID");
                data.dispose();
            }
            else
            {
                string sql = "";
                sql = "INSERT INTO [Commands] ([Name], [InstanceID], [OwnerID], [UniqueID], [Source], [Issued], [Status], [Message], [Metadata]) VALUES (@Name, @InstanceID, @OwnerID, @UniqueID, @Source, @Issued, @Status, @Message, @Metadata);";

                Tree NewChannel = new Tree();
                NewChannel.addElement("@Name", command.Name);
                NewChannel.addElement("@InstanceID", command.InstanceID);
                NewChannel.addElement("@OwnerID", command.OwnerID);
                command.UniqueID = "K" + System.Guid.NewGuid().ToString().Replace("-", "");
                NewChannel.addElement("@UniqueID", command.UniqueID);
                NewChannel.addElement("@Source", command.Source);
                NewChannel.addElement("@Issued", command.Issued.ToString());
                NewChannel.addElement("@Status", command.Status);
                NewChannel.addElement("@Message", command.Message);
                command.Metadata = TreeDataAccess.writeTreeToXMLString(command.Command, "Metadata");
                NewChannel.addElement("@Metadata", command.Metadata);

                managementDB.ExecuteDynamic(sql, NewChannel);
                NewChannel.dispose();
            }
        }

        static public void defaultSQL(IntDatabase database, int DatabaseSyntax)
        {
            string configDB = "";
            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE TABLE [Commands](" +
                        "[OwnerID] TEXT NULL, " +
                        "[InstanceID] TEXT NULL, " +
                        "[UniqueID] TEXT NULL, " +
                        "[Source] TEXT NULL, " +
                        "[Issued] INTEGER NULL, " +
                        "[Status] TEXT NULL, " +
                        "[Message] TEXT NULL, " +
                        "[Name] TEXT NULL, " +
                        "[Metadata] TEXT NULL);";
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE TABLE [Commands](" +
                        "[OwnerID] VARCHAR(33) NULL, " +
                        "[InstanceID] VARCHAR(33) NULL, " +
                        "[UniqueID] VARCHAR(33) NULL, " +
                        "[Source] VARCHAR(50), " +
                        "[Issued] BIGINT NULL, " +
                        "[Status] VARCHAR(20) NULL, " +
                        "[Message] TEXT NULL, " +
                        "[Name] TEXT NULL, " +
                        "[Metadata] TEXT NULL);";
                    break;
            }

            database.ExecuteNonQuery(configDB);

            // Create Indexes

            switch (DatabaseSyntax)
            {
                case DatabaseSoftware.SQLite:
                    configDB = "CREATE INDEX ix_basecommands ON Commands([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basecommandsinstance ON Commands([InstanceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
                case DatabaseSoftware.MicrosoftSQLServer:
                    configDB = "CREATE INDEX ix_basecommands ON Commands([UniqueID]);";
                    database.ExecuteNonQuery(configDB);
                    configDB = "CREATE INDEX ix_basecommandsinstance ON Commands([InstanceID]);";
                    database.ExecuteNonQuery(configDB);
                    break;
            }
        }

        static public void deleteCommand(IntDatabase managementDB, BaseCommand Command)
        {
            string delSQL = "DELETE FROM [Commands] WHERE [UniqueID]=@commandid;";
            Tree parms = new Tree();
            parms.addElement("@commandid", Command.UniqueID);
            managementDB.ExecuteDynamic(delSQL, parms);
            parms.dispose();
        }

        public static DataTable getCommandsList(IntDatabase managementDB)
        {
            string SQL = "select * from [Commands];";
            DataTable dt = managementDB.Execute(SQL);
            return dt;
        }

        static public string getXML(BaseCommand current)
        {
            string result = "";
            Tree tmp = new Tree();
            tmp.addElement("Name", current.Name);
            tmp.addElement("OwnerID", current.OwnerID);
            tmp.addElement("UniqueID", current.UniqueID); 
            tmp.addElement("Source", current.Source);
            tmp.addElement("InstanceID", current.InstanceID);
            tmp.addElement("Message", current.Message);
            tmp.addElement("Status", current.Status);
            tmp.addElement("Issued", current.Issued.ToString());
            tmp.addNode(current.Command.Duplicate(), "Metadata");

            TextWriter outs = new StringWriter();
            TreeDataAccess.writeXML(outs, tmp, "BaseCommand");
            tmp.dispose();
            result = outs.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "");
            return result;
        }

        public void performCommand(CollectionState State)
        {
            switch (Name.ToLower())
            {
                case "updateflow":
                    updateFlow(State);
                    break;
                case "activateflow":
                    resumeFlow(State);
                    break;
                case "deactivateflow":
                    suspendFlow(State);
                    break;
                case "registerflow":
                    registerFlow(State);
                    break;
                case "deregisterflow":
                    deregisterFlow(State);
                    break;
                case "deleteflow":
                    deleteFlow(State);
                    break;
                case "activateservice":
                    activateService(State);
                    break;
                case "deactivateservice":
                    deactivateService(State);
                    break;
                case "updateservice":
                    updateService(State);
                    break;
                case "activatesource":
                    activateSource(State);
                    break;
                case "deactivatesource":
                    deactivateSource(State);
                    break;
                case "updateprocessor":
                    updateProcessor(State);
                    break;
                case "deleteprocessor":
                    deleteProcessor(State);
                    break;
                case "enableprocessor":
                    enableProcessor(State);
                    break;
                case "suspendprocessor":
                    suspendProcessor(State);
                    break;
                case "resumeprocessor":
                    resumeProcessor(State);
                    break;
                case "updaterules":
                    updateRules(State);
                    break;
                case "enablerule":
                    enableRule(State);
                    break;
                case "disablerule":
                    disableRule(State);
                    break;
                case "deleterule":
                    deleteRule(State);
                    break;
                case "addtask":
                    addTask(State);
                    break;
                case "updatetask":
                    updateTask(State);
                    break;
                case "deletetask":
                    deleteTask(State);
                    break;
                case "updateforwarder":
                    updateForwarder(State);
                    break;
                case "deleteforwarder":
                    deleteForwarder(State);
                    break;
                case "activateforwarder":
                    activateforwarder(State);
                    break;
                case "systemrestart":
                    systemrestart(State);
                    break;
                default:
                    // Some error message needs to go here...
                    break;
            }
            Status = "Complete";
            BaseCommand.updateCommand(State.managementDB, this);
        }

        private void systemrestart(CollectionState state)
        {
            systemcollectionstop(state);
            state.LoadState();
        }

        public static void systemcollectionstop(CollectionState State)
        {
            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    currentService.Enabled = false;
                    foreach (ReceiverInterface current in currentService.Receivers)
                    {
                        current.Stop();
                        current.Dispose();
                    }
                    currentService.Receivers.Clear();
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        try
                        {
                            currentFlow.Suspend();
                            currentFlow.close(State);
                        }
                        catch (Exception xyz)
                        {
                            int i = 0;
                        }
                    }
                    currentService.Flows.Clear();
                }
                currentSource.Enabled = false;
                currentSource.Services.Clear();
            }

            if (State.searchSystem!=null)
            {
                State.searchSystem.closeDatabases();
            }

            foreach (BaseForwarder currentForwarder in State.Forwarders)
            {
                try
                {
                    currentForwarder.Enabled = "false";
                }
                catch (Exception xyz)
                {
                    int i = 0;
                }
            }
            State.Forwarders.Clear();
            State.ChannelFlows.Clear();
            State.Channels.Clear();
            State.Instances.Clear();
            State.Filters.Clear();
            State.DocumentColors.Clear();
            State.TaskList.Clear();
            if (State.searchSystem!=null)
            {
                State.searchSystem.closeDatabases();
                State.searchSystem = null;
            }
        }

        private void activateforwarder(CollectionState state)
        {
            // Load all forwarders
            ArrayList forwarderList = BaseForwarder.loadForwarders(state.managementDB);
            ArrayList insertList = new ArrayList();

            foreach (BaseForwarder existingForwarder in state.Forwarders)
            {
                Boolean matchedForwarder = false;

                foreach (BaseForwarder currentForwarder in forwarderList)
                {
                if (currentForwarder.UniqueID == existingForwarder.UniqueID)
                    {
                        insertList.Add(currentForwarder);
                    }
                }
            }

            if (insertList.Count>0)
            {
                lock (state.Forwarders.SyncRoot)
                {
                    foreach (BaseForwarder currentForwarder in insertList)
                    {
                        state.Forwarders.Add(currentForwarder);
                    }
                }
            }

            forwarderList.Clear();
            insertList.Clear();
        }

        private void deleteForwarder(CollectionState state)
        {
            string forwarderid = Command.getElement("ForwarderID");
            int index = 0;
            int foundindex = -1;
            foreach (BaseForwarder forwarder in state.Forwarders)
            {
                if (forwarder.UniqueID == forwarderid)
                {
                    foundindex = index;
                    break;
                }
                index++;
            }

            if (foundindex != -1)
            {
                BaseForwarder currentForwarder = (BaseForwarder)state.Forwarders[foundindex];
                if (currentForwarder != null)
                {
                    state.Forwarders.RemoveAt(foundindex);
                }

                foreach (BaseSource currentsource in state.Sources)
                {
                    foreach (BaseService currentservice in currentsource.Services)
                    {
                        foreach (BaseFlow currentflow in currentservice.Flows)
                        {
                            FlowReference flowinfo = currentflow.flowReference;
                            if (flowinfo != null)
                            {
                                if (flowinfo.ForwarderLinks != null)
                                {
                                    int forwarderindex = -1;
                                    int loopindex = 0;

                                    foreach (ForwarderLink currentLink in flowinfo.ForwarderLinks)
                                    {
                                        if (currentLink.ForwarderID== forwarderid)
                                        {
                                            forwarderindex = loopindex;
                                            break;
                                        }
                                        loopindex++;
                                    }
                                    if (forwarderindex!=-1)
                                    {
                                        ForwarderLink targetLink = (ForwarderLink) flowinfo.ForwarderLinks[forwarderindex];
                                        lock (flowinfo.ForwarderLinks.SyncRoot)
                                        {
                                            flowinfo.ForwarderLinks.RemoveAt(forwarderindex);
                                        }
                                        targetLink.ForwarderID = "removed";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            activateforwarder(state);
        }

        private void updateForwarder(CollectionState state)
        {
            string forwarderid = Command.getElement("ForwarderID");
            int index = 0;
            int foundindex = -1;
            foreach (BaseForwarder forwarder in state.Forwarders)
            {
                if (forwarder.UniqueID == forwarderid)
                {
                    foundindex = index;
                    break;
                }
                index++;
            }

            if (foundindex != -1)
            {
                BaseForwarder currentForwarder = (BaseForwarder)state.Forwarders[foundindex];
                if (currentForwarder != null)
                {
                    state.Forwarders.RemoveAt(foundindex);
                }

                foreach (BaseSource currentsource in state.Sources)
                {
                    foreach (BaseService currentservice in currentsource.Services)
                    {
                        foreach (BaseFlow currentflow in currentservice.Flows)
                        {
                            FlowReference flowinfo = currentflow.flowReference;
                            if (flowinfo != null)
                            {
                                if (flowinfo.ForwarderLinks != null)
                                {
                                    int forwarderindex = -1;
                                    int loopindex = 0;

                                    foreach (ForwarderLink currentLink in flowinfo.ForwarderLinks)
                                    {
                                        if (currentLink.ForwarderID == forwarderid)
                                        {
                                            forwarderindex = loopindex;
                                            break;
                                        }
                                        loopindex++;
                                    }
                                    if (forwarderindex != -1)
                                    {
                                        ForwarderLink targetLink = (ForwarderLink)flowinfo.ForwarderLinks[forwarderindex];
                                        lock (flowinfo.ForwarderLinks.SyncRoot)
                                        {
                                            flowinfo.ForwarderLinks.RemoveAt(forwarderindex);
                                        }
                                        targetLink.ForwarderID = "removed";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void updateFlow(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");
            string flowid = Command.getElement("FlowID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            Boolean isPresent = false;

                            for (int i=0;i<currentService.Flows.Count;i++)
                            {
                                BaseFlow currentFlow = (BaseFlow)currentService.Flows[i];
                                if (currentFlow.UniqueID == flowid)
                                {
                                    isPresent = true;
                                    if (currentFlow.Enabled)
                                    {
                                        currentFlow.Suspend();
                                        currentFlow.reload(State);
                                        BaseFlow.enableFlow(State, currentFlow);
                                        currentFlow.Resume();
                                        break; 
                                    }
                                    else
                                    {
                                        currentFlow.Suspend();
                                        currentFlow.close(State);
                                        currentService.Flows.RemoveAt(i);
                                        break;
                                    }
                                }
                            }

                            if (!isPresent)
                            {
                                BaseFlow currentFlow = BaseFlow.loadFlowByUniqueID(State.managementDB, flowid);
                                
                                if (currentFlow.Enabled)
                                {
                                    currentFlow.ParentService = currentService;
                                    currentFlow.Suspend();
                                    currentFlow.reload(State);
                                    BaseFlow.enableFlow(State, currentFlow);
                                    currentService.Flows.Add(currentFlow);
                                    currentFlow.Resume();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void suspendFlow(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");
            string flowid = Command.getElement("FlowID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            foreach (BaseFlow currentFlow in currentService.Flows)
                            {
                                if (currentFlow.UniqueID == flowid)
                                {
                                    currentFlow.Suspend();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void registerFlow(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");
            string flowid = Command.getElement("FlowID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    if (currentSource.Enabled)
                    {
                        foreach (BaseService currentService in currentSource.Services)
                        {
                            if (serviceid == currentService.UniqueID)
                            {
                                if (currentService.Enabled)
                                {
                                    Boolean flowAlreadyExists = false;
                                    foreach (BaseFlow currentFlow in currentService.Flows)
                                    {
                                        if (currentFlow.UniqueID == flowid)
                                        {
                                            flowAlreadyExists = true;
                                            break;
                                        }
                                    }

                                    if (!flowAlreadyExists)
                                    {
                                        BaseFlow newFlow = BaseFlow.loadFlowByUniqueID(State.managementDB, flowid);
                                        if (newFlow.Enabled)
                                        {
                                            newFlow.ParentService = currentService;
                                            currentService.Flows.Add(newFlow);
                                            BaseFlow.enableFlow(State, newFlow);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void deregisterFlow(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");
            string flowid = Command.getElement("FlowID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            int flowIndex = 0;
                            Boolean found = false;

                            foreach (BaseFlow currentFlow in currentService.Flows)
                            {
                                if (currentFlow.UniqueID == flowid)
                                {
                                    found = true;
                                    break;
                                }
                                flowIndex++;
                            }

                            if (found)
                            {
                                lock (currentService.Flows.SyncRoot)
                                {
                                    BaseFlow targetFlow = (BaseFlow)currentService.Flows[flowIndex];
                                    targetFlow.Suspend();
                                    currentService.Flows.RemoveAt(flowIndex);
                                    foreach (ReceiverInterface receiver in currentService.Receivers)
                                    {
                                        receiver.deregisterFlow(targetFlow);
                                    }
                                    if (flowIndex > -1)
                                    {
                                        lock (currentService.Flows.SyncRoot)
                                        {
                                            currentService.Flows.RemoveAt(flowIndex);
                                        }
                                    }
                                    targetFlow.close(State);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void deleteFlow(CollectionState State)
        {
            string flowid = Command.getElement("FlowID");
            BaseFlow.deleteFlow(State.managementDB, flowid);
        }

        private void resumeFlow(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");
            string flowid = Command.getElement("FlowID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            foreach (BaseFlow currentFlow in currentService.Flows)
                            {
                                if (currentFlow.UniqueID == flowid)
                                {
                                    if (currentFlow.Enabled)
                                    {
                                        currentFlow.Resume();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void deactivateService(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            // Step one, lets stop everything.
                            currentService.Enabled = false;
                            foreach (BaseFlow currentFlow in currentService.Flows)
                            {
                                currentFlow.Suspend();
                                foreach (ReceiverInterface receiver in currentService.Receivers)
                                {
                                    receiver.deregisterFlow(currentFlow);
                                }
                                currentFlow.close(State);
                            }

                            currentService.Flows.Clear();
                            BaseService.updateService(State.managementDB, currentService);
                            break;
                        }
                    }
                }
            }
        }

        private void activateService(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            currentService.Flows = BaseFlow.loadFlowsByServiceEnabledOnly(State, currentService);
                            currentService.Enabled = true;
                            foreach (BaseFlow currentFlow in currentService.Flows)
                            {
                                foreach (ReceiverInterface receiver in currentService.Receivers)
                                {
                                    if (currentFlow.Enabled)
                                    {
                                        BaseFlow.enableFlow(State, currentFlow);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void updateService(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");
            string serviceid = Command.getElement("ServiceID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (serviceid == currentService.UniqueID)
                        {
                            BaseService updatedService = BaseService.loadServiceByUniqueID(State.managementDB, currentService.UniqueID);

                            // Credentials...

                            if (updatedService.CredentialID != currentService.CredentialID)
                            {
                                currentService.CredentialID = updatedService.CredentialID;
                                BaseCredential disposeme = currentService.Credentials;
                                currentService.Credentials = updatedService.Credentials;
                                disposeme.ExtractedMetadata.dispose();
                                disposeme.ExtractedMetadata = null;
                            }

                            // Parameters...

                            if (updatedService.ParameterID != currentService.ParameterID)
                            {
                                currentService.ParameterID = updatedService.ParameterID;
                                BaseParameter disposeme = currentService.Parameter;
                                currentService.Parameter = updatedService.Parameter;
                                disposeme.ExtractedMetadata.dispose();
                                disposeme.ExtractedMetadata = null;
                            }

                            currentService.GroupID = updatedService.GroupID;
                            currentService.OwnerID = updatedService.OwnerID;
                            currentService.ServiceType = updatedService.ServiceType;
                            currentService.ServiceSubtype = updatedService.ServiceSubtype;
                            currentService.Description = updatedService.Description;
                            currentService.DefaultRuleGroup = updatedService.DefaultRuleGroup;
                            currentService.ServiceName = updatedService.ServiceName;

                            // dispose of updated service...

                            updatedService.Parameter = null;
                            updatedService.Credentials = null;

                            // Check if we are starting / restarting

                            if (updatedService.Enabled == true && currentService.Enabled == false)
                            {
                                currentService.Enabled = true;
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.Enabled)
                                    {
                                        BaseFlow.enableFlow(State, currentFlow);
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void activateSource(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    currentSource.Enabled = true;
                    currentSource.Services = BaseService.loadServicesBySource(State.managementDB, currentSource);
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        currentService.Flows = BaseFlow.loadFlowsByServiceEnabledOnly(State, currentService);
                       
                        if (currentService.Enabled)
                        {
                            foreach (BaseFlow currentFlow in currentService.Flows)
                            {
                                if (currentFlow.Enabled)
                                {
                                    BaseFlow.enableFlow(State, currentFlow);
                                }
                            }
                        }
                        break;
                    }
                    break;
                }
            }
        }

        private void deactivateSource(CollectionState State)
        {
            string sourceid = Command.getElement("SourceID");

            foreach (BaseSource currentSource in State.Sources)
            {
                if (sourceid == currentSource.UniqueID)
                {
                    currentSource.Enabled = false;

                    foreach (BaseService currentService in currentSource.Services)
                    {
                        foreach (ReceiverInterface receiver in currentService.Receivers)
                        {
                            receiver.StartSuspend();
                        }

                        foreach (BaseFlow currentFlow in currentService.Flows)
                        {
                            currentFlow.Suspend();
                            foreach (ReceiverInterface receiver in currentService.Receivers)
                            {
                                receiver.deregisterFlow(currentFlow);
                            }
                            currentFlow.close(State);
                        }

                        currentService.Flows.Clear();

                        foreach (ReceiverInterface receiver in currentService.Receivers)
                        {
                            receiver.Stop();
                        }
                        currentService.Receivers.Clear();

                        foreach (BaseFlow currentFlow in currentService.Flows)
                        {
                            currentFlow.close(State);
                        }

                        currentService.Flows.Clear();
                    }
                    break;
                }
            }
        }

        private void updateProcessor(CollectionState State)
        {
            string processorid = Command.getElement("ProcessorID");

            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.Enabled)
                        {
                            if (currentFlow.ProcessingEnabled)
                            {
                                Boolean HasProcessor = false;
                                foreach (BaseProcessor currentProcessor in currentFlow.flowReference.Processors)
                                {
                                    if (currentProcessor.UniqueID == processorid)
                                    {
                                        HasProcessor = true;
                                        break;
                                    }
                                }
                                if (HasProcessor)
                                {
                                    currentFlow.Suspend();
                                    FlowReference newReference = new FlowReference(currentFlow, State, State.MasterReceiver.onDocumentReceived);
                                    currentFlow.flowReference = newReference;
                                    // Might want to somehow deallocate the old flowreference but it might crash a running process...
                                    currentFlow.Resume();
                                }
                            }
                        }
                    }
                }
            }

            foreach (BaseTask currentTask in State.TaskList)
            {
                if (currentTask.ProcessorID == processorid)
                {
                    IntLanguage tmp = currentTask.runtime;
                    currentTask.runtime = null;
                    if (tmp != null)
                    {
                        tmp.dispose();
                    }
                }
            }
        }

        private void enableProcessor(CollectionState State)
        {
            string processorid = Command.getElement("ProcessorID");

            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.ProcessingEnabled)
                        {
                            Boolean HasProcessor = false;
                            foreach (BaseProcessor currentProcessor in currentFlow.flowReference.Processors)
                            {
                                if (currentProcessor.UniqueID == processorid)
                                {
                                    HasProcessor = true;
                                    break;
                                }
                            }
                            if (HasProcessor)
                            {
                                currentFlow.Suspend();
                                FlowReference newReference = new FlowReference(currentFlow, State, State.MasterReceiver.onDocumentReceived);
                                currentFlow.flowReference = newReference;
                                // Might want to somehow deallocate the old flowreference but it might crash a running process...
                                currentFlow.Resume();
                            }
                        }
                    }
                }
            }

            foreach (BaseTask currentTask in State.TaskList)
            {
                if (currentTask.ProcessorID == processorid)
                {
                    IntLanguage tmp = currentTask.runtime;
                    currentTask.runtime = null;
                    if (tmp != null)
                    {
                        tmp.dispose();
                    }
                }
            }
        }

        private void deleteProcessor(CollectionState State)
        {
            string processorid = Command.getElement("ProcessorID");

            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        for (int i = 0; i < currentFlow.flowReference.Processors.Count; i++)
                        {
                            BaseProcessor currentProcessor = (BaseProcessor)currentFlow.flowReference.Processors[i];
                            if (currentProcessor.UniqueID == processorid)
                            {
                                currentFlow.Suspend();
                                currentFlow.flowReference.Processors.RemoveAt(i);
                                currentFlow.flowReference.Workspaces.RemoveAt(i);
                                currentFlow.Resume();
                                break;
                            }
                        }
                    }
                }
            }

            foreach (BaseTask currentTask in State.TaskList)
            {
                if (currentTask.ProcessorID == processorid)
                {
                    currentTask.ProcessorID = "";
                    IntLanguage tmp = currentTask.runtime;
                    currentTask.runtime = null;
                    if (tmp != null)
                    {
                        tmp.dispose();
                    }
                }
            }
        }

        private void suspendProcessor(CollectionState State)
        {
            string processorid = Command.getElement("ProcessorID");

            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        for (int i = 0; i < currentFlow.flowReference.Processors.Count; i++)
                        {
                            BaseProcessor currentProcessor = (BaseProcessor)currentFlow.flowReference.Processors[i];
                            if (currentProcessor.UniqueID == processorid)
                            {
                                currentFlow.Suspend();
                                currentProcessor.Enabled = "false";
                                currentFlow.Resume();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void resumeProcessor(CollectionState State)
        {
            string processorid = Command.getElement("ProcessorID");

            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        for (int i = 0; i < currentFlow.flowReference.Processors.Count; i++)
                        {
                            BaseProcessor currentProcessor = (BaseProcessor)currentFlow.flowReference.Processors[i];
                            if (currentProcessor.UniqueID == processorid)
                            {
                                currentFlow.Suspend();
                                currentProcessor.Enabled = "true";
                                currentFlow.Resume();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void updateRules(CollectionState State)
        {
            string processorid = Command.getElement("RuleGroupID");
            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.Enabled)
                        {
                            if (currentFlow.ProcessingEnabled)
                            {
                                if (currentFlow.RuleGroupID == processorid)
                                {
                                    currentFlow.Suspend();
                                    Boolean HasProcessor = false;
                                    FlowReference newReference = new FlowReference(currentFlow, State, State.MasterReceiver.onDocumentReceived);
                                    currentFlow.flowReference = newReference;

                                    // Might want to somehow deallocate the old flowreference but it might crash a running process...

                                    currentFlow.Resume();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void enableRule(CollectionState State)
        {
            string ruleid = Command.getElement("RuleID");
            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.flowReference!=null)
                        {
                            if (currentFlow.flowReference.Rules!=null)
                            {
                                foreach (BaseRule currentRule in currentFlow.flowReference.Rules)
                                {
                                    if (currentRule.UniqueID == ruleid)
                                    {
                                        currentRule.Enabled = "true";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void disableRule(CollectionState State)
        {
            string ruleid = Command.getElement("RuleID");
            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.flowReference != null)
                        {
                            if (currentFlow.flowReference.Rules != null)
                            {
                                foreach (BaseRule currentRule in currentFlow.flowReference.Rules)
                                {
                                    if (currentRule.UniqueID == ruleid)
                                    {
                                        currentRule.Enabled = "false";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void deleteRule(CollectionState State)
        {
            string ruleid = Command.getElement("RuleID");
            foreach (BaseSource currentSource in State.Sources)
            {
                foreach (BaseService currentService in currentSource.Services)
                {
                    foreach (BaseFlow currentFlow in currentService.Flows)
                    {
                        if (currentFlow.flowReference != null)
                        {
                            if (currentFlow.flowReference.Rules != null)
                            {
                                for (int i = 0; i < currentFlow.flowReference.Rules.Count; i++)
                                {
                                    BaseRule currentRule = (BaseRule)currentFlow.flowReference.Rules[i];
                                    if (currentRule.UniqueID==ruleid)
                                    {
                                        currentFlow.flowReference.Rules.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void addTask(CollectionState State)
        {
            string ruleid = Command.getElement("TaskID");
            BaseTask newTask = BaseTask.loadTaskByUniqueID(State.managementDB, ruleid);
            newTask.lastrun = DateTime.Now;
            if (newTask!=null)
            {
                if (newTask.Enabled.ToString().ToLower() == "true")
                {
                    newTask.lastrun = DateTime.Now;
                    State.TaskList.Add(newTask);
                }
            }
        }

        private void updateTask(CollectionState State)
        { 
            string ruleid = Command.getElement("TaskID");
            BaseTask updatedTask = BaseTask.loadTaskByUniqueID(State.managementDB, ruleid);

            foreach (BaseTask currentTask in State.TaskList)
            {
                if (currentTask.UniqueID == updatedTask.UniqueID)
                {
                    currentTask.Description = updatedTask.Description;
                    currentTask.Enabled = updatedTask.Enabled;
                    currentTask.EndOfMonth = updatedTask.EndOfMonth;
                    currentTask.Monday = updatedTask.Monday;
                    currentTask.Tuesday = updatedTask.Tuesday;
                    currentTask.Wednesday = updatedTask.Wednesday;
                    currentTask.Thursday = updatedTask.Thursday;
                    currentTask.Friday = updatedTask.Friday;
                    currentTask.Saturday = updatedTask.Saturday;
                    currentTask.Sunday = updatedTask.Sunday;
                    currentTask.hour = updatedTask.hour;
                    currentTask.minute = updatedTask.minute;
                    currentTask.Name = updatedTask.Name;
                    currentTask.Occurence = updatedTask.Occurence;
                    if (currentTask.ProcessorID != updatedTask.ProcessorID)
                    {
                        currentTask.ProcessorID = updatedTask.ProcessorID;
                        IntLanguage tmp = currentTask.runtime;
                        currentTask.runtime = null;
                        if (tmp!=null)
                        {
                            tmp.dispose();
                        }
                    }
                }
            }
        }

        private void deleteTask(CollectionState State)
        {
            string ruleid = Command.getElement("TaskID");
            int index = 0;
            int position = -1;
            
            foreach (BaseTask currentTask in State.TaskList)
            {
                if (currentTask.UniqueID == ruleid)
                {
                    position = index;
                    break;
                }
                index++;
            }

            if (position != -1)
            {
                try
                {
                    BaseTask tmpTask = (BaseTask)State.TaskList[position];
                    State.TaskList.RemoveAt(position);
                    IntLanguage tmp = tmpTask.runtime;
                    tmpTask.runtime = null;
                    if (tmp != null)
                    {
                        tmp.dispose();
                    }
                }
                catch (Exception xyz)
                {
                    // Concurrency exception, most likely. This will abort normally after it completes running.
                }
            }
        }
    }
}

