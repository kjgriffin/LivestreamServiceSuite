using AngleSharp.Dom;
using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Pilot;
using System;
using System.Collections.Generic;
using System.Text;

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

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {
            return "";
        }

        public BlockType BlockType()
        {
            return Pilot.BlockType.UNKNOWN;
        }
    }
}
