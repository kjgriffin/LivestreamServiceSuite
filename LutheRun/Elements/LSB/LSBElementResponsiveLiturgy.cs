using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System.Collections.Generic;
using System.Text;

namespace LutheRun.Elements.LSB
{
    internal class LSBElementResponsiveLiturgy : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public IElement SourceHTML { get; private set; } // this will be original root element containing all the liturgy that created this
        private List<IElement> _litPTags { get; set; } = new List<IElement>();

        public static ILSBElement Create(IElement root, List<IElement> ptags)
        {
            return new LSBElementResponsiveLiturgy()
            {
                _litPTags = new List<IElement>(ptags),
                SourceHTML = root
            };

        }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            return Pilot.BlockType.LITURGY_CORPERATE;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_RESPONSIVE_LITURGY.";
        }

        public bool PredictsEmptyContent()
        {
            int a = 0;
            int b = 0;
            return string.IsNullOrWhiteSpace(LSBResponsorialExtractor.ExtractResponsiveLiturgy(_litPTags, ref a, b));
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces, ParsedLSBElement fullInfo, Dictionary<string, string> ExtraFiles)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#liturgyresponsive".Indent(indentDepth, indentSpaces));
            sb.AppendLine("{".Indent(indentDepth, indentSpaces));
            indentDepth++;
            sb.AppendLine(LSBResponsorialExtractor.ExtractResponsiveLiturgy(_litPTags, ref indentDepth, indentSpaces));
            indentDepth--;
            sb.Append("}".Indent(indentDepth, indentSpaces));
            sb.AppendLine(PostsetCmd);
            return sb.ToString();
        }
    }

}
