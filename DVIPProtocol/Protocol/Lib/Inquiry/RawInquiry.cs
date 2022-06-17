using DVIPProtocol.Protocol.Lib.Command;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry
{
    public class RawInquiry : IInquiry<IResponse>
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
            return new RawInquiry(data);
        }

        IResponse IInquiry<IResponse>.Parse(byte[] resp)
        {
            return Parse(resp);
        }

        IResponse IInquiry<IResponse>.Parse_FakeGood()
        {
            return Parse_FakeGood();
        }

        IResponse IInquiry<IResponse>.Parse_FakeBad()
        {
            return Parse_FakeBad();
        }
    }

    public class RawInquiryResp : IResponse
    {
        public byte[] Data { get; private set; }
        public int ExpectedResponseLength { get => 0; }

        public RawInquiryResp(byte[] resp)
        {
            Data = resp;
        }
    }

}
