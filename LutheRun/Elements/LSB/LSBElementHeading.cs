using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Pilot;

using System.Linq;

namespace LutheRun.Elements.LSB
{
    class LSBElementHeading : ILSBElement
    {
        public string PostsetCmd { get; set; }

        public string Heading { get; private set; } = "";

        public IElement SourceHTML { get; private set; }

        public static ILSBElement Parse(IElement element)
        {
            var res = new LSBElementHeading();
            res.SourceHTML = element;
            res.Heading = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent;
            return res;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_HEADING. Heading:'{Heading}'";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {
            //return $"/// <XENON_AUTO_GEN>\r\n/// Heading: {Heading.Replace('\"', '\'')}\r\n/// </XENON_AUTO_GEN>";
            return $"// Heading: {Heading.Replace('\"', '\'')}";
        }

        public BlockType BlockType()
        {
            return Pilot.BlockType.UNKNOWN;
        }

    }
}
