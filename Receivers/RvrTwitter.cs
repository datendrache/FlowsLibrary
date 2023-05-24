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
using Proliferation.Fatum;
using TweetSharp;

namespace Proliferation.Flows
{
    public class RvrTwitter : ReceiverInterface
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

        TwitterService twitterService = null;

        public BaseService twitService = null;

        private Boolean ServerAuthenticated = false;
        private Boolean HeartBeatProcessing = false;
        private Boolean collectionLocked = false;

        public BaseCredential credential = null;

        public string TwitterRequestTokenURL = "https://api.twitter.com/oauth/request_token";
        public string TwitterAuthorizeURL = "https://api.twitter.com/oauth/authorize";
        public string TwitterAccessTokenURL = "https://api.twitter.com/oauth/access_token";
        public string TwitterAppOnlyAuthentication = "https://api.twitter.com/oauth2/token";

        public RvrTwitter(fatumconfig FC, CollectionState S)
        {
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
                twitterService = new TwitterService(twitService.Credentials.ExtractedMetadata.GetElement("ConsumerKey"), twitService.Credentials.ExtractedMetadata.GetElement("ConsumerKeySecret"));
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
                    twitterService.CancelStreaming();
                }
                else
                {
                    if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already stopped."));
                    running = false;
                    HeartBeat.Enabled = false;
                    twitterService.CancelStreaming();
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

        public void collectTwitterFlows()
        {
            if (!collectionLocked)
            {
                collectionLocked = true;

                if (!ServerAuthenticated)
                {
                    twitterService.AuthenticateWith(twitService.Credentials.ExtractedMetadata.GetElement("ConsumerKey"), twitService.Credentials.ExtractedMetadata.GetElement("ConsumerKeySecret"), twitService.Credentials.ExtractedMetadata.GetElement("AccessToken"), twitService.Credentials.ExtractedMetadata.GetElement("AccessTokenSecret"));
                    ServerAuthenticated = true;
                }

                ArrayList AllTweets = new ArrayList();

                foreach (BaseSource currentSource in State.Sources)
                {
                    if (currentSource.Enabled)
                    {
                        foreach (BaseService currentService in currentSource.Services)
                        {
                            if (currentService.Enabled)
                            {
                                if (currentService.ServiceSubtype == "Twitter Service")
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
                                                    if (flowname[0] == '#')   // HASHTAG
                                                    {
                                                        if (currentFlow.FlowStatus.FlowPosition <= 0)
                                                        {
                                                            currentFlow.FlowStatus.FlowPosition = TwitterGetHashTagFlowPosition(currentFlow, currentFlow.FlowName);
                                                        }
                                                        else
                                                        {
                                                            count = TwitterGatherHashTag(currentFlow, currentFlow.FlowName, currentFlow.FlowStatus.FlowPosition);
                                                        }
                                                    }
                                                    else
                                                    {  // USER
                                                        if (currentFlow.FlowStatus.FlowPosition <= 0)
                                                        {
                                                            currentFlow.FlowStatus.FlowPosition = TwitterGetUserFlowPosition(currentFlow, currentFlow.FlowName);
                                                            BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                                        }
                                                        else
                                                        {
                                                            currentFlow.FlowStatus.FlowPosition = TwitterGatherUser(currentFlow, currentFlow.FlowName, currentFlow.FlowStatus.FlowPosition);
                                                            BaseFlow.updateFlowPosition(State, currentFlow, currentFlow.FlowStatus.FlowPosition);
                                                        }
                                                    }
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
              //Use ListTweetsOnUserTimeline to get all tweets from user 
        private long TwitterGetUserFlowPosition(BaseFlow FLOW, string user)
        {
            long result = 0;

            var tweets = twitterService.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { ScreenName = user, Count = 1 });
            FLOW.FlowStatus.Requests++;
            FLOW.FlowStatus.LastServerResponse = DateTime.Now;
            int count = 0;
            if (tweets != null)
            {
                foreach (var tweet in tweets)
                {
                    count++;
                    if (tweet.Id > result) result = tweet.Id;
                    FLOW.FlowStatus.BytesReceived += tweet.RawSource.Length * 2;   // We multiply x2 because UTF-16
                }
            }

            if (count == 0)
            {
                FLOW.FlowStatus.EmptySets++;
            }

            return result;
        }

        private long TwitterGatherUser(BaseFlow FLOW, string user, long last)
        {
            long result = last;
            FLOW.FlowStatus.LastCollectionAttempt = DateTime.Now;
            int count = 0;

            try
            {
                var tweets = twitterService.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { ScreenName = user, SinceId = last, Count = 100 });
                FLOW.FlowStatus.Requests++;
                FLOW.FlowStatus.LastServerResponse = DateTime.Now;

                if (tweets != null)
                {
                    foreach (var tweet in tweets)
                    {
                        processTweet(FLOW, tweet.RawSource);
                        FLOW.FlowStatus.BytesReceived += tweet.RawSource.Length * 2;   // We multiply x2 because UTF-16
                        if (tweet.Id > result) result = tweet.Id;
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

        //Use Search option to get specific hash tag from twitter 
        private long TwitterGetHashTagFlowPosition(BaseFlow FLOW, string hashtag)
        {
            int count = 0;
            long result = 0;
            try
            {
                var options = new SearchOptions { Q = hashtag, Count = 1 };
                
                TwitterSearchResult tweets = twitterService.Search(options);
                FLOW.FlowStatus.LastServerResponse = DateTime.Now;
                FLOW.FlowStatus.Requests++;
                if (tweets != null)
                {
                    List<TweetSharp.TwitterStatus> tweetlist = tweets.Statuses.ToList();
                    foreach (var tweet in tweetlist)
                    {
                        count++;
                        if (tweet.Id > result)
                        {
                            result = tweet.Id;
                        }
                    }
                }
            }
            catch (Exception xyz)
            {
                int abc = 1;
            }

            if (count == 0)
            {
                FLOW.FlowStatus.EmptySets++;
            }
            return result;
        }

        private int TwitterGatherHashTag(BaseFlow FLOW, string hashtag, long last)
        {
            int count = 0;

            FLOW.FlowStatus.LastCollectionAttempt = DateTime.Now;

            try
            {
                var options = new SearchOptions { Q = hashtag, SinceId = last, Count = 100 };
                FLOW.FlowStatus.Requests++;

                TwitterSearchResult tweets = twitterService.Search(options);

                if (tweets!=null)
                {
                    if (tweets.RawSource!=null)
                    {
                        FLOW.FlowStatus.BytesReceived += tweets.RawSource.Length * 2;   // We multiply x2 because UTF-16
                        FLOW.FlowStatus.LastServerResponse = DateTime.Now;
                        Tree afterjson = TreeDataAccess.ReadJsonFromString(tweets.RawSource);

                        try
                        {
                            for (int i = 0; i < afterjson.leafnames.Count; i++)
                            {
                                if ((string)afterjson.leafnames[i] == "search_metadata")
                                {
                                    Tree metrics = (Tree)afterjson.tree[i];
                                    string currentposition = metrics.GetElement("max_id");
                                    FLOW.FlowStatus.FlowPosition = long.Parse(currentposition);
                                    string processingDuration = metrics.GetElement("completed_in");
                                    double duration = 0;
                                    if (double.TryParse(processingDuration, out duration))
                                    {
                                        FLOW.FlowStatus.CollectionDuration = FLOW.FlowStatus.CollectionDuration.AddSeconds(duration);
                                    }
                                }
                                else
                                {
                                    Tree tweet = (Tree)afterjson.tree[i];
                                    processTweet(FLOW, TreeDataAccess.WriteJsonToString(tweet, "Tweet"));
                                    count++;
                                }
                            }
                        }
                        catch (Exception xyz)
                        {
                            int xyzzy = 0;
                        }
                        afterjson.Dispose();
                        tweets.RawSource = "";
                    }
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
            }
            catch (Exception servererror)
            {
                FLOW.FlowStatus.Errors++;
            }
            return count;
        }

        private void processTweet(BaseFlow FLOW, string tweet)
        {
            DocumentEventArgs newArgs = new DocumentEventArgs();
            newArgs.Document = new BaseDocument(FLOW);

            newArgs.Document.FlowID = FLOW.UniqueID;
            newArgs.Document.Document = tweet;
            newArgs.Document.received = DateTime.Now;
            newArgs.Document.assignedFlow = FLOW;
            try
            {
                Tree meta = TreeDataAccess.ReadJsonFromString(tweet);
                newArgs.Document.Metadata = meta;
            }
            catch (Exception xyz)
            {
                int y = 0;
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

                    foreach (BaseSource currentSource in State.Sources)
                    {
                        if (currentSource.Enabled)
                        {
                            foreach (BaseService currentService in currentSource.Services)
                            {
                                if (currentService.Enabled)
                                {
                                    if (currentService.ServiceSubtype == "Twitter Service")
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
                                                            processTweet(currentFlow, document.Document);
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
                        collectTwitterFlows();
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
                                if (currentService.ServiceSubtype == "Twitter Service")
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
                            if (currentService.ServiceSubtype == "Twitter Service")
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
