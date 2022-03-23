namespace DVIPProtocol.Binary.Magic
{


    internal static class BinaryHelpers
    {
        internal static byte High(this ushort val)
        {
            return (byte)((val >> 8) & 0xFF);
        }
        internal static byte Low(this ushort val)
        {
            return (byte)(val & 0xFF);
        }
    }


    internal enum ControlCommandBytes : byte
    {
        CAM = 0x81,
        CMD = 0x01,
        INQ = 0x09,
        END = 0xFF,
    }

    internal enum CommandStyleBytes : byte
    {
        INDIRECT = 0x06,
        DIRECT = 0x04,
    }

    internal enum CommandModeBytes : byte
    {
        PANTILTDRIVE_MOTION = 0x01,
        PANTILTDRIVE_ABSPOS = 0x02,
        PANTILTDRIVE_RELPOS = 0x03,
        PANTILTDRIVE_HOME = 0x04,
        PANTILTDRIVE_RESET = 0x05,
    }





}