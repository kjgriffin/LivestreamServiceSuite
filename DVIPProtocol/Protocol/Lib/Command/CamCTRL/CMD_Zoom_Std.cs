using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DVIPProtocol.Protocol.Lib.Command.CamCTRL.CMD_Zoom_Std;

namespace DVIPProtocol.Protocol.Lib.Command.CamCTRL
{
    public partial class CMD_Zoom_Std : ICommand
    {
        public byte[] Data { get; }

        private CMD_Zoom_Std(byte[] data)
        {
            Data = data;
        }

        public static ICommand Create(ZoomDir dir)
        {
            byte dirCode = 0x00;
            switch (dir)
            {
                case ZoomDir.STOP:
                    dirCode = 0x00;
                    break;
                case ZoomDir.TELE:
                    dirCode = 0x02;
                    break;
                case ZoomDir.WIDE:
                    dirCode = 0x03;
                    break;
            }
            return new CMD_Zoom_Std(new byte[] { 0x81, 0x01, 0x04, 0x07, dirCode, 0xFF });
        }

    }

    public class CMD_Zoom_Variable : ICommand
    {
        public byte[] Data { get; }

        private CMD_Zoom_Variable(byte[] data)
        {
            Data = data;
        }

        public static ICommand Create(ZoomDir dir, byte speed)
        {
            switch (dir)
            {
                case ZoomDir.STOP:
                    speed = 0x00;
                    break;
                case ZoomDir.TELE:
                    speed |= 0x20;
                    break;
                case ZoomDir.WIDE:
                    speed |= 0x30;
                    break;
            }
            return new CMD_Zoom_Variable(new byte[] { 0x81, 0x01, 0x04, 0x07, speed, 0xFF });

        }
    }
}
