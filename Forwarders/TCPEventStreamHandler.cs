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

using System.Text;
using System.Net.Sockets;

namespace Proliferation.Flows
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

