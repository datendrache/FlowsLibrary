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

namespace PhlozLib
{
    public class RvrHTTP : ReceiverInterface
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        string ServiceID;

        CollectionState State = null;
        fatumconfig fatumConfig = null;

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onReceiverError;
        public EventHandler onStopped;
        public DocumentEventHandler onDocumentReceived;
        public FlowEventHandler onFlowDetected;

        Thread receiverThread = null;
        System.Timers.Timer HeartBeat = null;

        // Status Fields

        long DocumentsReceived = 0;
        DateTime ReceiverStartTime = DateTime.Now;
        long DataTransferred = 0;

        SyndicationFeed htmlService = null;

        Boolean HeartBeatProcessing = false;

        public RvrHTTP(fatumconfig FC, CollectionState S)
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
                                    if (currentService.ServiceSubtype == "HTTP")
                                    {
                                        foreach (BaseFlow currentFlow in currentService.Flows)
                                        {
                                            if (currentFlow.Enabled)
                                            {
                                                if (currentFlow.Interval == 0)
                                                {
                                                    currentFlow.Interval = 900;  // Default is 15 minute polls
                                                }
                                                currentFlow.intervalTicks = DateTime.MinValue;  // While we are at it, let's prime the pump for an immediate poll.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    receiverThread = new System.Threading.Thread(startReceiver);
                    receiverThread.Start();
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
                htmlService = new SyndicationFeed();
                HeartBeat = new System.Timers.Timer(1000);
                HeartBeat.Elapsed += new System.Timers.ElapsedEventHandler(HeartBeatCallBack);
                HeartBeat.Enabled = true;
            }
            catch (Exception xyz)
            {
                int i = 0;
            }
        }

        public void Stop()
        {
            if (active)
            {
                if (running == false)
                {
                    running = false;
                    HeartBeat.Enabled = false;
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
                HeartBeat.Enabled = false;
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
                HeartBeat.Enabled = true;
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
            return "HTTP";
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;
            newStatus.documents = DocumentsReceived;
            newStatus.transferred = DataTransferred;

            return newStatus;
        }

        public void collectHttpFlows()
        {
            ArrayList AllTweets = new ArrayList();

            foreach (BaseSource currentSource in State.Sources)
            {
                if (currentSource.Enabled)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (currentService.Enabled)
                        {
                            if (currentService.ServiceType == "HTTP")
                            {
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.Enabled && !(currentFlow.Suspended))
                                    {
                                        if ((currentFlow.intervalTicks.Ticks + (currentFlow.Interval * 10000000)) < DateTime.Now.Ticks)
                                        {                                                             
                                            currentFlow.intervalTicks = DateTime.Now;
                                            try
                                            {
                                                string data = "";
                                                using (WebClient client = new WebClient())
                                                {
                                                    data = client.DownloadString(currentFlow.UniqueID);
                                                }

                                                currentFlow.FlowStatus.FlowPosition = HttpGather(data, currentFlow, currentFlow.Parameter.ExtractedMetadata.getElement("URL"), currentFlow.FlowStatus.FlowPosition);
                                                BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                            }
                                            catch (Exception xyz)
                                            {
                                                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " HTTP flow \"" + currentFlow.UniqueID + "\" encountered error."));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private long HttpGather(string httpFlow, BaseFlow FLOW, string url, long last)
        {
            long result = last;

            processHttp(FLOW, httpFlow);

            return result;
        }

        private void processHttp(BaseFlow FLOW, string htmlDetails)
        {
            DocumentEventArgs newArgs = new DocumentEventArgs();
            newArgs.Document = new BaseDocument(FLOW);
            newArgs.Document.FlowID = FLOW.UniqueID;
            newArgs.Document.Document = htmlDetails;
            newArgs.Document.received = DateTime.Now;
            newArgs.Document.assignedFlow = FLOW;

            if (onDocumentReceived != null) onDocumentReceived.Invoke(this, newArgs);
        }

        private void HeartBeatCallBack(Object o, System.Timers.ElapsedEventArgs e)
        {
            if (running)
            {
                if (!HeartBeatProcessing)
                {
                    HeartBeatProcessing = true;

                    //  First step, lets process all the flows that were targets of forwarders

                    foreach (BaseSource currentSource in State.Sources)
                    {
                        if (currentSource.Enabled)
                        {
                            foreach (BaseService currentService in currentSource.Services)
                            {
                                if (currentService.Enabled)
                                {
                                    foreach (BaseFlow currentFlow in currentService.Flows)
                                    {
                                        if (currentFlow.Incoming.Count > 0)
                                        {
                                            lock (currentFlow.Incoming.SyncRoot)
                                            {
                                                int last = 0;

                                                for (int i = 0; i < currentFlow.Incoming.Count; i++)
                                                {
                                                    last = i;
                                                    BaseDocument document = (BaseDocument)currentFlow.Incoming[i];

                                                    document.FlowID = currentFlow.UniqueID;

                                                    processHttp(currentFlow, document.Document);
                                                }
                                                currentFlow.Incoming.RemoveRange(0, last);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //  Now lets process (once every five minutes) all polling network locations

                    try
                    {
                        collectHttpFlows();
                    }
                    catch (Exception)
                    {

                    }
                    HeartBeatProcessing = false;
                }
            }
            else
            {
                HeartBeat.Enabled = false;
            }
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
