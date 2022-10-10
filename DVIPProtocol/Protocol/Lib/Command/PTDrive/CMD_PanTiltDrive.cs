using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Command.PTDrive
{
    public class CMD_PanTiltDrive : ICommand
    {
        public byte[] Data { get; }

        private CMD_PanTiltDrive(byte[] data)
        {
            Data = data;
        }

        public static ICommand UpDownLeftRight(PanTiltDirection dir, byte speed)
        {
            byte panCode = 0x03;
            byte tiltCode = 0x03;

            switch (dir)
            {
                case PanTiltDirection.UP:
                    tiltCode = 0x01;
                    break;
                case PanTiltDirection.DOWN:
                    tiltCode = 0x02;
                    break;
                case PanTiltDirection.LEFT:
                    panCode = 0x01;
                    break;
                case PanTiltDirection.RIGHT:
                    panCode = 0x02;
                    break;
                case PanTiltDirection.STOP:
                default:
                    panCode = 0x03;
                    tiltCode = 0x03;
                    break;
            }

            // pan/tilt at same speed for now
            return new CMD_PanTiltDrive(new byte[] { 0x81, 0x01, 0x06, 0x01, speed, speed, panCode, tiltCode, 0xFF });
        }

        public static ICommand CMD_STOP_DRIVE()
        {
            return new CMD_PanTiltDrive(new byte[] { 0x81, 0x01, 0x06, 0x01, 0x00, 0x00, 0x03, 0x03, 0xFF });
        }

    }

    public enum PanTiltDirection
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        STOP,
    }
}
