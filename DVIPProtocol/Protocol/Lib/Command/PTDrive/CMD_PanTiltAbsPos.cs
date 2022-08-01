using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Command.PTDrive
{
    public class CMD_PanTiltAbsPos : ICommand
    {
        public byte[] Data { get; }

        private CMD_PanTiltAbsPos(byte[] data)
        {
            Data = data;
        }

        public static ICommand CMD_ABS_POS(long pan, int tilt, byte speed)
        {
            byte p = (byte)((pan >> 16) & 0x0F);
            byte q = (byte)((pan >> 12) & 0x0F);
            byte r = (byte)((pan >> 8) & 0x0F);
            byte s = (byte)((pan >> 4) & 0x0F);
            byte t = (byte)((pan >> 0) & 0x0F);

            byte a = (byte)((tilt >> 12) & 0x0F);
            byte b = (byte)((tilt >> 8) & 0x0F);
            byte c = (byte)((tilt >> 4) & 0x0F);
            byte d = (byte)((tilt >> 0) & 0x0F);

            return new CMD_PanTiltAbsPos(new byte[]
            {
                0x81,
                0x01,
                0x06,
                0x02,
                speed,
                0x00,
                p, q, r, s, t,
                a, b, c, d,
                0xFF
            });
        }


    }
}
