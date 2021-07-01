using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    abstract class ExternalElement : ILSBElement
    {

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
