//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using FatumCore;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;

namespace PhlozLib
{
    public class RvrUDP : ReceiverInterface 
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        public int port = 0;
        string ServiceID = null;

        CollectionState State = null;

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onReceiverError;
        public EventHandler onStopped;
        public DocumentEventHandler onDocumentReceived;
        public FlowEventHandler onFlowDetected;

        Thread receiverThread = null;
        Thread processorThread = null;

        ArrayList Buffer = new ArrayList(5000);

        UdpClient udpListener = null;

        // Status Fields

        DateTime ReceiverStartTime = DateTime.Now;

        public RvrUDP(CollectionState S, int Port)
        {
            State = S;
            port = Port;
        }

        public void setCallbacks(DocumentEventHandler documentEventHandler,
    PhlozLib.ErrorEventHandler errorEventHandler,
    EventHandler communicationLost,
    EventHandler stoppedReceiver,
    FlowEventHandler flowEventHandler)
        {
            onDocumentReceived = new DocumentEventHandler(documentEventHandler);
            onReceiverError = new PhlozLib.ErrorEventHandler(errorEventHandler);
            onCommunicationLost = new EventHandler(communicationLost);
            onStopped = new EventHandler(stoppedReceiver);
            onFlowDetected = new FlowEventHandler(flowEventHandler);
        }

        public void Start()
        {
            if (active)
            {
                if (!running)
                {
                    if ((port <= 0) || (port > 65535))
                    {
                        if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + "Invalid UDP port provided for flow."));
                    }
                    else
                    {
                        ReceiverStartTime = DateTime.Now;
                        receiverThread = new System.Threading.Thread(startReceiver);
                        //receiverThread.IsBackground = true;
                        receiverThread.Name = "PhlozUDPReceiver";
                        processorThread = new System.Threading.Thread(startProcessor);
                        //processorThread.IsBackground = true;
                        processorThread.Name = "PhlozUDPProcessor";
                        receiverThread.Start();
                        processorThread.Start();
                    }
                }
                else
                {
                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + "Receiver told to start, already running."));
                }
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Error: " + getReceiverType() + " Receiver told to start, already disposed."));
            }
        }

        private void startReceiver()
        {
            running = true;
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                Boolean waitingForBinding = true;
                byte[] bReceive;

                while (waitingForBinding)
                {
                    try
                    {
                        udpListener = new UdpClient(port);
                        waitingForBinding = false;
                    }
                    catch (Exception xyz)
                    {
                        waitingForBinding = true;
                        Thread.Sleep(1000); // Wait one second and try again.
                    }
                }

                /* Main Loop */
                /* Listen for incoming data on udp port 514 (default for SysLog events) */
                while (running)
                {
                    try
                    {
                        if (!suspended && running)
                        {
                            if (udpListener.Available > 0)
                            {
                                bReceive = udpListener.Receive(ref anyIP);
                                MessageNetworkReference newRef = new MessageNetworkReference();
                                newRef.ip = anyIP.Address;
                                newRef.message = bReceive;
                                if (bReceive!=null)
                                {
                                    lock (Buffer.SyncRoot)
                                    {
                                        Buffer.Add(newRef);
                                    }
                                }
                                else
                                {
                                    int wtf = 1;
                                }
                            }
                            else
                            {
                                Thread.Sleep(50);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        int breakpnt = 0;
                        breakpnt++;
                    }
                }

                try
                {
                    udpListener.Close();
                }
                catch (Exception xyz)
                {
                    int zyz = 0;
                }
                onStopped.Invoke(this, new EventArgs());
            }
            catch (ThreadInterruptedException exception)
            {
                int itsbad = 0;
            }
        }

        private void startProcessor()
        {
            string sReceive;
            string sourceIP;
            running = true;
            ArrayList PendingNewFeeds = new ArrayList();

            while (running)
            {
                try
                {
                    if (Buffer.Count > 0)
                    {
                        ArrayList tmpList = new ArrayList(5000);

                        lock (Buffer.SyncRoot)
                        {
                            try
                            {
                                foreach (var item in Buffer)
                                {
                                    tmpList.Add(item);
                                }
                                Buffer.Clear();
                            }
                            catch (Exception xyzzy)
                            {

                            }
                        }

                        lock (tmpList.SyncRoot)
                        {
                            foreach (MessageNetworkReference incoming in tmpList)
                            {
                                /* Convert incoming data from bytes to ASCII */
                                sReceive = Encoding.ASCII.GetString(incoming.message);
                                /* Get the IP of the device sending the syslog */

                                Boolean knownflow = false;

                                foreach (BaseSource currentSource in State.Sources)
                                {
                                    if (currentSource.Enabled)
                                    {
                                        foreach (BaseService currentService in currentSource.Services)
                                        {
                                            if (currentService.Enabled)
                                            {
                                                if (currentService.ServiceType=="UDP" && currentService.ServiceSubtype == "Syslog")
                                                {
                                                    foreach (BaseFlow currentFlow in currentService.Flows)
                                                    {
                                                        if (currentFlow.ServiceID == ServiceID)
                                                        {
                                                            try
                                                            {
                                                                if (currentFlow.meta_ipaddress!=null)
                                                                {
                                                                    if (currentFlow.meta_ipaddress.Equals(incoming.ip))
                                                                    {
                                                                        if (currentFlow.Enabled && !(currentFlow.Suspended))
                                                                        {
                                                                            sourceIP = incoming.ip.ToString();
                                                                            knownflow = true;

                                                                            DocumentEventArgs newArgs = new DocumentEventArgs();
                                                                            newArgs.Document = new BaseDocument(currentFlow);
                                                                            newArgs.Document.FlowID = currentFlow.UniqueID;
                                                                            newArgs.Document.received = DateTime.Now;
                                                                            newArgs.Document.Document = sReceive.Replace(Environment.NewLine, "").Trim();

                                                                            if (newArgs.Document.Metadata == null)
                                                                            {
                                                                                newArgs.Document.Metadata = new Tree();
                                                                            }
                                                                            newArgs.Document.Metadata.setElement("ip_address", sourceIP);
                                                                            newArgs.Document.Label = "Syslog";
                                                                            newArgs.Document.Category = "Raw";

                                                                            currentFlow.FlowStatus.DocumentCount++;
                                                                            currentFlow.FlowStatus.BytesReceived += incoming.message.Length;
                                                                            currentFlow.FlowStatus.MostRecentData = DateTime.Now;
                                                                            currentFlow.FlowStatus.LastServerResponse = DateTime.Now;
                                                                            currentFlow.FlowStatus.LastCollectionAttempt = DateTime.Now;

                                                                            onDocumentReceived.Invoke(this, newArgs);
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    // Error, the IP address is not set in the parameter for this feed.
                                                                    knownflow = true;
                                                                }
                                                            }
                                                            catch (Exception xyzzz)
                                                            {
                                                                knownflow = true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!knownflow)  // Check to see if flow is known, if not then we need to create one.
                                {
                                    sourceIP = incoming.ip.ToString();
                                    Boolean doIcreate = true;
                                    if (!PendingNewFeeds.Contains(sourceIP))  // Likewise, we might have lots of logs trigger before the flow is defined,
                                                                             // we're going to have to wait for the system to say we are ready to process
                                                                             // before actually collecting any more.
                                    {
                                        BaseParameter newFlowParameter = new BaseParameter();
                                        newFlowParameter.ExtractedMetadata = new Tree();
                                        newFlowParameter.ExtractedMetadata.setElement("Server", incoming.ip.ToString());
                                        newFlowParameter.Description = "Autogenerated UDP " + sourceIP;
                                        newFlowParameter.GroupID = State.config.GetProperty("AdministratorGroupID");
                                        newFlowParameter.OwnerID = State.config.GetProperty("AdministratorUserID");
                                        BaseInstance thisServer = BaseInstance.loadInstanceByUniqueID(State.managementDB, State.InstanceUniqueID);
                                        newFlowParameter.Name = thisServer.InstanceName + " UDP Syslog " + sourceIP + " Flow";
                                        BaseParameter.updateParameter(State.managementDB, newFlowParameter);

                                        BaseService service = BaseService.loadServiceByUniqueID(State.managementDB, State.config.GetProperty("UDPSysogSourceID"));

                                        PendingNewFeeds.Add(sourceIP);

                                        FlowEventArgs newFlow = new FlowEventArgs();
                                        newFlow.Flow = new BaseFlow();
                                        newFlow.Flow.FlowName = "UDP " + sourceIP;
                                        newFlow.Flow.ServiceID = service.UniqueID;
                                        newFlow.Flow.ProcessingEnabled = false;
                                        newFlow.Flow.RetainDocuments = true;
                                        newFlow.Flow.IndexString = true;
                                        newFlow.Flow.CollectionMethod = "static";
                                        newFlow.Flow.OwnerID = State.config.GetProperty("AdministratorUserID");
                                        newFlow.Flow.GroupID = State.config.GetProperty("AdministratorGroupID");
                                        newFlow.Flow.DateAdded = DateTime.Now.Ticks.ToString();
                                        newFlow.Flow.Description = "Auto-generated UDP port " + port.ToString() + " flow";
                                        newFlow.Flow.ParentService = BaseService.loadServiceByUniqueID(State.managementDB, newFlow.Flow.ServiceID);
                                        newFlow.Flow.ParentService.ParentSource = BaseSource.loadSourceByUniqueID(State.managementDB, newFlow.Flow.ParentService.SourceID);
                                        newFlow.Flow.ParameterID = newFlowParameter.UniqueID;
                                        newFlow.Flow.Parameter = newFlowParameter;
                                        newFlow.Flow.Parsing = "application/x-syslog";
                                        newFlow.Flow.meta_ipaddress = incoming.ip;
                                        newFlow.Flow.ControlState = "ready";
                                        newFlow.Flow.RuleGroupID = service.DefaultRuleGroup;
                                        BaseFlow.updateFlow(State.managementDB, newFlow.Flow);

                                        if (onFlowDetected != null)
                                        {
                                            onFlowDetected.Invoke(this, newFlow);
                                        }
                                    }
                                }
                            }
                            tmpList.Clear();
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                catch (Exception anotherExecption)
                {
                    running = false;
                }
            }
        }

        public void Stop()
        {
            if (active)
            {
                if (running)
                {
                    running = false;
                    try
                    {
                        udpListener.Close();
                        udpListener.Dispose();
                    }
                    catch (Exception xyz)
                    {
                        //if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Error: " + getReceiverType() + " Receiver failed to close."));
                    }
                }
                else
                {
                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already stopped."));
                }
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already disposed."));
            }
        }

        public void Dispose()
        {
            if (active)
            {
                active = false;
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to dispose, already disposed."));
            }
        }

        public void StartSuspend()
        {
            if (!suspended)
            {
                suspended = true;
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver for told to suspend, already suspended."));
            }
        }

        public void EndSuspend()
        {
            if (suspended)
            {
                suspended = false;
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver for told to end suspend, already running."));
            }
        }

        public Boolean isSuspended()
        {
            return suspended;
        }

        public Boolean isRunning()
        {
            return running;
        }

        public string getReceiverType()
        {
            return "UDP";
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;

            return newStatus;
        }

        public string getServiceID()
        {
            return ServiceID;
        }

        public void setServiceID(string serviceid)
        {
            ServiceID = serviceid;
        }

        public void MSPHeartBeat()
        {
            if (State != null)
            {
                foreach (BaseSource currentSource in State.Sources)
                {
                    if (currentSource.Enabled)
                    {
                        foreach (BaseService currentService in currentSource.Services)
                        {
                            if (currentService.Enabled)
                            {
                                if (currentService.ServiceType == "UDP" && currentService.ServiceSubtype == "Syslog")
                                {
                                    foreach (BaseFlow currentFlow in currentService.Flows)
                                    {
                                        if (currentFlow.FlowStatus != null)
                                        {
                                            currentFlow.FlowStatus.updateFlowPosition(State);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void registerFlow(BaseFlow flow)
        {

        }

        public void deregisterFlow(BaseFlow flow)
        {

        }
        public void reloadFlow(BaseFlow flow)
        {

        }
    }
}
