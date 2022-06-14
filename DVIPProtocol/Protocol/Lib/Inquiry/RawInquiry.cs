using DVIPProtocol.Protocol.Lib.Command;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry
{
    public class RawInquiry : IInquiry<RawInquiryResp>
    {
        public byte[] Data { get; private set; }

        public RawInquiryResp Parse(byte[] resp)
        {
            return new RawInquiryResp(resp);
        }

        public RawInquiryResp Parse_FakeBad()
        {
            return new RawInquiryResp(new byte[0]);
        }

        public RawInquiryResp Parse_FakeGood()
        {
            return new RawInquiryResp(new byte[0]);
        }

        private RawInquiry(byte[] data)
        {
            Data = data;
        }

        public static IInquiry<IResponse> Create(byte[] data)
        {
            var x = new RawInquiry(data);
            return (IInquiry<IResponse>)x;
        }

    }

    public class RawInquiryResp : IResponse
    {
        public byte[] Data { get; private set; }
        public RawInquiryResp(byte[] resp)
        {
            Data = resp;
        }
    }

}
