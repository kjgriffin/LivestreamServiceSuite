using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun
{
    class LSBElementLiturgy : ILSBElement
    {

        public string LiturgyText { get; private set; }

        public static ILSBElement Parse(IElement element)
        {
            // process liturgy text
            return new LSBElementLiturgy() { LiturgyText = element.StrippedText() };
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_LITURGY. LiturgyText:'{LiturgyText}'";
        }

        public string XenonAutoGen()
        {
            if (LiturgyText.Trim() != String.Empty)
            {
                return "/// <XENON_AUTO_GEN>\r\n#liturgy{\r\n" + LiturgyText + "\r\n}\r\n/// </XENON_AUTO_GEN>";
            }
            return "";
        }
    }
}
