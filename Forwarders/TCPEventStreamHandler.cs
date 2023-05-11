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
    public class TCPEventStreamHandler
    {
        TcpClient clientSocket;
        public LinkedList<BaseDocument> Queue = new LinkedList<BaseDocument>();
        public Boolean Authenticated = false;
        public Boolean Running = true;
        
        public EventHandler onConnectionLost;

        public void startClient(TcpClient inClientSocket)
        {
            this.clientSocket = inClientSocket;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string rCount = null;

            while (Running)
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    while (!Authenticated)
                    {
                        StreamReader easierStream = new StreamReader(networkStream);
                        StreamWriter outStream = new StreamWriter(networkStream);

                        dataFromClient = easierStream.ReadLine();
                        
                        if (dataFromClient == "")
                        {
                            Authenticated = true;
                        }
                    }

                    while (Running)
                    {
                        if (Queue.Count > 0)
                        {
                            LinkedList<BaseDocument> tmpQueue = new LinkedList<BaseDocument>();
                            tmpQueue = Queue;
                            Queue = new LinkedList<BaseDocument>();

                            foreach (BaseDocument current in tmpQueue)
                            {
                                string msg = current.FlowID + "," + current.Category + "," + current.Label + "," + current.Document + "\r\n";
                                sendBytes = Encoding.ASCII.GetBytes(msg);
                                networkStream.Write(sendBytes, 0, sendBytes.Length);
                                networkStream.Flush();
                            }
                            tmpQueue.Clear();
                        }
                        else
                        {
                            Thread.Sleep(1000);  // Wait one second for more data to roll in
                        }
                    }
                    Queue.Clear();
                    if (onConnectionLost != null) onConnectionLost.Invoke(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    Running = false;
                    if (onConnectionLost != null) onConnectionLost.Invoke(this, new EventArgs());
                }
            }
        }
    } 
}

