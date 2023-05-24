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

namespace Proliferation.Flows
{    
    public class BaseReceiver
    {
        public EventHandler onCommunicationLost;
        public ErrorEventHandler onReceiverError;
        public EventHandler onStopped;
        public DocumentEventHandler onDocumentReceived;
        public FlowEventHandler onFlowDetected;

        public BaseReceiver(DocumentEventHandler DocumentArrived, 
            ErrorEventHandler ErrorReceived,
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
