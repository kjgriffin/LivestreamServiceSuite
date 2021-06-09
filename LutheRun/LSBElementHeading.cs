using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun
{
    class LSBElementHeading : ILSBElement
    {

       public string Heading { get; private set; } = "";

        public static ILSBElement Parse(AngleSharp.Dom.IElement element)
        {
            var res = new LSBElementHeading();
            res.Heading = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent;
            return res;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_HEADING. Heading:'{Heading}'";
        }

        public string XenonAutoGen()
        {
            return $"/// <XENON_AUTO_GEN>\r\n/// Heading: {Heading.Replace('\"', '\'')}\r\n/// </XENON_AUTO_GEN>";
        }
    }
}
