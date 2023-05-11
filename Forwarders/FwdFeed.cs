//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace PhlozLib
{
    public class FwdFlow : ForwarderInterface
    {
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;

        BaseForwarder assignedForwarder = null;
        BaseFlow assignedFlow = null;

        ArrayList SuspendedQueue = new ArrayList();

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onForwarderError;

        public FwdFlow(BaseForwarder forwarder, BaseFlow flow)
        {
            assignedForwarder = forwarder;
            assignedFlow = flow;
        }

        public void Start()
        {
            if (active)
            {
                if (!running)
                {
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
                        lock (assignedFlow.Incoming.SyncRoot)
                        {
                            BaseDocument newDocument = Document.copy();
                            assignedFlow.Incoming.Add(newDocument);
                            result = true;
                        }
                    }
                    else
                    {
                        result = false;
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
                ArrayList updatedSuspended = new ArrayList();
                suspended = false;
                lock (SuspendedQueue.SyncRoot)
                {
                    foreach (BaseDocument doc in SuspendedQueue)
                    {
                        if (!sendDocument(doc))
                        {
                            updatedSuspended.Add(doc);
                        }
                    }

                    SuspendedQueue.Clear();
                    if (updatedSuspended.Count > 0) SuspendedQueue = updatedSuspended;
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
