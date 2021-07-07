using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    abstract class ExternalElement : ILSBElement
    {
        public string PostsetCmd { get; set; }

        public virtual string DebugString()
        {
            return $"/// XENON DEBUG::Added External Element";
        }

        public virtual string XenonAutoGen()
        {
            return "";
        }
    }
}
