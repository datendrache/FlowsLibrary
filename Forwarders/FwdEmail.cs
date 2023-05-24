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

using System.Collections;
using System.Net.Mail;

namespace Proliferation.Flows
{
    public class FwdEmail : ForwarderInterface
    {
        StreamWriter output = null;
        Boolean active = true;
        Boolean suspended = false;
        Boolean running = false;
        BaseForwarder assignedForwarder = null;

        ArrayList SuspendedQueue = new ArrayList();

        public EventHandler onCommunicationLost;
        public ErrorEventHandler onForwarderError;

        public FwdEmail(BaseForwarder forwarder)
        {
            assignedForwarder = forwarder;
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            running = false;
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
                        // Send the email here.
                        int port = 0;
                        int.TryParse(assignedForwarder.Parameters.ExtractedMetadata.GetElement("smtpport").ToLower(), out port);
                        SmtpClient cl = new SmtpClient();
                        cl.Host = assignedForwarder.Parameters.ExtractedMetadata.GetElement("smtpserver");
                        cl.Port = port;
                        if (assignedForwarder.Parameters.ExtractedMetadata.GetElement("smtpsslenabled").ToLower() == "true")
                        {
                            cl.EnableSsl = true;
                        }
                        else
                        {
                            cl.EnableSsl = false;
                        }

                        MailMessage mg = new MailMessage();
                        mg.Subject = "Notification";
                        mg.Body = Document.Document;
                        
                        mg.From = new MailAddress(assignedForwarder.Parameters.ExtractedMetadata.GetElement("emailorigin"));
                        mg.To.Add(new MailAddress(assignedForwarder.Parameters.ExtractedMetadata.GetElement("emaildestination")));

                        try
                        {
                            cl.Send(mg);
                        }
                        catch (Exception xyz)
                        {
                            onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send email: " + xyz.Message.ToString()));
                            SuspendedQueue.Add(Document);
                        }
                        result = true;
                    }
                    else
                    {
                        SuspendedQueue.Add(Document);
                    }
                }
                catch (Exception xyz)
                {
                    if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send message, unknown error."));
                }
            }
            else
            {
                if (onForwarderError != null) onForwarderError.Invoke(this, new ErrorEventArgs("Error: Forwarder for " + assignedForwarder.forwarderName + " cannot send message, already disposed."));
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
