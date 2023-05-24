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
    public class FwdEventStream : ForwarderInterface
    {
        TcpClient output = null;
        NetworkStream outstream = null; 

        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        BaseForwarder assignedForwarder = null;

        ArrayList SuspendedQueue = new ArrayList();
        ArrayList ConnectedClients = new ArrayList();

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onForwarderError;
        private Thread serverThread = null;

        public FwdEventStream(BaseForwarder forwarder)
        {
            assignedForwarder = forwarder;
        }

        public void Start()
        {
            if (active)
            {
                if (!running)
                {
                    serverThread = new Thread(serverThreadProcess);
                    serverThread.Start();
                    running = true;
                }
                else
                {
                    if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Warning: Forwarder for " + assignedForwarder.forwarderName + " told to start, already running."));
                }
            }
            else
            {
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " told to start, already disposed."));
            }
        }

        public void Stop()
        {
            if (active)
            {
                if (running == false)
                {
                    running = false;
                    if (output != null)
                    {
                        output.Close();
                        
                        output = null;
                    }
                    else
                    {
                        if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " told to stop, communications object is NULL."));
                    }
                }
                else
                {
                    if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Warning: Forwarder for " + assignedForwarder.forwarderName + " told to stop, already stopped."));
                }
            }
            else
            {
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Warning: Forwarder for " + assignedForwarder.forwarderName + " told to stop, already disposed."));
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
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Warning: Forwarder for " + assignedForwarder.forwarderName + " told to dispose, already disposed."));
            }
        }

        public Boolean sendDocument(BaseDocument Document)
        {
            Boolean result = false;

            if (active)
            {
                try
                {
                    if (!suspended)
                    {
                        for (int i = 0; i < ConnectedClients.Count; i++)
                        {
                            TCPEventStreamHandler current = (TCPEventStreamHandler)ConnectedClients[i];
                            current.Queue.AddLast(Document);
                        }
                        result = true;
                    }
                    else
                    {
                        SuspendedQueue.Add(Document);
                    }
                }
                catch (Exception xyz)
                {
                    if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send document, unknown error."));
                }
            }
            else
            {
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send document, already disposed."));
            }
            return result;
        }

        public void StartSuspend()
        {
            if (!suspended)
            {
                suspended = true;
            }
            else
            {
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Warning: Forwarder for " + assignedForwarder.forwarderName + " told to suspend, already suspended."));
            }
        }

        public void EndSuspend()
        {
            if (suspended)
            {
                suspended = false;
                foreach (BaseDocument msg in SuspendedQueue)
                {
                    sendDocument(msg);
                }
            }
            else
            {
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Warning: Forwarder for " + assignedForwarder.forwarderName + " told to end suspend, already running."));
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

        // This is the server code

        private void serverThreadProcess()
        {
            try
            {
                int port = 31337;
                int.TryParse(assignedForwarder.Parameters.ExtractedMetadata.GetElement("syslogport"), out port);
                if (port == 0) port = 31337;

                IPAddress localAddr = IPAddress.Parse(assignedForwarder.Parameters.ExtractedMetadata.GetElement("syslog"));

                // TcpListener server = new TcpListener(port);
                TcpListener serverSocket = new TcpListener(localAddr, port);
                TcpClient clientSocket = default(TcpClient);

                // Start listening for client requests.

                serverSocket.Start();

                while (running)
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                    TCPEventStreamHandler client = new TCPEventStreamHandler();
                    client.onConnectionLost += new EventHandler(onClientDisconnect);
                    client.startClient(clientSocket);
                    ConnectedClients.Add(client);
                }
                
                clientSocket.Close();
                serverSocket.Stop();
                for (int i=0;i<ConnectedClients.Count;i++)
                {
                    TCPEventStreamHandler current = (TCPEventStreamHandler)ConnectedClients[i];
                    current.Running = false;
                    current.Queue.Clear();
                }
            }
            catch (Exception xyz)
            {

            }
        }

        private void onClientDisconnect(object o, EventArgs e)
        {
            TCPEventStreamHandler lostConnection = (TCPEventStreamHandler)o;
            ConnectedClients.Remove(lostConnection);
        }

        public void HeartBeat()
        {

        }
    }
}
