using System.Text.Json.Serialization;

namespace DVIPProtocol.Protocol.Lib.Inquiry.PTDrive
{
    public class RESP_PanTilt_Position : IResponse
    {
        public long Pan { get; set; }
        public int Tilt { get; set; }
        public bool Valid { get; set; }

        [JsonIgnore]
        public byte[] Data
        {
            get
            {
                byte[] res = new byte[12];

                res[0] = 0x90;
                res[1] = 0x50;

                res[2] = (byte)(Pan >> 16 & 0xF);
                res[3] = (byte)(Pan >> 12 & 0xF);
                res[4] = (byte)(Pan >> 8 & 0xF);
                res[5] = (byte)(Pan >> 4 & 0xF);
                res[6] = (byte)(Pan >> 0 & 0xF);

                res[7] = (byte)(Pan >> 12 & 0xF);
                res[8] = (byte)(Pan >> 8 & 0xF);
                res[9] = (byte)(Pan >> 4 & 0xF);
                res[10] = (byte)(Pan >> 0 & 0xF);

                res[11] = 0xFF;

                return res;
            }
        }

        public static int ExpectedResponseLength { get => 12; }

        /// <summary>
        /// Creates a pan/tilt preset.
        /// </summary>
        /// <param name="pan">Assumes the byte order to be: 000_pqrst</param>
        /// <param name="tilt">Assumes the byte order to be: 0000_abcd</param>
        /// <param name="valid">Is the preset valid</param>
        /// <returns></returns>
        internal static RESP_PanTilt_Position Create(long pan, int tilt, bool valid)
        {
            return new RESP_PanTilt_Position
            {
                Pan = pan,
                Tilt = tilt,
                Valid = valid,
            };
        }

        /// <summary>
        /// Creates a pan/tilt preset.
        /// </summary>
        /// <param name="sortedbytes">Assumes byte order to be: 0->8:: pqrst, abcd</param>
        /// <param name="speed">Desired recall speed</param>
        /// <param name="valid">Is the preset valid</param>
        /// <returns></returns>
        internal static RESP_PanTilt_Position Create(byte[] sortedbytes, bool valid)
        {
            if (sortedbytes.Length != 9)
            {
                return new RESP_PanTilt_Position
                {
                    Pan = 0,
                    Tilt = 0,
                    Valid = false
                };
            }

            long pan = sortedbytes[0] << 16 | sortedbytes[1] << 12 | sortedbytes[2] << 8 | sortedbytes[3] << 4 | sortedbytes[4];
            int tilt = sortedbytes[5] << 12 | sortedbytes[6] << 8 | sortedbytes[7] << 4 | sortedbytes[8];
            return Create(pan, tilt, valid);
        }

    }
}
