namespace DVIPProtocol.Protocol.Lib.Inquiry.PTDrive
{
    public class INQ_PanTilt_Position : IInquiry<IResponse>
    {
        public byte[] Data { get; }

        private INQ_PanTilt_Position(byte[] data)
        {
            Data = data;
        }

        public static IInquiry<IResponse> Create()
        {
            return new INQ_PanTilt_Position(new byte[] { 0x81, 0x09, 0x06, 0x12, 0xff });
        }

        public RESP_PanTilt_Position Parse(byte[] resp)
        {

            bool ackOK = false;
            // find start index
            int i = 0;
            while (i + 2 < resp.Length)
            {
                if (resp[i] == 0x90 && resp[i + 1] == 0x50)
                {
                    i = i + 1;
                    ackOK = true;
                    break;
                }
                i++;
            }

            // parse response
            if (ackOK)
            {
                int di = 0;

                byte[] data = new byte[9];

                while (++i < resp.Length && di < 9)
                {
                    data[di] = (byte)(resp[i] & 0xF);
                    di++;
                }

                if (i < resp.Length && resp[i] == 0xFF)
                {
                    // found end of response, build valid response
                    return RESP_PanTilt_Position.Create(data, true);
                }

            }

            // return invalid
            return RESP_PanTilt_Position.Create(0, 0, false);
        }

        public RESP_PanTilt_Position Parse_FakeGood()
        {
            return RESP_PanTilt_Position.Create(0, 0, true);
        }

        public RESP_PanTilt_Position Parse_FakeBad()
        {
            return RESP_PanTilt_Position.Create(0, 0, false);
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
}
