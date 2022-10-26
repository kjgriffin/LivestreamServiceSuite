using AngleSharp.Dom;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LutheRun
{
    internal class LSBElementUnknownFromContent : ILSBElement
    {
        public string PostsetCmd { get; set; }
        public IElement SourceHTML { get; private set; } // this will be original root element containing whatever created this 

        public string TextContent { get; set; } = "";

        public static ILSBElement Create(IElement root, string text)
        {
            return new LSBElementUnknownFromContent()
            {
                SourceHTML = root,
                TextContent = text,
            };

        }

        public BlockType BlockType()
        {
            return LutheRun.BlockType.LITURGY_CORPERATE;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_UNKNOWN_FROM_CONTENT.";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {
            // TODO: have some sort of general method for dealing with this...
            StringBuilder sb = new StringBuilder();

            // for now.... just extract the content and put it in as a comment??


            if (!string.IsNullOrWhiteSpace(TextContent))
            {
                sb.AppendLine("// Found extra content that's not quite liturgy...".Indent(indentDepth, indentSpaces));
                sb.AppendLine($"// {TextContent}".Indent(indentDepth, indentSpaces));
            }

            return sb.ToString();
        }
    }

}
