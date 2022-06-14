using DVIPProtocol.Protocol.Lib.Command;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry
{
    public interface IInquiry<TResp> : ICommand
    {
         TResp Parse(byte[] resp); 
    }
}
