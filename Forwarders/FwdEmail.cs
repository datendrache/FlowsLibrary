//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Net.Mail;

namespace PhlozLib
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
                        int.TryParse(assignedForwarder.Parameters.ExtractedMetadata.getElement("smtpport").ToLower(), out port);
                        SmtpClient cl = new SmtpClient();
                        cl.Host = assignedForwarder.Parameters.ExtractedMetadata.getElement("smtpserver");
                        cl.Port = port;
                        if (assignedForwarder.Parameters.ExtractedMetadata.getElement("smtpsslenabled").ToLower() == "true")
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
                        
                        mg.From = new MailAddress(assignedForwarder.Parameters.ExtractedMetadata.getElement("emailorigin"));
                        mg.To.Add(new MailAddress(assignedForwarder.Parameters.ExtractedMetadata.getElement("emaildestination")));

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
