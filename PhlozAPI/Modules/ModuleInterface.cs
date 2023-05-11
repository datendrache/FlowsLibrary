using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;

namespace PhlozLib
{
    public interface ModuleInterface
    {
        void Init(CollectionState state);

        void SendRequest(HttpListenerContext httpContext);

        Boolean IsHandler(string uri);   //  Send this a URL and it determines if this is the proper module to execute
    }
}
