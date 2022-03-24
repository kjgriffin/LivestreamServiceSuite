using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DVIPProtocol.Binary.Magic;

namespace DVIPProtocol.Protocol.ControlCommand.Cmd.PanTiltDrive
{
    public class PanTiltDrive_Direction_Command : ControlCommand
    {
        public static ControlCommand Create(PanTiltDriveDirection direction, byte panSpeed, byte tiltSpeed)
        {
            return new PanTiltDrive_Direction_Command
            {
                SendCommandData = new List<byte>
                {
                    (byte)ControlCommandBytes.CAM,
                    (byte)ControlCommandBytes.CMD,
                    (byte)CommandStyleBytes.INDIRECT,
                    (byte)CommandModeBytes.PANTILTDRIVE_MOTION,
                    panSpeed,
                    tiltSpeed,
                    ((ushort)direction).High(),
                    ((ushort)direction).Low(),
                    (byte)ControlCommandBytes.END,
                }
            };
        }

        private protected override void ParseResponse(byte[] response)
        {

        }
    }

    public enum PanTiltDriveDirection : ushort
    {
        Up = 0x0301,
        Down = 0x3002,
        Left = 0x0103,
        Right = 0x0203,
        UpLeft = 0x0101,
        UpRight = 0x0201,
        DownLeft = 0x0102,
        DownRight = 0x0202,
        Stop = 0x0303,
    }

}
