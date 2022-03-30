using DVIPProtocol.Binary.Magic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.ControlCommand.Cmd.Memory
{
    public class RecallMemory_Command : ControlCommand
    {
        public static ControlCommand Create(int memoryNum)
        {
            return new RecallMemory_Command
            {
                SendCommandData = new List<byte>
                {
                    (byte)ControlCommandBytes.CAM,
                    (byte)ControlCommandBytes.CMD,
                    (byte)CommandStyleBytes.DIRECT,
                    (byte)MemoryCommandBytes.MemoryPreset,
                    (byte)MemoryCommandBytes.RecallPreset,
                    (byte)memoryNum,
                    (byte)ControlCommandBytes.END,
                }
            };
        }

        private protected override void ParseResponse(byte[] response)
        {
        }
    }
}
