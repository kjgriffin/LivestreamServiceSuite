using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp.Dom;
using System.Linq;

namespace LutheRun
{
    class LSBElementCaption : ILSBElement
    {

        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";

        public static ILSBElement Parse(IElement element)
        {
            var res = new LSBElementCaption();
            res.Caption = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent ?? "";
            res.SubCaption = element.QuerySelectorAll(".subcaption-text").FirstOrDefault()?.TextContent ?? "";
            return res;

        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_CAPTION. Caption:'{Caption}' SubCaption:'{SubCaption}'";
        }

        public string XenonAutoGen()
        {
            // for now just make a title slide:: and flag it as optional
            return $"/// <XENON_AUTO_GEN optional=\"true\">\r\n#2title(\"{Caption.Replace('\"', '\'')}\", \"{SubCaption.Replace('\"', '\'')}\", \"horizontal\")\r\n/// </XENON_AUTO_GEN>";
        }
    }
}
