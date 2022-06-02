using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Clients.Advanced
{

    public delegate void OnRequestReply(byte[] response);

    public interface IClient
    {
        public IPEndPoint Endpoint { get; set; }
        void Init();
        void Stop();
    }

    public interface ICmdClient : IClient
    {
        void SendCommand(byte[] data);
    }

    public interface IInqClient : IClient
    {
        void SendRequest(byte[] data, int expectedRequestLength, OnRequestReply reply);
    }

    public interface IFullClient : ICmdClient, IInqClient
    {

    }

}
