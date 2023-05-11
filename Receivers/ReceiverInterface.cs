//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhlozLib
{
    public interface ReceiverInterface
    {
        void Start();
        void Stop();
        void Dispose();
        void StartSuspend();
        void EndSuspend();
        Boolean isSuspended();
        Boolean isRunning();
        ReceiverStatus getStatus();
        String getReceiverType();
        String getServiceID();
        void setServiceID(string serviceid);
        void MSPHeartBeat();
        void registerFlow(BaseFlow flow);
        void deregisterFlow(BaseFlow flow);
        void reloadFlow(BaseFlow flow);

        void setCallbacks(DocumentEventHandler documentEventHandler,
            PhlozLib.ErrorEventHandler errorEventHandler,
            EventHandler communicationLost,
            EventHandler stoppedReceiver,
            FlowEventHandler flowEventHandler);
    }
}
