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

    public static class PanTitlDriveDirectionBuilder
    {
        /// <summary>
        /// Build A Direction
        /// </summary>
        /// <param name="X">-1 = left, 1 = right, 0 = stop</param>
        /// <param name="Y">-1 = up, 1 = down, 0 = stop</param>
        /// <returns></returns>
        public static PanTiltDriveDirection BuildDir(int X, int Y)
        {
            if (X == -1)
            {
                if (Y == -1)
                {
                    return PanTiltDriveDirection.UpLeft;
                }
                if (Y == 1)
                {
                    return PanTiltDriveDirection.DownLeft;
                }
                return PanTiltDriveDirection.Left;
            }
            if (X == 0)
            {
                if (Y == -1)
                {
                    return PanTiltDriveDirection.Up;
                }
                if (Y == 1)
                {
                    return PanTiltDriveDirection.Down;
                }
                return PanTiltDriveDirection.Stop;
            }
            if (X == 1)
            {
                if (Y == -1)
                {
                    return PanTiltDriveDirection.UpRight;
                }
                if (Y == 1)
                {
                    return PanTiltDriveDirection.DownRight;
                }
                return PanTiltDriveDirection.Right;
            }
            return PanTiltDriveDirection.Stop;
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
