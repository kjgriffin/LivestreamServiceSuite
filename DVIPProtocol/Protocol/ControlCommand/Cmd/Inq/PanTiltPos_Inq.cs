using DVIPProtocol.Binary.Magic;

namespace DVIPProtocol.Protocol.ControlCommand.Cmd.Inq
{
    public class PanTiltPos_Inq : ControlCommand
    {

        public static PanTiltPos_Inq Create()
        {
            return new PanTiltPos_Inq
            {
                SendCommandData = new List<byte>
                {
                    (byte)ControlCommandBytes.CAM,
                    (byte)ControlCommandBytes.INQ,
                    (byte)CommandStyleBytes.INDIRECT,
                    0x12,
                    (byte)ControlCommandBytes.END,
                }
            };
        }

        private protected override void ParseResponse(byte[] response)
        {

        }
    }
}
