using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.ControlCommand
{
    public abstract class ControlCommand : IRequest
    {

        protected List<byte>? SendCommandData;


        public event EventHandler<int>? OnCommandReturnPacketRecieved;
        public event EventHandler<RejectionReason>? OnCommandRejected;


        public byte[] CompletedData { get; protected set; } = new byte[0];

        public void Complete(byte[] response)
        {
            CompletedData = response;
            ParseResponse(response);
            OnCommandReturnPacketRecieved?.Invoke(this, 0); // TODO: send meaningful data here?
        }

        private protected abstract void ParseResponse(byte[] response);

        public byte[] PackagePayload()
        {
            int payloadLength = (SendCommandData?.Count ?? 0) + 2;
            byte[] payload = new byte[payloadLength];

            payload[0] = SendPacketLengthHighByte(payloadLength);
            payload[1] = SendPacketLengthLowBytes(payloadLength);

            int i = 0;
            foreach (byte data in SendCommandData ?? new List<byte>())
            {
                payload[i + 2] = data;
                i++;
            }

            return payload;
        }

        public void Reject(RejectionReason reason)
        {
            OnCommandRejected?.Invoke(this, reason);
        }

        private byte SendPacketLengthHighByte(int data)
        {
            return (byte)((data >> 8) & 0xFF);
        }
        private byte SendPacketLengthLowBytes(int data)
        {
            return (byte)(data & 0xFF);
        }



    }
}
