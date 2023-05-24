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

using MailKit;
using MailKit.Net.Pop3;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Exchange.WebServices.Data;
using System.Net;

namespace Proliferation.Flows
{
    public class RvrEmail : ReceiverInterface
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        String ServiceID = "";

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

        Boolean HeartBeatProcessing = false;

        public RvrEmail(fatumconfig FC, CollectionState S)
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
                                    if (currentService.ServiceSubtype == "Email")
                                    {
                                        foreach (BaseFlow currentFlow in currentService.Flows)
                                        {
                                            if (currentFlow.Enabled && !(currentFlow.Suspended))
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

            HeartBeat = new System.Timers.Timer(1000);
            HeartBeat.Elapsed += new System.Timers.ElapsedEventHandler(HeartBeatCallBack);
            HeartBeat.Enabled = true;
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
                    running = false;
                    HeartBeat.Enabled = false;
                }
            }
            else
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver told to stop, already disposed."));
                running = false;
                HeartBeat.Enabled = false;
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
            return "Email";
        }

        public void bindFlow(BaseFlow flow)
        {

        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;
            newStatus.documents = DocumentsReceived;
            newStatus.transferred = DataTransferred;

            return newStatus;
        }

        public void collectEmailFlows()
        {
            try
            {
                foreach (BaseSource currentSource in State.Sources)
                {
                    if (currentSource.Enabled)
                    {
                        if (currentSource.SourceName == "Email Source")
                        {
                            foreach (BaseService currentService in currentSource.Services)
                            {
                                if (currentService.Enabled)
                                {
                                    if (currentService.ServiceSubtype == "Email")
                                    {
                                        foreach (BaseFlow currentFlow in currentService.Flows)
                                        {
                                            if (currentFlow.Enabled && !(currentFlow.Suspended))
                                            {
                                                if ((currentFlow.intervalTicks.Ticks + (currentFlow.Interval * 10000000)) < DateTime.Now.Ticks)
                                                {
                                                    currentFlow.intervalTicks = DateTime.Now;
                                                    if (currentFlow.Parameter != null)
                                                    {
                                                        try
                                                        {
                                                            BaseCredential creds = BaseCredential.loadCredentialByUniqueID(State.managementDB, currentFlow.CredentialID);

                                                            // IMAP

                                                            if (currentFlow.Parameter.ExtractedMetadata.GetElement("Protocol").ToLower() == "imap")
                                                            {
                                                                using (var emailClient = new ImapClient())
                                                                {
                                                                    int port = int.Parse(currentFlow.Parameter.ExtractedMetadata.GetElement("Port"));
                                                                    Boolean useSSL = false;
                                                                    if (currentFlow.Parameter.ExtractedMetadata.GetElement("Encryption").ToLower() == "true")
                                                                    {
                                                                        useSSL = true;
                                                                    }
                                                                    emailClient.Connect(currentFlow.Parameter.ExtractedMetadata.GetElement("Server"), port, useSSL);
                                                                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                                                                    emailClient.Authenticate(creds.ExtractedMetadata.GetElement("Account"), creds.ExtractedMetadata.GetElement("Password"));

                                                                    emailClient.Inbox.Open(FolderAccess.ReadWrite);

                                                                    var uids = emailClient.Inbox.Search(SearchQuery.All);

                                                                    foreach (var uid in uids)
                                                                    {
                                                                        StreamReader document = new StreamReader(emailClient.Inbox.GetStream(uid, 0, 20971520));
                                                                        processEmail(currentFlow, document.ReadToEnd());
                                                                        emailClient.Inbox.AddFlags(uid, MessageFlags.Deleted, true);
                                                                    }

                                                                    emailClient.Disconnect(true);
                                                                }
                                                            }

                                                            // POP3

                                                            if (currentFlow.Parameter.ExtractedMetadata.GetElement("Protocol").ToLower() == "pop3")
                                                            {
                                                                Pop3Client emailClient = new Pop3Client();

                                                                int port = int.Parse(currentFlow.Parameter.ExtractedMetadata.GetElement("Port"));
                                                                Boolean useSSL = false;
                                                                if (currentFlow.Parameter.ExtractedMetadata.GetElement("Encryption").ToLower() == "true")
                                                                {
                                                                    useSSL = true;
                                                                }
                                                                emailClient.Connect(currentFlow.Parameter.ExtractedMetadata.GetElement("Server"), port, useSSL);
                                                                emailClient.Authenticate(creds.ExtractedMetadata.GetElement("Account"), creds.ExtractedMetadata.GetElement("Password"));

                                                                // Keep downloading until we're done.

                                                                while (emailClient.Count > 0)
                                                                {
                                                                    StreamReader document = new StreamReader(emailClient.GetStream(0));
                                                                    processEmail(currentFlow, document.ReadToEnd());
                                                                    emailClient.DeleteMessage(0);
                                                                }

                                                                emailClient.Disconnect(true);
                                                            }

                                                            // Microsoft Exchange

                                                            if (currentFlow.Parameter.ExtractedMetadata.GetElement("Protocol").ToLower() == "microsoft exchange")
                                                            {
                                                                ExchangeService service;
                                                                switch (currentFlow.Parameter.ExtractedMetadata.GetElement("Protocol").ToLower())
                                                                {
                                                                    case "exchange 2007 sp1":
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                                                                        break;
                                                                    case "exchange 2010":
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2010);
                                                                        break;
                                                                    case "exchange 2010 sp1":
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2010_SP1);
                                                                        break;
                                                                    case "exchange 2013":
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2013);
                                                                        break;
                                                                    case "exchange 2013 sp1":
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
                                                                        break;
                                                                    case "exchange 2016":
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
                                                                        break;
                                                                    default:
                                                                        service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
                                                                        break;
                                                                }

                                                                service.Credentials = new NetworkCredential(creds.ExtractedMetadata.GetElement("Account"), creds.ExtractedMetadata.GetElement("Password"), currentFlow.Parameter.ExtractedMetadata.GetElement("Domain"));
                                                                service.AutodiscoverUrl(creds.ExtractedMetadata.GetElement("Account"));

                                                                FindItemsResults<Item> findResults = service.FindItems(
                                                                   WellKnownFolderName.Inbox,
                                                                   new ItemView(1000)
                                                                ).Result;

                                                                foreach (Item item in findResults.Items)
                                                                {
                                                                    processEmail(currentFlow, item.TextBody);
                                                                    item.Delete(DeleteMode.HardDelete);
                                                                }
                                                            }
                                                        }
                                                        catch (Exception xyz)
                                                        {
                                                            if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " flow \"" + currentFlow.UniqueID + "\" encountered error."));
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
            }
            catch (Exception xyz)
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Error: " + getReceiverType() + " encountered error."));
            }
        }

        private void processEmail(BaseFlow FLOW, string document)
        {
            DocumentEventArgs newArgs = new DocumentEventArgs();
            newArgs.Document = new BaseDocument(FLOW);
            newArgs.Document.FlowID = FLOW.UniqueID;
            newArgs.Document.Document = document;
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

                    foreach (BaseSource currentSource in State.Sources)
                    {
                        if (currentSource.Enabled)
                        {
                            foreach (BaseService currentService in currentSource.Services)
                            {
                                if (currentService.Enabled)
                                {
                                    if (currentService.ServiceSubtype == "Email")
                                    {
                                        foreach (BaseFlow current in currentService.Flows)
                                        {
                                            if (current.Incoming.Count > 0)
                                            {
                                                lock (current.Incoming.SyncRoot)
                                                {
                                                    int last = 0;
                                                    for (int x = 0; x < current.Incoming.Count; x++)
                                                    {
                                                        last = x;
                                                        BaseDocument document = (BaseDocument)current.Incoming[x];
                                                        document.FlowID = current.UniqueID;
                                                        processEmail(current, document.Document);
                                                    }
                                                    current.Incoming.RemoveRange(0, last);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        collectEmailFlows();
                    }
                    catch (Exception xyz)
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