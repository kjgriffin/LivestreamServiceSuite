using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class LSBElementUnknown : ILSBElement
    {

        public string Unknown { get; private set; } = "";

        public static ILSBElement Parse(AngleSharp.Dom.IElement element)
        {
            return new LSBElementUnknown() { Unknown = element.StrippedText() };
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as UNKNOWN. {Unknown}";
        }

        public string XenonAutoGen()
        {
            return "";
        }
    }
}
