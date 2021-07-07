using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    interface ILSBElement
    {

        public string PostsetCmd { get; set; }
        public string DebugString();
        public string XenonAutoGen();

    }
}
