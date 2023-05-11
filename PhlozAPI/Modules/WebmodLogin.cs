using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using System.Security.Cryptography;
using FatumCore;

namespace PhlozLib
{
    public class WebmodLogin : ModuleInterface
    {
        CollectionState State = null;
  
        public void Init(CollectionState state)
        {
            State = state;
        }

        public void SendRequest(HttpListenerContext httpContext)
        {
            SendResponse(httpContext);
        }

        public Boolean IsHandler(string uri)
        {
            Boolean result = false;

            char[] sep = { '/' };
            string[] request = uri.Split(sep);

            if (request.Length > 1)
                {
                    if (request[1] == "Login")
                    {
                        result = true;
                    }
                }

            return result;
        }

        public void SendResponse(HttpListenerContext context)
        {
            try
            {
                char[] sep = { '/' };
                string[] request = context.Request.RawUrl.Split(sep);
                byte[] msg = null;

                if (request.Length == 2)
                {
                    if (request[1] == "Login")
                    {
                        // Acquire additional information from POST form
                        ExtractForm data = new ExtractForm(context);
                        Boolean sanitycheck = true;

                        string username = data.extraction.getElement("User");
                        
                        if (username != null)
                        {
                            username = FatumLib.fromSafeString(username);
                            if (username.Contains('@'))
                            {
                                if (!SecurityInputSanitizer.SafetyCheck(SecurityInputSanitizer.EMAIL, username))
                                {
                                    sanitycheck = false;
                                }
                            }
                            else
                            {
                                if (!SecurityInputSanitizer.SafetyCheck(SecurityInputSanitizer.USERNAME, username))
                                {
                                    sanitycheck = false;
                                }
                            }
                        }
                        else
                        {
                            sanitycheck = false;
                        }

                        string password = data.extraction.getElement("Password");
                        if ( password != null)
                        {
                            if (!SecurityInputSanitizer.SafetyCheck(SecurityInputSanitizer.PASSWORD,password))
                            {
                                sanitycheck = false;
                            }
                        }
                        else
                        {
                            sanitycheck = false;
                        }

                        if (sanitycheck)
                        {
                            string hash = "";

                            BaseAccount locatedAccount = BaseAccount.loadAccountByUsername(State.managementDB, username);
                            if (locatedAccount != null)
                            {
                                hash = locatedAccount.PasswordHash;

                                string pwHash = Authentication.getPasswordHash(username, password);
                                if (BaseAccount.Authenticate(State.managementDB, username, pwHash))
                                {
                                    BaseSession newSession = new BaseSession();
                                    newSession.DateAdded = DateTime.Now;
                                    newSession.DateExpires = DateTime.Now;
                                    newSession.DateExpires = newSession.DateExpires.AddHours(1);
                                    newSession.IPAddress = context.Request.RemoteEndPoint.ToString();
                                    newSession.Account = username;
                                    newSession.SessionID = Authentication.generateSession(username);

                                    context.Response.SetCookie(new Cookie("sid", newSession.SessionID));
                                    //State.Sessions.Add(newSession);

                                    // DISPLAY MAIN MENU

                                    string redirectToMainMenu = StaticHTMLLibrary.redirectPage("mainmenu.html");
                                    msg = Encoding.ASCII.GetBytes(redirectToMainMenu);
                                }
                                else
                                {
                                    // DISPLAY LOGIN FAILED FORM

                                    string redirectToMainMenu = StaticHTMLLibrary.redirectPage("failedlogin.html");
                                    msg = Encoding.ASCII.GetBytes(redirectToMainMenu);
                                }
                            }
                            else
                            {
                                // DISPLAY LOGIN FAILED FORM
                                string redirectToMainMenu = StaticHTMLLibrary.redirectPage("failedlogin.html");
                                msg = Encoding.ASCII.GetBytes(redirectToMainMenu);
                            }
                        }
                    }
                }

                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
            catch (Exception xyzzy)
            {
                byte[] msg = null;
                msg = Encoding.ASCII.GetBytes(StaticHTMLLibrary.errorMessage(1, xyzzy.Message + "\n\n" + xyzzy.StackTrace)); 
                context.Response.ContentLength64 = msg.Length;
                using (Stream s = context.Response.OutputStream)
                    s.Write(msg, 0, msg.Length);
            }
        }
    }
}
