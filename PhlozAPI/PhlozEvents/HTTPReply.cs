using System;

namespace PhlozLib
{
    public class HTTPReply : EventArgs
    {
        public string ErrorMessage = "";

        public HTTPReply(string Msg)
        {
            ErrorMessage = Msg;
        }
    }
}
