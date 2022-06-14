using DVIPProtocol.Protocol.Lib.Command;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry.PTDrive
{
    public class INQ_PanTilt_Position : IInquiry<RESP_PanTilt_Position>
    {
        public byte[] Data { get; }
        byte[] ICommand.Data { get; }

        private INQ_PanTilt_Position(byte[] data)
        {
            Data = data;
        }

        public static IInquiry<RESP_PanTilt_Position> Create()
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
                    data[di] = (byte)(resp[i] & 0xF << di);
                    di++;
                }
                
                if (i++ < resp.Length && resp[i] == 0xFF)
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

        [Flags]
        enum ParseState
        {
            AwaitStart = 1,
            RespHeadder = 1 << 1,
            Data = 1 << 2,
            AwaitEnd = 1 << 3,

            Incomplete = 1 << 5,

            Invalid = 1 << 8,
        }

    }
}
