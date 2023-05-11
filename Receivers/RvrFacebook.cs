//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using Facebook;
using FatumCore;

namespace PhlozLib
{
    public class RvrFacebook : ReceiverInterface
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        string ServiceID;
        ArrayList FlowList = new ArrayList();

        CollectionState State = null;
        FatumCore.fatumconfig fatumConfig = null;

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

        FacebookClient facebookClient = null;
        Boolean HeartBeatProcessing = false;
        BaseService twitService = null;
        private Boolean ServerAuthenticated = false;

        public RvrFacebook(FatumCore.fatumconfig FC, CollectionState S)
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
                                    if (currentService.ServiceSubtype == "Facebook User Service")
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
            return "Facebook";
        }

        public void bindFlow(BaseFlow flow)
        {
            lock (FlowList.SyncRoot)
            {
                FlowList.Add(flow);
            }
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;
            newStatus.documents = DocumentsReceived;
            newStatus.transferred = DataTransferred;

            return newStatus;
        }

        public void collectFacebookFlows()
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
                            if (currentService.ServiceSubtype == "Facebook User Source")
                            {
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.Parameter != null)
                                    {
                                        if ((currentFlow.intervalTicks.Ticks + (currentFlow.Interval * 10000000)) < DateTime.Now.Ticks)
                                        {
                                            currentFlow.intervalTicks = DateTime.Now;

                                            if (currentFlow.FlowStatus.FlowPosition <= 0)
                                            {
                                                facebookClient = new FacebookClient();
                                                facebookClient.AppId = currentFlow.ParentService.Credentials.ExtractedMetadata.getElement("ApplicationID");
                                                facebookClient.AppSecret = currentFlow.ParentService.Credentials.ExtractedMetadata.getElement("SecretKey");
                                                facebookClient.AccessToken = currentFlow.ParentService.Credentials.ExtractedMetadata.getElement("Token");

                                                currentFlow.FlowStatus.FlowPosition = FacebookGetUserFlowPosition(currentFlow.Parameter.ExtractedMetadata.getElement("Account"));
                                                BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                                facebookClient = null;
                                            }
                                            else
                                            {
                                                facebookClient = new FacebookClient();
                                                facebookClient.AppId = currentFlow.ParentService.Credentials.ExtractedMetadata.getElement("ApplicationID");
                                                facebookClient.AppSecret = currentFlow.ParentService.Credentials.ExtractedMetadata.getElement("SecretKey");
                                                facebookClient.AccessToken = currentFlow.ParentService.Credentials.ExtractedMetadata.getElement("Token");

                                                currentFlow.FlowStatus.FlowPosition = FacebookGatherUser(currentFlow, currentFlow.Parameter.ExtractedMetadata.getElement("Account"), currentFlow.FlowStatus.FlowPosition);
                                                BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                                facebookClient = null;
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
              //Use ListTweetsOnUserTimeline to get all tweets from user 
        private long FacebookGetUserFlowPosition(string user)
        {
            long result = 0;

            try
            {
                dynamic posts = facebookClient.Get("/me/feed");
                foreach (var item in posts)
                {
                    var post = (KeyValuePair<string, object>)item;
                    System.Console.Out.Write("test");
                    //string postid = post["id"].ToString();

                    //long postlong = 0;
                    //long.TryParse(postid, out postlong);
                    //if (postlong > result)
                    //{
                    //    result = postlong;
                    //}
                }
            }
            catch (Exception xyz)
            {
                System.Console.Out.Write(xyz.Message);
            }
            return result;
        }

        private long FacebookGatherUser(BaseFlow FLOW, string user, long last)
        {
            long result = last;
            long updated = result;
            try
            {
                dynamic posts = facebookClient.Get("/me/feed");
                foreach (var item in posts)
                {
                    var post = (KeyValuePair<string, object>)item;
                    System.Console.Out.Write("test");
                    //string postid = post["id"].ToString();

                    //long postlong = 0;
                    //long.TryParse(postid, out postlong);
                    //if (postlong > last)
                    //{
                    //    updated = postlong;
                    //}
                }
            }
            catch (Exception xyz)
            {
                System.Console.Out.Write(xyz.Message);
            }
            return updated;
        }

        private void processPost(BaseFlow FLOW, string tweet)
        {
            DocumentEventArgs newArgs = new DocumentEventArgs();
            newArgs.Document = new BaseDocument(FLOW);
            newArgs.Document.FlowID = FLOW.UniqueID;
            newArgs.Document.Document = tweet;
            newArgs.Document.received = DateTime.Now;
            newArgs.Document.assignedFlow = FLOW;

            switch (FLOW.ParentService.ServiceSubtype)
            {
                case "Facebook User Source":
                    {
                        newArgs.Document.FlowID = FLOW.UniqueID;
                    } break;
            }

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
                                    if (currentService.ServiceType == "UDP" && currentService.ServiceSubtype == "Syslog")
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

                                                        processPost(currentFlow, document.Document);
                                                    }
                                                    currentFlow.Incoming.RemoveRange(0, last);
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
                        collectFacebookFlows();
                    }
                    catch (Exception)
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
