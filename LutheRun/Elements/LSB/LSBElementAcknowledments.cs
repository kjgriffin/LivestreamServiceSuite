using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System.Collections.Generic;

namespace LutheRun.Elements.LSB
{
    internal class LSBElementAcknowledments : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public IElement SourceHTML { get; private set; }

        public string Text { get; private set; }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            return Pilot.BlockType.IGNORED;
        }

        public string DebugString()
        {
            return "";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace, ParsedLSBElement fullInfo, Dictionary<string, string> ExtraFiles)
        {
            return $"acknowledments={{{Text}}}";
        }

        public static LSBElementAcknowledments Parse(IElement elem)
        {
            return new LSBElementAcknowledments()
            {
                PostsetCmd = "",
                SourceHTML = elem,
                Text = elem?.StrippedText(),
            };
        }
    }
}
