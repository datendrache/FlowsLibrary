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
using System.Net;
using System.Net.Sockets;

namespace Proliferation.Flows
{
    public class RvrTCPXML : ReceiverInterface 
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        int port = 0;
        public static ArrayList FlowList = new ArrayList();
        string ServiceID;

        CollectionState State = null;

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onReceiverError;
        public EventHandler onStopped;
        public DocumentEventHandler onDocumentReceived;
        public FlowEventHandler onFlowDetected;

        Thread receiverThread = null;

        ArrayList Buffer = new ArrayList(5000);

        // Status Fields

        DateTime ReceiverStartTime = DateTime.Now;
        ArrayList ConnectedClients = new ArrayList();

        public string LocalIpAddress = "127.0.0.1";

        public RvrTCPXML(CollectionState S, int Port)
        {
            State = S;
            port = Port;
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
                    if ((port <= 0) || (port > 65535))
                    {
                        if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + "Invalid TCP port provided for flow."));
                    }
                    else
                    {
                        ReceiverStartTime = DateTime.Now;
                        receiverThread = new System.Threading.Thread(startReceiver);
                        receiverThread.Start();
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
            try
            {
                IPAddress localAddr = IPAddress.Parse(LocalIpAddress);

                // TcpListener server = new TcpListener(port);
                TcpListener serverSocket = new TcpListener(localAddr, port);
                TcpClient clientSocket = default(TcpClient);

                // Start listening for client requests.
                running = true;
                serverSocket.Start();

                while (running)
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                    TCPXMLHandler client = new TCPXMLHandler();
                    client.onConnectionLost += new EventHandler(onClientDisconnect);
                    client.onDocument += new DocumentEventHandler(onDocumentArrived);
                    client.startClient(State, clientSocket);
                    ConnectedClients.Add(client);
                }

                clientSocket.Close();
                serverSocket.Stop();
                for (int i = 0; i < ConnectedClients.Count; i++)
                {
                    TCPEventStreamHandler current = (TCPEventStreamHandler)ConnectedClients[i];
                    current.Running = false;
                    current.Queue.Clear();
                }
            }
            catch (Exception xyz)
            {
                if (onReceiverError != null) onReceiverError.Invoke(this, new ErrorEventArgs("Warning: " + getReceiverType() + " Receiver failed to initialize."));
                onStopped.Invoke(this, new EventArgs());
            }
        }

        public void Stop()
        {
            if (active)
            {
                if (running)
                {
                    running = false;
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

        public Boolean FlowCheck(BaseFlow flow)
        {
            Boolean result = false;
            if (flow.Parameter != null)
            {
                if (flow.ParentService.ServiceType == "TCPXML")
                {
                    int tmpFlowPort = -1;
                    int.TryParse(flow.Parameter.ExtractedMetadata.GetElement("Port"), out tmpFlowPort);

                    if (port == tmpFlowPort)
                    {
                        result = true;
                    }
                }
            }
            return result;
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
            return "TCPXML";
        }

        public ReceiverStatus getStatus()
        {
            ReceiverStatus newStatus = new ReceiverStatus();

            newStatus.StartTime = ReceiverStartTime;

            return newStatus;
        }
        private void serverThreadProcess()
        {
            
        }

        private void onDocumentArrived(object o, DocumentEventArgs e)
        {
            if (onDocumentReceived != null) onDocumentReceived.Invoke(this, e);
        }

        private void onClientDisconnect(object o, EventArgs e)
        {
            TCPEventStreamHandler lostConnection = (TCPEventStreamHandler)o;
            ConnectedClients.Remove(lostConnection);
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
