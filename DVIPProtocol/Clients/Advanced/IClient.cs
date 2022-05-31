using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Advanced
{

    public delegate void OnCommandReply(byte[] response);

    internal interface IClient
    {
        public IPEndPoint Endpoint { get; set; }
        void SendCommand(byte[] data, OnCommandReply responseDelegate);
    }
}
