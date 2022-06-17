using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVIPProtocol.Protocol.Lib.Command.PTDrive;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Command.PTDrive.Tests
{
    [TestClass()]
    public class CMD_PanTiltAbsPosTests
    {
        [TestMethod()]
        public void CMD_ABS_POSTest_GeneratesCorrectBitstream()
        {
            byte[] expectedData = new byte[]
            {
                0x00,
                0x12,
                0x81,
                0x01,
                0x06,
                0x02,
                0x0a,
                0x00,
                0x0a, 0x0b, 0x0c, 0x0d, 0x0e,
                0x0a, 0x0b, 0x0c, 0x0d,
                0xFF
            };

            var cmd = CMD_PanTiltAbsPos.CMD_ABS_POS(703710, 43981, 10);
            var res = cmd.PackagePayload();

            CollectionAssert.AreEqual(expectedData, res);
        }
    }
}