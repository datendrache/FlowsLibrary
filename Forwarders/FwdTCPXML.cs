//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using FatumCore;

namespace PhlozLib
{
    public class FwdTCPXML : ForwarderInterface
    {
        TcpClient output = null;
        NetworkStream outstream = null; 

        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        BaseForwarder assignedForwarder = null;
        int Port = 31338;

        ArrayList SuspendedQueue = new ArrayList();

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onForwarderError;


        public FwdTCPXML(BaseForwarder forwarder)
        {
            assignedForwarder = forwarder;
        }

        public void Start()
        {
            if (active)
            {
                if (!running)
                {
                    running = true;
                    int port = 31338;
                    int.TryParse(assignedForwarder.Parameters.ExtractedMetadata.getElement("syslogport"), out port);
                    Port = port;
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

        Boolean inProgress = false;
        ArrayList processList = null;

        public Boolean sendDocument(BaseDocument Document)
        {
            Boolean result = false;

            if (inProgress)
            {
                SuspendedQueue.Add(Document);
            }
            else
            {
                inProgress = true;

                if (active)
                {
                    try
                    {
                        if (!suspended)
                        {

                            if (SuspendedQueue.Count > 0)
                            {
                                lock (SuspendedQueue.SyncRoot)
                                {
                                    processList = SuspendedQueue;
                                    SuspendedQueue = new ArrayList();
                                }
                            }
                            else
                            {
                                processList = new ArrayList(1);
                            }

                            processList.Add(Document);

                            output = new TcpClient(assignedForwarder.Parameters.ExtractedMetadata.getElement("syslog"), Port);
                            outstream = output.GetStream();

                            Byte[] sendBytes = Encoding.Unicode.GetBytes(documentListToString(processList));
                            outstream.Write(sendBytes, 0, sendBytes.Length);
                            outstream.Flush();
                            outstream.Close();

                            processList.Clear();
                            processList = null;
                            result = true;
                        }
                        else
                        {
                            SuspendedQueue.Add(Document);
                        }
                    }
                    catch (Exception xyz)
                    {
                        foreach (BaseDocument current in processList)
                        {
                            SuspendedQueue.Add(current);
                        }
                        processList.Clear();
                        processList = null;
                        if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send message, unknown error."));
                    }
                }
                else
                {
                    if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send message, already disposed."));
                }

                inProgress = false;
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

        public void HeartBeat()
        {
            if (!suspended)
            {
                if (SuspendedQueue.Count > 0)
                {
                    if (!inProgress)
                    {
                        inProgress = true;

                        try
                        {
                            if (output != null)
                            {
                                lock (SuspendedQueue.SyncRoot)
                                {
                                    processList = SuspendedQueue;
                                    SuspendedQueue = new ArrayList();
                                }

                                Byte[] sendBytes = Encoding.ASCII.GetBytes(documentListToString(processList));
                                outstream.Write(sendBytes, 0, sendBytes.Length);
                                outstream.Flush();

                                processList.Clear();
                                processList = null;

                            }
                            else
                            {
                                foreach (BaseDocument current in processList)
                                {
                                    SuspendedQueue.Add(current);
                                }
                                processList.Clear();
                                processList = null;
                                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " sent a document, communications object NULL."));
                            }
                        }
                        catch (Exception xyz)
                        {
                            foreach (BaseDocument current in processList)
                            {
                                SuspendedQueue.Add(current);
                            }
                            processList.Clear();
                            processList = null;
                            if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send queued document batch from Heart Beat, unknown error."));
                        }
                    }
                }
            }
        }

        private string documentListToString(ArrayList documentlist)
        {
            string result = "";
            Tree toconvert = new Tree();

            for (int i = 0; i < documentlist.Count; i++)
            {
                BaseDocument current = (BaseDocument)documentlist[i];
                toconvert.addNode(current.getMetadata(), "Document");
            }

            StringWriter sw = new StringWriter();
            TreeDataAccess.writeXML(sw, toconvert, "Documents");
            result = sw.ToString();
            toconvert.dispose();
            return result;
        }
    }
}
