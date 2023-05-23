using AngleSharp.Dom;

using LutheRun.Parsers;
using LutheRun.Pilot;

namespace LutheRun.Elements.Interface
{
    public interface ILSBElement
    {

        public string PostsetCmd { get; set; }
        public string DebugString();
        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpace);


        public IElement SourceHTML { get; }

        internal BlockType BlockType();
    }
}
