//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace PhlozLib
{
    //Class to handle each client request separatly
    public class TCPXMLHandler
    {
        TcpClient clientSocket;
        public Boolean Authenticated = false;
        public Boolean Running = true;
        
        public EventHandler onConnectionLost;
        public DocumentEventHandler onDocument;
        private CollectionState State = null;

        public void startClient(CollectionState state, TcpClient inClientSocket)
        {
            State = state;
            this.clientSocket = inClientSocket;
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
                    //        flows, but this is hardly security. For example, anyone can inject anything into any stream.

                }
            }
            catch (Exception ex)
            {
                if (onConnectionLost != null) onConnectionLost.Invoke(clientSocket, new EventArgs());
            }
        }
    } 
}

