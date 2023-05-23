using DVIPProtocol.Binary.Magic;

namespace DVIPProtocol.Protocol.ControlCommand.Cmd.Zoom
{
    public class ZoomCommand_STD : ControlCommand
    {
        public static ControlCommand Create(ZoomDir_STD dir)
        {
            return new ZoomCommand_STD
            {
                SendCommandData = new List<byte>
                {
                    (byte)ControlCommandBytes.CAM,
                    (byte)ControlCommandBytes.CMD,
                    (byte)CommandStyleBytes.DIRECT,
                    (byte)CommandModeBytes.CAM_ZOOM,
                    (byte)dir,
                    (byte)ControlCommandBytes.END,
                }
            };
        }

        private protected override void ParseResponse(byte[] response)
        {
        }


    }

    public enum ZoomDir_STD : ushort
    {

        STOP = 00,
        TELE = 02,
        WIDE = 03,
    }
}
