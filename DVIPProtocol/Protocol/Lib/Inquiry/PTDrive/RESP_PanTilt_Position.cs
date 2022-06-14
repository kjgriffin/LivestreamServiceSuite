using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry.PTDrive
{
    public class RESP_PanTilt_Position
    {
        public int Pan { get; private set; }
        public int Tilt { get; private set; }
        public byte Speed { get; private set; }
        public bool Valid { get; private set; }

        /// <summary>
        /// Creates a pan/tilt preset.
        /// </summary>
        /// <param name="pan">Assumes the byte order to be: 000_pqrst</param>
        /// <param name="tilt">Assumes the byte order to be: 0000_abcd</param>
        /// <param name="speed">Desired recall speed</param>
        /// <param name="valid">Is the preset valid</param>
        /// <returns></returns>
        internal static RESP_PanTilt_Position Create(int pan, int tilt, byte speed, bool valid)
        {
            return new RESP_PanTilt_Position
            {
                Pan = pan,
                Tilt = tilt,
                Speed = speed,
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
        internal static RESP_PanTilt_Position Create(byte[] sortedbytes, byte speed, bool valid)
        {
            if (sortedbytes.Length != 9)
            {
                return new RESP_PanTilt_Position
                {
                    Pan = 0,
                    Tilt = 0,
                    Speed = 0,
                    Valid = false
                };
            }

            int pan = sortedbytes[0] | sortedbytes[1] | sortedbytes[2] | sortedbytes[3] | sortedbytes[4];
            int tilt = sortedbytes[5] | sortedbytes[6] | sortedbytes[7] | sortedbytes[8];
            return Create(pan, tilt, speed, valid);
        }

    }
}
