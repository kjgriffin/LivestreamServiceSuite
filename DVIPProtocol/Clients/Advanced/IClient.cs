using DVIPProtocol.Protocol;
using DVIPProtocol.Protocol.Lib.Command;
using DVIPProtocol.Protocol.Lib.Inquiry;

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
        public IPEndPoint Endpoint { get; }
        void Init();
        void Stop();
    }

    public interface ICmdClient : IClient
    {
        void SendCommand(ICommand cmd);
    }

    public interface IInqClient : IClient
    {
        void SendRequest<TResp>(IInquiry<IResponse> inq, int expectedRequestLength, OnRequestReply reply);
    }

    public interface IFullClient : ICmdClient, IInqClient
    {

    }

}
