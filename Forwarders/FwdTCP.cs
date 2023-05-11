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

namespace PhlozLib
{
    public class FwdTCP : ForwarderInterface
    {
        TcpClient output = null;
        NetworkStream outstream = null; 

        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        BaseForwarder assignedForwarder = null;

        ArrayList SuspendedQueue = new ArrayList();

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onForwarderError;

        public FwdTCP(BaseForwarder forwarder)
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
                    int port = 514;
                    int.TryParse(assignedForwarder.Parameters.ExtractedMetadata.getElement("syslogport"), out port);

                    output = new TcpClient(assignedForwarder.Parameters.ExtractedMetadata.getElement("syslog"), port);
                    outstream = output.GetStream();
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
                        if (output != null)
                        {
                            Byte[] sendBytes = Encoding.ASCII.GetBytes(Document + "\r\n");
                            outstream.Write(sendBytes, 0, sendBytes.Length);
                            result = true;
                        }
                        else
                        {
                            if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " sent a document, communications object NULL."));
                        }
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

        public void HeartBeat()
        {

        }
    }
}
