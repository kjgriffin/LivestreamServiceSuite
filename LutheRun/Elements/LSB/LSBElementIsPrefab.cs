using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

namespace LutheRun.Elements.LSB
{
    class LSBElementIsPrefab : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public string Prefab { get; private set; }
        public string SourceText { get; private set; }

        public IElement SourceHTML { get; private set; }

        internal BlockType BType { get; private set; } = Pilot.BlockType.UNKNOWN;

        public LSBElementIsPrefab(string command, string elementtext, IElement source, BlockType bType)
        {
            Prefab = command;
            SourceText = elementtext;
            SourceHTML = source;
            BType = bType;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_PREFAB[{Prefab}]. For Source Element: {SourceText}";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces, ParsedLSBElement fullInfo)
        {
            return $"#{Prefab}".Indent(indentDepth, indentSpaces);
        }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            return BType;
        }
    }
}
