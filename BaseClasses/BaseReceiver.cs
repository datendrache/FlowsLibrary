//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Data;
using System.Collections;
using FatumCore;

namespace PhlozLib
{    
    public class BaseReceiver
    {
        public EventHandler onCommunicationLost;
        public ErrorEventHandler onReceiverError;
        public EventHandler onStopped;
        public DocumentEventHandler onDocumentReceived;
        public FlowEventHandler onFlowDetected;

        public BaseReceiver(DocumentEventHandler DocumentArrived, 
            PhlozLib.ErrorEventHandler ErrorReceived,
            EventHandler CommunicationLost,
            EventHandler StoppedReceiver,
            FlowEventHandler FlowDetected)
        {
             onCommunicationLost = StoppedReceiver;
             onReceiverError = ErrorReceived;
             onStopped = CommunicationLost;
             onDocumentReceived = DocumentArrived;
             onFlowDetected = FlowDetected;
        }

        public ReceiverInterface locateReceiver(CollectionState State, BaseFlow flow)
        {
            ReceiverInterface result = null;
            foreach (ReceiverInterface currentreceiver in flow.ParentService.Receivers)
            {
                if (flow.ParentService.UniqueID == currentreceiver.getServiceID())
                {
                    result = currentreceiver;
                    break;
                }
            }
            return result;
        }

        public void activateReceiver(CollectionState State, BaseFlow current)
        {
            ReceiverInterface receiver = current.ParentService.ParentSource.ConditionalStart(State, current);
            if (receiver != null)  //  Null means that the received did not pass its conditional loading parameters.
            {
                receiver.setCallbacks(onDocumentReceived, onReceiverError, onCommunicationLost, onStopped, onFlowDetected);
                receiver.Start();
            }
        }
    }
}
