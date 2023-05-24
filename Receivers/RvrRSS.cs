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

using Proliferation.Fatum;
using System.Net;
using System.ServiceModel.Syndication;
using System.Globalization;

namespace Proliferation.Flows
{
    public class RvrRSS : ReceiverInterface
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

        public string ServiceID = "";

        Thread receiverThread = null;
        System.Timers.Timer HeartBeat = null;

        // Status Fields

        long DocumentsReceived = 0;
        DateTime ReceiverStartTime = DateTime.Now;
        long DataTransferred = 0;

        SyndicationFeed rssService = null;

        Boolean HeartBeatProcessing = false;


        public RvrRSS(fatumconfig FC, CollectionState S)
        {
            fatumConfig = FC;
            State = S;
        }

        public void setCallbacks(DocumentEventHandler documentEventHandler,
    ErrorEventHandler errorEventHandler,
    EventHandler communicationLost,
    EventHandler stoppedReceiver,
    FlowEventHandler flowEventHandler)
        {
            onDocumentReceived = new DocumentEventHandler(documentEventHandler);
            onReceiverError = new ErrorEventHandler(errorEventHandler);
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
                                    if (currentService.ServiceSubtype == "RSS")
                                    {
                                        foreach (BaseFlow currentFlow in currentService.Flows)
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
                rssService = new SyndicationFeed();
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
                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already disposed."));
                    running = false;
                    HeartBeat.Enabled = false;
                }
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already disposed."));
                HeartBeat.Enabled = false;
                running = false;
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
            return "RSS";
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;
            newStatus.documents = DocumentsReceived;
            newStatus.transferred = DataTransferred;

            return newStatus;
        }

        public void collectRssFlows()
        {
            foreach (BaseSource currentSource in State.Sources)
            {
                if (currentSource.Enabled)
                {
                    foreach (BaseService currentService in currentSource.Services)
                    {
                        if (currentService.Enabled)
                        {
                            if (currentService.UniqueID == ServiceID)
                            {
                                foreach (BaseFlow currentFlow in currentService.Flows)
                                {
                                    if (currentFlow.ParentService.UniqueID == ServiceID)
                                    {
                                        if (currentFlow.Enabled && !(currentFlow.Suspended))
                                        {
                                            if ((currentFlow.intervalTicks.Ticks + (currentFlow.Interval * 10000000)) < DateTime.Now.Ticks)
                                            {
                                                currentFlow.intervalTicks = DateTime.Now;
                                                try
                                                {
                                                    currentFlow.FlowStatus.LastCollectionAttempt = DateTime.Now;
                                                    var myClient = new WebClient();
                                                    StreamReader response = new StreamReader(myClient.OpenRead(currentFlow.Parameter.ExtractedMetadata.GetElement("URI")));
                                                    String content = response.ReadToEnd();
                                                    Tree flowdata = null;
                                                    currentFlow.FlowStatus.BytesReceived += content.Length;
                                                    try
                                                    {
                                                        flowdata = XMLTree.ReadXmlFromString(content);
                                                        RssGather(flowdata, currentFlow);
                                                    }
                                                    catch (Exception xyz)
                                                    {
                                                        currentFlow.FlowStatus.Errors++;
                                                    }

                                                    //currentFlow.FlowStatus.FlowPosition = RssGather(rss, currentFlow, currentFlow.Parameter.ExtractedMetadata.GetElement("URL"), currentFlow.FlowStatus.FlowPosition);
                                                    BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                                }
                                                catch (Exception xyz)
                                                {
                                                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " RSS flow \"" + currentFlow.UniqueID + "\" encountered error: " + xyz.Message + "\r\n" + xyz.StackTrace));
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
        }

        private long RssGather(Tree rssdata, BaseFlow FLOW)
        {
            long result = FLOW.FlowStatus.FlowPosition;
            Boolean isEmptySet = true;

            if (rssdata.tree.Count>0)
            {
                try
                {
                    //  Figure out what type of structure this is...
                    Boolean depthloop = true;
                    Tree channel = rssdata;
                    Tree channelroot = rssdata;

                    while (depthloop)   // could call this Depth Spiral....
                    {
                        if (channel.FindNode("channel")!=null)
                        {
                            channelroot = channel;
                            channel = channel.FindNode("channel");
                            depthloop = false;
                        }
                        else
                        {
                            if (channel.tree.Count==1)
                            {
                                channelroot = channel;
                                channel = (Tree)channel.tree[0];
                            }
                            else
                            {
                                depthloop = false;
                                channel = null;
                            }
                        }
                    }

                    if (channel != null)   // We've found the channel. w00t.
                    {
                        Boolean updateFlow = false;
                        string title = channel.GetElement("Title");
                        string Description = channel.GetElement("Description");
                        string Link = channel.GetElement("Link");
                        string PubDate = channel.GetElement("PubDate");
                        string SkipHours = channel.GetElement("Skip Hours");
                        string ShipDays = channel.GetElement("Skip Days");
                        string Language = channel.GetElement("Language");
                        string Category = channel.GetElement("Category");
                        string Cloud = channel.GetElement("Cloud");
                        string Webmaster = channel.GetElement("Webmaster");
                        string Editor = channel.GetElement("Editor");
                        string Copyright = channel.GetElement("Copyright");
                        string Rating = channel.GetElement("Rating");
                        string Docs = channel.GetElement("Docs");
                        string TTL = channel.GetElement("TTL");

                        long newlatest = FLOW.FlowStatus.FlowPosition;

                        for (int i = 0; i < channel.leafnames.Count; i++)
                        {
                            string segment = (string)channel.leafnames[i];
                            if (segment.ToLower() == "item")
                            {
                                long isNewer = processRss(FLOW, (Tree)channel.tree[i], ref isEmptySet);
                                if (isNewer > newlatest)
                                {
                                    newlatest = isNewer;
                                }
                            }
                        }

                        for (int i = 0; i < channelroot.leafnames.Count; i++)
                        {
                            string segment = (string)channelroot.leafnames[i];
                            if (segment.ToLower() == "item")
                            {
                                long isNewer = processRss(FLOW, (Tree)channelroot.tree[i], ref isEmptySet);
                                if (isNewer > newlatest)
                                {
                                    newlatest = isNewer;
                                }
                            }
                        }

                        if (newlatest>0)
                        {
                            FLOW.FlowStatus.FlowPosition = newlatest;
                        }
                    }
                    else
                    {
                        FLOW.FlowStatus.Errors++;
                    }
                }
                catch (Exception xyz)
                {
                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + "\" encountered RSS parsing error: " + xyz.Message + "\r\n" + xyz.StackTrace));
                }
            }

            if (isEmptySet)
            {
                FLOW.FlowStatus.EmptySets++;
            }
            return result;
        }

        private long processRss(BaseFlow FLOW, Tree rssDetails, ref Boolean isEmptySet)
        {
            long result = 0;

            try
            {
                Boolean legit = false;
                DateTime flowTime = DateTime.MinValue;

                string arrival = rssDetails.GetElement("pubDate");

                if (arrival == "")
                {
                    arrival = rssDetails.GetElement("dc:date");

                    if (arrival != "")
                    {
                        flowTime = Convert.ToDateTime(arrival);
                        legit = true;
                    }
                }
                else
                {
                    flowTime = DateTime.ParseExact(arrival, "ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
                    legit = true;
                }

                if (legit)
                {
                    if (flowTime.Ticks > FLOW.FlowStatus.FlowPosition)
                    {
                        result = flowTime.Ticks;
                        string xml = TreeDataAccess.WriteTreeToXmlString(rssDetails, "RSS");

                        DocumentEventArgs newArgs = new DocumentEventArgs();
                        newArgs.Document = new BaseDocument(FLOW);
                        newArgs.Document.FlowID = FLOW.UniqueID;
                        newArgs.Document.Document = xml;
                        newArgs.Document.received = DateTime.Now;
                        newArgs.Document.assignedFlow = FLOW;

                        if (onDocumentReceived != null) onDocumentReceived.Invoke(this, newArgs);
                        isEmptySet = false;
                    }
                }
            }
            catch (Exception xyz)
            {
                int notastoryIguess = 1;
            }

            return result;
        }

        private void HeartBeatCallBack(Object o, System.Timers.ElapsedEventArgs e)
        {
            if (running)
            {
                if (!HeartBeatProcessing)
                {
                    HeartBeatProcessing = true;
                    collectRssFlows();
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
