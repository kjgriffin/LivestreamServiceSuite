using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System.Collections.Generic;

namespace LutheRun.Elements.LSB
{
    class LSBElementUnknown : ILSBElement
    {

        public string PostsetCmd { get; set; }
        public string Unknown { get; private set; } = "";

        public IElement SourceHTML { get; private set; }

        public static ILSBElement Parse(IElement element)
        {
            return new LSBElementUnknown() { Unknown = element.StrippedText(), SourceHTML = element };
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as UNKNOWN. {Unknown}";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces, ParsedLSBElement fullInfo, Dictionary<string, string> ExtraFiles)
        {
            return "";
        }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            return Pilot.BlockType.UNKNOWN;
        }
    }
}
