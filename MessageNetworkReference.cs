//   Phloz
//   Copyright (C) 2003-2019 Eric Knight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace PhlozLib
{
    public class MessageNetworkReference
    {
        public byte[] message;
        public IPAddress ip;
    }
}
