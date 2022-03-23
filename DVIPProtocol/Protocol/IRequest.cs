using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol
{
    internal interface IRequest
    {
        byte[] PackagePayload();
        void Reject(RejectionReason reason);
        void Complete(byte[] response);
    }

    public enum RejectionReason
    {
        ClientNotCreated,
        BadCommand,
        Failed,
    }

}
