using AngleSharp.Dom;
using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Pilot;
using System;
using System.Collections.Generic;
using System.Text;

namespace LutheRun.Elements.LSB
{
    class LSBElementLiturgy : ILSBElement
    {

        public string LiturgyText { get; private set; }
        public string PostsetCmd { get; set; } = "";

        public IElement SourceHTML { get; private set; }

        public BlockType BlockType()
        {
            return Pilot.BlockType.LITURGY_CORPERATE;
        }

        public static ILSBElement Parse(IElement element)
        {
            // process liturgy text

            StringBuilder sb = new StringBuilder();

            var lines = element.ExtractTextAsLiturgy();

            foreach (var line in lines)
            {
                if (line.hasspeaker)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.Append(line.speaker);
                    sb.Append(" ");
                    sb.Append(line.value);
                }
                else
                {
                    sb.Append(line.value);
                }
            }

            return new LSBElementLiturgy() { LiturgyText = sb.ToString() };
        }

        public static ILSBElement Create(string liturgyText, IElement source)
        {
            return new LSBElementLiturgy() { LiturgyText = liturgyText, SourceHTML = source };
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_LITURGY. LiturgyText:'{LiturgyText}'";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {

            string litcontent = LSBResponsorialExtractor.ExtractResponsiveLiturgy(SourceHTML, ref indentDepth, indentSpaces);

            if (LiturgyText.Trim() != string.Empty)
            {
                //return "/// <XENON_AUTO_GEN>\r\n#liturgy{\r\n" + LiturgyText + "\r\n}\r\n/// </XENON_AUTO_GEN>";
                if (lSBImportOptions.UseResponsiveLiturgy)
                {
                    return $"#liturgyresponsive{Environment.NewLine}{{{Environment.NewLine}{litcontent}{Environment.NewLine}}}{PostsetCmd}{Environment.NewLine}";
                }
                else
                {
                    return ("#liturgy{\r\n" + LiturgyText + "\r\n}" + PostsetCmd).IndentBlock(indentDepth, indentSpaces);
                }
            }
            return "";
        }
    }
}
