using AngleSharp.Dom;

using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System.Collections.Generic;

namespace LutheRun.Elements.Interface
{
    public interface ILSBElement
    {

        public string PostsetCmd { get; set; }
        public string DebugString();
        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace, ParsedLSBElement fullInfo, Dictionary<string, string> ExtraFiles);


        public IElement SourceHTML { get; }

        internal BlockType BlockType(LSBImportOptions importOptions);
    }
}
