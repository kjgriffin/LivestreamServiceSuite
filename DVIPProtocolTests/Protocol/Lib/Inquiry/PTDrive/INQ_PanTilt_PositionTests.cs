﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVIPProtocol.Protocol.Lib.Inquiry.PTDrive;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVIPProtocol.Protocol.Lib.Inquiry.PTDrive.Tests
{
    [TestClass()]
    public class INQ_PanTilt_PositionTests
    {
        [TestMethod()]
        public void Parse_ValidResponse_ReturnsCorrectRESP()
        {

            // this is an real response returned by hardware
            byte[] resp = new byte[14]
            {
                0x00, // length
                0x0E,

                0x90,
                0x50, // ack

                0x00, // pqrst
                0x04,
                0x00,
                0x0E,
                0x00,

                0x00, // abcd
                0x00,
                0x0E,
                0x00,

                0xFF, // end
            };

            var test = INQ_PanTilt_Position.Create() as INQ_PanTilt_Position;
            var result = test.Parse(resp);

            Assert.AreEqual(16608, result.Pan);
            Assert.AreEqual(224, result.Tilt);
            Assert.AreEqual(true, result.Valid);
        }
    }
}