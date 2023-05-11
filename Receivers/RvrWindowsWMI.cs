//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Collections;
using FatumCore;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.ServiceModel.Syndication;
using System.Management;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Management.Instrumentation;

namespace PhlozLib
{
    public class RvrWindowsWMI : ReceiverInterface
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;

        CollectionState State = null;
        fatumconfig fatumConfig = null;

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onReceiverError;
        public EventHandler onStopped;
        public DocumentEventHandler onDocumentReceived;
        public FlowEventHandler onFlowDetected;
        ManagementEventWatcher Mew = null;

        Thread receiverThread = null;

        // Status Fields

        int garbageCollectionTimer = 0;
        long DocumentsReceived = 0;
        DateTime ReceiverStartTime = DateTime.Now;
        long DataTransferred = 0;
        string ServiceID;

        Boolean HeartBeatProcessing = false;
        public BaseFlow currentFlow = null;

        public RvrWindowsWMI(fatumconfig FC, CollectionState S)
        {
            fatumConfig = FC;
            State = S;
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
                    ReceiverStartTime = DateTime.Now;
                    foreach (BaseSource currentSource in State.Sources)
                    {
                        if (currentSource.Enabled)
                        {
                            foreach (BaseService currentService in currentSource.Services)
                            {
                                if (currentService.Enabled)
                                {
                                    if (currentService.ServiceType=="WMI" && currentService.ServiceSubtype == "Windows Event Logs")
                                    {
                                        foreach (BaseFlow currentFlow in currentService.Flows)
                                        {
                                            if (currentFlow.Enabled)
                                            {
                                                registerFlow(currentFlow);
                                            }
                                        }
                                    }
                                }
                            }
                        }
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

        public void Stop()
        {
            if (active)
            {
                if (running == false)
                {
                    running = false;
                    StopWatcher();
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
            return "wmievents";
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;
            newStatus.documents = DocumentsReceived;
            newStatus.transferred = DataTransferred;

            return newStatus;
        }

        private void StopWatcher()
        {
            if (Mew != null)
            {
                Mew.EventArrived -= new EventArrivedEventHandler(WMIEventArrived);
                Mew.Stop();
                Mew.Dispose();
            }
        }

        private void WMIEventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject newEvent = e.NewEvent;

            DocumentEventArgs newArgs = new DocumentEventArgs();
            newArgs.Document = new BaseDocument(currentFlow);
            newArgs.Document.FlowID = currentFlow.UniqueID;
            newArgs.Document.received = DateTime.Now;
            newArgs.Document.assignedFlow = currentFlow;
            newArgs.Document.Category = "Windows";
            newArgs.Document.Label = "WMI Event";
            Tree result = new Tree();

            foreach (PropertyData pd in newEvent.Properties)
            {
                ManagementBaseObject mbo;
                if ((mbo = pd.Value as ManagementBaseObject) != null)
                {
                    Tree subset = new Tree();
                    foreach (PropertyData prop in mbo.Properties)
                    {
                        try
                        {
                            if (prop.Value != null)
                            {
                                if (prop.Value.GetType() == typeof(byte[]))
                                {
                                    BinaryFormatter bf = new BinaryFormatter();
                                    using (var ms = new MemoryStream())
                                    {
                                        bf.Serialize(ms, prop.Value);
                                        subset.addElement(prop.Name, FatumLib.convertBytesTostring(ms.ToArray()));
                                    }
                                }
                                else
                                {
                                    subset.addElement(prop.Name, prop.Value.ToString());
                                }
                            }
                        }
                        catch (Exception xyz)
                        {
                            int abc = 1;
                        }
                    }
                    result.addNode(subset, "EventData");
                }
            }

            newArgs.Document.Document = TreeDataAccess.writeTreeToXMLString(result, "Event");
            result.dispose();

            if (currentFlow.FlowStatus != null)
            {
                currentFlow.FlowStatus.LastCollectionAttempt = DateTime.Now;
                currentFlow.FlowStatus.MostRecentData = DateTime.Now;
                currentFlow.FlowStatus.DocumentCount++;
                currentFlow.FlowStatus.BytesReceived += newArgs.Document.Document.Length;
            }
            if (onDocumentReceived != null) onDocumentReceived.Invoke(this, newArgs);
        }

        //[DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        //private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int
        //maximumWorkingSetSize);

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

            foreach (BaseSource currentSource in State.Sources)
            {
                if (currentSource.Enabled)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (currentService.Enabled)
                        {
                            if (currentService.ServiceType == "WMI" && currentService.ServiceSubtype == "Windows Event Logs")
                            {
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.Enabled && !(currentFlow.Suspended))
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

        public void registerFlow(BaseFlow flow)
        {
            BaseCredential creds = BaseCredential.loadCredentialByUniqueID(State.managementDB, flow.CredentialID);
            BaseParameter parms = BaseParameter.loadParameterByUniqueID(State.managementDB, flow.ParameterID);

            ManagementScope managementScope = null;
            EventQuery eventQuery = null;

            ConnectionOptions wmiAuthentication = new ConnectionOptions();
            wmiAuthentication.Password = creds.ExtractedMetadata.getElement("Password");
            wmiAuthentication.Username = creds.ExtractedMetadata.getElement("Account");
            wmiAuthentication.EnablePrivileges = true;
            wmiAuthentication.Authentication = AuthenticationLevel.PacketPrivacy;
            wmiAuthentication.Impersonation = ImpersonationLevel.Impersonate;

            string hostname = string.Format(@"\\{0}\root\cimv2", parms.ExtractedMetadata.getElement("Server"));
            managementScope = new ManagementScope(hostname, new ConnectionOptions());
            managementScope.Connect();

            switch (parms.ExtractedMetadata.getElement("QueryType"))
            {
                case "Windows Event Sources":
                    eventQuery = new EventQuery(@"SELECT * FROM __InstanceCreationEvent WHERE TargetInstance ISA 'Win32_NTLogEvent'");
                    break;
                case "CPU Percent Usage":
                    eventQuery = new EventQuery(@"SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor");
                    break;
                case "Drive IO Performance":
                    eventQuery = new EventQuery(@"SELECT PercentDiskTime, AvgDiskQueueLength, DiskReadBytesPerSec, DiskWriteBytesPerSec FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
                    break;
                case "Network Performance":
                    eventQuery = new EventQuery(@"SELECT Caption, BytesReceivedPerSec, BytesSentPerSec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface");
                    break;
                case "Memory Usage":
                    eventQuery = new EventQuery(@"SELECT Caption, CommittedBytes, AvailableBytes, PercentCommittedBytesInUse, PagesPerSec, PageFaultsPerSec FROM Win32_PerfFormattedData_PerfOS_Memory");
                    break;
                case "Custom":
                    eventQuery = new EventQuery(parms.ExtractedMetadata.getElement("WSQ"));
                    break;
            }

            Mew = new ManagementEventWatcher(managementScope, eventQuery);
            Mew.EventArrived += new EventArrivedEventHandler(WMIEventArrived);
            try
            {
                Mew.Start();
            }
            catch (Exception xyz)
            {
                 if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Error: " + getReceiverType() + " cannot start WMI collection: " + xyz.Message + ": " + xyz.StackTrace));
            }
            flow.ServiceObject = Mew;
        }

        public void deregisterFlow(BaseFlow flow)
        {
            if (flow.ServiceObject!=null)
            {
                ManagementEventWatcher Mew = (ManagementEventWatcher)flow.ServiceObject;
                Mew.Stop();
                Mew.Dispose();
                flow.ServiceObject = null;
            }
        }

        public void reloadFlow(BaseFlow flow)
        {
            deregisterFlow(flow);
            registerFlow(flow);
        }
    }
}
