using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonVersionInfo;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonVersionInfo.Tests
{
    [TestClass()]
    public class BuildVersionTests
    {

        [TestMethod()]
        public void GreaterVersionTest_Build()
        {
            var v1 = new BuildVersion(1, 1, 1, 1, "Release");
            var v2 = new BuildVersion(1, 1, 1, 2, "Release");
            Assert.IsTrue(v2.GreaterVersion(v1));
        }
        [TestMethod()]
        public void GreaterVersionTest_Rev()
        {
            var v1 = new BuildVersion(1, 1, 1, 1, "Release");
            var v2 = new BuildVersion(1, 1, 2, 1, "Release");
            Assert.IsTrue(v2.GreaterVersion(v1));
        }

        [TestMethod()]
        public void GreaterVersionTest_Min()
        {
            var v1 = new BuildVersion(1, 1, 1, 1, "Release");
            var v2 = new BuildVersion(1, 2, 1, 1, "Release");
            Assert.IsTrue(v2.GreaterVersion(v1));
        }

        [TestMethod()]
        public void GreaterVersionTest_Maj()
        {
            var v1 = new BuildVersion(1, 1, 1, 1, "Release");
            var v2 = new BuildVersion(2, 1, 1, 1, "Release");
            Assert.IsTrue(v2.GreaterVersion(v1));
        }


        [TestMethod()]
        public void GreaterVersionTest_Build2()
        {
            var v1 = new BuildVersion(1, 10, 1, 1, "Release");
            var v2 = new BuildVersion(2, 0, 0, 1, "Release");
            Assert.IsTrue(v2.GreaterVersion(v1));
        }

    }
}