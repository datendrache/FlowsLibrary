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
using TweetSharp;

namespace PhlozLib
{
    public class RvrAPI : ReceiverInterface
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        string ServiceID;

        CollectionState State = null;

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

        private Boolean HeartBeatProcessing = false;
        private Boolean collectionLocked = false;
        public BaseService apiService = null;
        public BaseCredential credential = null;

        public RvrAPI(fatumconfig FC, CollectionState S)
        {
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
                // INSERT API LAUNCH

                //twitterService = new TwitterService(twitService.Credentials.ExtractedMetadata.getElement("ConsumerKey"), twitService.Credentials.ExtractedMetadata.getElement("ConsumerKeySecret"));
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
                if (running == true)
                {
                    running = false;
                    HeartBeat.Enabled = false;
                }
                else
                {
                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already stopped."));
                    running = false;
                    HeartBeat.Enabled = false;
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
            return "Twitter";
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;
            newStatus.documents = DocumentsReceived;
            newStatus.transferred = DataTransferred;

            return newStatus;
        }

        public void collectAPI()
        {
            if (!collectionLocked)
            {
                collectionLocked = true;

                ArrayList AllElements = new ArrayList();

                foreach (BaseSource currentSource in State.Sources)
                {
                    if (currentSource.Enabled)
                    {
                        foreach (BaseService currentService in currentSource.Services)
                        {
                            if (currentService.Enabled)
                            {
                                if (currentService.ServiceSubtype == "API")
                                {
                                    foreach (BaseFlow currentFlow in currentService.Flows)
                                    {
                                        if (currentFlow.Suspended!=true)
                                        {
                                            try
                                            {
                                                if ((currentFlow.intervalTicks.Ticks + (currentFlow.Interval * 10000000)) < DateTime.Now.Ticks)
                                                {
                                                    currentFlow.intervalTicks = DateTime.Now;
                                                    string flowname = currentFlow.FlowName;
                                                    int count = 0;

                                                    currentFlow.FlowStatus.FlowPosition = Gather(currentFlow, currentFlow.FlowName, currentFlow.FlowStatus.FlowPosition);
                                                    BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                                }
                                            }
                                            catch (Exception xyz)
                                            {
                                                int abc = 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                collectionLocked = false;
            }
        }

        private long Gather(BaseFlow FLOW, string user, long last)
        {
            long result = last;
            FLOW.FlowStatus.LastCollectionAttempt = DateTime.Now;
            int count = 0;

            try
            {
                // USE WEB CLIENT TO ACCESS API AND RECEIVE RESULT

                Tree elements = new Tree();

                //  var tweets = twitterService.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { ScreenName = user, SinceId = last, Count = 100 });
                FLOW.FlowStatus.Requests++;
                FLOW.FlowStatus.LastServerResponse = DateTime.Now;

                if (elements != null)
                {
                    foreach (Tree currentElement in elements.tree)
                    {
                        processElement(FLOW, currentElement);
                        
                        count++;
                    }
                }
                FLOW.FlowStatus.DocumentCount += count;
                FLOW.FlowStatus.MostRecentData = DateTime.Now;
            }
            catch (Exception xyz)
            {
                FLOW.FlowStatus.Errors++;
            }

            if (count > 0)
            {
                FLOW.FlowStatus.DocumentCount += count;
                FLOW.FlowStatus.MostRecentData = DateTime.Now;
            }
            else
            {
                FLOW.FlowStatus.EmptySets++;
            }
            return result;
        }

       
        private void processElement(BaseFlow FLOW, Tree element)
        {
            DocumentEventArgs newArgs = new DocumentEventArgs();
            newArgs.Document = new BaseDocument(FLOW);

            newArgs.Document.FlowID = FLOW.UniqueID;
            newArgs.Document.Document = TreeDataAccess.writeTreeToXMLString(element,"Root");
            newArgs.Document.received = DateTime.Now;
            newArgs.Document.assignedFlow = FLOW;
            newArgs.Document.Metadata = element.Duplicate();
            
            if (onDocumentReceived != null) onDocumentReceived.Invoke(this, newArgs);
        }

        private void HeartBeatCallBack(Object o, System.Timers.ElapsedEventArgs e)
        {
            if (running)
            {
                if (!HeartBeatProcessing)
                {
                    HeartBeatProcessing = true;

                    foreach (BaseSource currentSource in State.Sources)
                    {
                        if (currentSource.Enabled)
                        {
                            foreach (BaseService currentService in currentSource.Services)
                            {
                                if (currentService.Enabled)
                                {
                                    if (currentService.ServiceSubtype == "API")
                                    {
                                        foreach (BaseFlow currentFlow in currentService.Flows)
                                        {
                                            if (currentFlow.Incoming != null)
                                            {
                                                if (currentFlow.Incoming.Count > 0)
                                                {
                                                    lock (currentFlow.Incoming.SyncRoot)
                                                    {
                                                        for (int i = 0; i < currentFlow.Incoming.Count; i++)
                                                        {
                                                            BaseDocument document = (BaseDocument)currentFlow.Incoming[i];
                                                            document.FlowID = currentFlow.UniqueID;
                                                            processElement(currentFlow, document.Metadata);
                                                        }
                                                        currentFlow.Incoming.Clear();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //  Now lets process all polling network locations

                    try
                    {
                        collectAPI();
                    }
                    catch (Exception xyz)
                    {

                    }

                    HeartBeatProcessing = false;
                }
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
                                if (currentService.ServiceSubtype == "API")
                                {
                                    foreach (BaseFlow currentFlow in currentService.Flows)
                                    {
                                        if (currentFlow.FlowStatus != null)
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
            }
        }

        public void registerFlow(BaseFlow flow)
        {
            flow.intervalTicks = DateTime.MinValue; // Start immediately.
        }

        public void deregisterFlow(BaseFlow flow)
        {
            foreach (BaseSource currentSource in State.Sources)
            {
                if (currentSource.Enabled)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (currentService.Enabled)
                        {
                            if (currentService.ServiceSubtype == "API")
                            {
                                int index = 0;
                                int found = -1;
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.UniqueID == flow.UniqueID)
                                    {
                                        flow.Suspended = true;
                                        flow.Enabled = false;
                                        found = index;
                                        break;
                                    }
                                    index++;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void reloadFlow(BaseFlow flow)
        {

        }
    }
}
