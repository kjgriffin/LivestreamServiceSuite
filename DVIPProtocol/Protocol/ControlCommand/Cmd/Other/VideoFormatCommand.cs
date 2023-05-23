using DVIPProtocol.Binary.Magic;

namespace DVIPProtocol.Protocol.ControlCommand.Cmd.Other
{
    public class VideoFormatCommand : ControlCommand
    {

        public static ControlCommand Create(VideoFormatBytes videoFormat)
        {
            return new VideoFormatCommand
            {
                SendCommandData = new List<byte>
                {
                    (byte)ControlCommandBytes.CAM,
                    (byte)ControlCommandBytes.CMD,
                    (byte)CommandStyleBytes.DIRECT,
                    0x24,
                    0x72,
                    (byte)videoFormat,
                    (byte)ControlCommandBytes.END,
                }
            };
        }

        private protected override void ParseResponse(byte[] response)
        {
        }
    }

    public enum VideoFormatBytes : ushort
    {
        VF_1080_60i = 0x00,
        VF_1080_50i = 0x04,
        VF_1080_30p_NTSC = 0x06,
        VF_1080_25p = 0x08,
        VF_720_60p_NTSC = 0x09,
        VF_720_50p = 0x0C,
        VF_1080_60p_NTSC = 0x13,
        VF_1080_50p = 0x14,
    }


}
