using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry.PTDrive
{
    public class RESP_PanTilt_Position : IResponse
    {
        public int Pan { get; set; }
        public int Tilt { get; set; }
        public bool Valid { get; set; }

        [JsonIgnore]
        public byte[] Data
        {
            get
            {
                byte[] res = new byte[14];

                res[0] = 0;
                res[1] = 0x0e;

                res[2] = 0x90;
                res[3] = 0x50;

                res[4] = (byte)(Pan >> 0 & 0xF);
                res[5] = (byte)(Pan >> 4 & 0xF);
                res[6] = (byte)(Pan >> 8 & 0xF);
                res[7] = (byte)(Pan >> 12 & 0xF);
                res[8] = (byte)(Pan >> 16 & 0xF);

                res[9] = (byte)(Pan >> 0 & 0xF);
                res[10] = (byte)(Pan >> 4 & 0xF);
                res[11] = (byte)(Pan >> 8 & 0xF);
                res[12] = (byte)(Pan >> 12 & 0xF);

                res[13] = 0xFF;

                return res;
            }
        }

        /// <summary>
        /// Creates a pan/tilt preset.
        /// </summary>
        /// <param name="pan">Assumes the byte order to be: 000_pqrst</param>
        /// <param name="tilt">Assumes the byte order to be: 0000_abcd</param>
        /// <param name="valid">Is the preset valid</param>
        /// <returns></returns>
        internal static RESP_PanTilt_Position Create(int pan, int tilt, bool valid)
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

            int pan = sortedbytes[0] | sortedbytes[1] | sortedbytes[2] | sortedbytes[3] | sortedbytes[4];
            int tilt = sortedbytes[5] | sortedbytes[6] | sortedbytes[7] | sortedbytes[8];
            return Create(pan, tilt, valid);
        }

    }
}
