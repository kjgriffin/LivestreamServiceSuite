using DVIPProtocol.Binary.Magic;

namespace DVIPProtocol.Protocol.ControlCommand.Cmd.Memory
{
    public class SetMemory_Command : ControlCommand
    {

        public static ControlCommand Create(int memoryNum)
        {
            return new SetMemory_Command
            {
                SendCommandData = new List<byte>
                {
                    (byte)ControlCommandBytes.CAM,
                    (byte)ControlCommandBytes.CMD,
                    (byte)CommandStyleBytes.DIRECT,
                    (byte)MemoryCommandBytes.MemoryPreset,
                    (byte)MemoryCommandBytes.SetPreset,
                    (byte)memoryNum,
                    (byte)ControlCommandBytes.END,
                }
            };
        }

        private protected override void ParseResponse(byte[] response)
        {
        }
    }

    public enum MemoryCommandBytes : ushort
    {
        MemoryPreset = 0x3F,
        ResetPreset = 0x00,
        SetPreset = 0x01,
        RecallPreset = 0x02
    }
}
