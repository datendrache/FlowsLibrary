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

using System.Net.Sockets;

namespace Proliferation.Flows
{
    //Class to handle each client request separatly
    public class TCPHandler
    {
        TcpClient clientSocket;
        public Boolean Authenticated = false;
        public Boolean Running = true;
        BaseFlow assignedFlow;

        public EventHandler onConnectionLost;
        public DocumentEventHandler onDocument;
        private CollectionState State = null;

        public void startClient(CollectionState state, TcpClient inClientSocket, BaseFlow Flow)
        {
            State = state;
            this.clientSocket = inClientSocket;
            assignedFlow = Flow;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            string dataFromClient = null;

            try
            {
                NetworkStream networkStream = clientSocket.GetStream();
                StreamReader easierStream = new StreamReader(networkStream);
                StreamWriter outStream = new StreamWriter(networkStream);

                dataFromClient = easierStream.ReadToEnd();

                if (onDocument != null)
                {

                    // TODO:  NOTE TO SELF -- this needs something like a GUARD or at least parsing of the data to
                    //        properly identify where this is sent.  I've passed in the State variable to look up
                    //        feeds, but this is hardly security. For example, anyone can inject anything into any stream.

                    DocumentEventArgs msg = new DocumentEventArgs();
                    BaseDocument incoming = new BaseDocument(assignedFlow);

                    incoming.Document = dataFromClient;
                    incoming.received = DateTime.Now;
                    msg.Document = incoming;

                    onDocument.Invoke(networkStream, msg);
                }
            }
            catch (Exception ex)
            {
                if (onConnectionLost != null) onConnectionLost.Invoke(clientSocket, new EventArgs());
            }
        }
    } 
}

