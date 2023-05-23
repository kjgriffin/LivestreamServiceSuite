using DVIPProtocol.Protocol.Lib.Command;

namespace DVIPProtocol.Protocol.Lib.Inquiry
{
    public interface IInquiry<TResp> : ICommand where TResp : IResponse
    {
        TResp Parse(byte[] resp);
        TResp Parse_FakeGood();
        TResp Parse_FakeBad();
    }

    public interface IResponse
    {
        byte[] Data { get; }
        static int ExpectedResponseLength { get; }
    }

}
