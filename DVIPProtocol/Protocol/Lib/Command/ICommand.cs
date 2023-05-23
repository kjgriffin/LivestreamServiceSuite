namespace DVIPProtocol.Protocol.Lib.Command
{
    public interface ICommand
    {
        internal byte[] Data { get; }

        public byte[] PackagePayload()
        {
            int payloadLength = Data.Length + 2;
            byte[] payload = new byte[payloadLength];

            payload[0] = HighByte(payloadLength);
            payload[1] = LowByte(payloadLength);

            int i = 0;
            foreach (byte data in Data)
            {
                payload[i + 2] = data;
                i++;
            }

            return payload;
        }

        public static byte HighByte(int data)
        {
            return (byte)((data >> 8) & 0xFF);
        }
        public static byte LowByte(int data)
        {
            return (byte)(data & 0xFF);
        }

    }
}
