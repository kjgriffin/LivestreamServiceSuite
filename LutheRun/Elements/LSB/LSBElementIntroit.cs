using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static LutheRun.Parsers.LSBResponsorialExtractor;

namespace LutheRun.Elements.LSB
{
    class LSBElementIntroit : ILSBElement
    {


        public string Caption { get; private set; }
        public List<(bool hasspeaker, string speaker, string text)> Lines { get; private set; } = new List<(bool hasspeaker, string speaker, string text)>();
        public string PostsetCmd { get; set; } = "";

        public IElement SourceHTML { get; private set; }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            return Pilot.BlockType.LITURGY_CORPERATE;
        }

        public static ILSBElement Parse(IElement element)
        {
            var lines = new List<(bool, string, string)>();
            foreach (var lsbcontent in element.Children.Where(c => c.LocalName == "lsb-content"))
            {
                lines.AddRange(lsbcontent.ExtractTextAsLiturgicalPoetry());
            }

            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;

            return new LSBElementIntroit() { Lines = lines, Caption = caption?.Caption, SourceHTML = element };

        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_INTROIT'";
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces, ParsedLSBElement fullInfo)
        {
            if (lSBImportOptions.UseComplexIntroit)
            {
                return AsComplex(ref indentDepth, indentSpaces);
            }
            // for now we'll have to assume that a 2 line cadence will work
            // this may not at all be correct

            StringBuilder sb = new StringBuilder();

            bool showspeaker = false;
            // only show speakers if introit has multiple speakers
            var firstspeaker = Lines.FirstOrDefault().speaker;
            showspeaker = !Lines.All(x => x.speaker == firstspeaker);

            string lastspeaker = "$";

            int tcmdnum = 0;
            int totalcomands = (int)Math.Ceiling(Lines.Count / 2.0);
            var postset = PostsetCmd.ExtractPostsetValues();

            int lnum = 0;
            foreach (var line in Lines)
            {
                if (lnum > 1)
                {
                    lnum = 0;
                    tcmdnum += 1;
                }

                if (line.hasspeaker)
                {
                    lastspeaker = line.speaker;
                }

                if (lnum == 0)
                {
                    sb.Append("#tlverse(\"");
                    sb.Append(Caption);
                    sb.Append("\", ");
                    sb.Append("\"\", ");
                    sb.Append("\"");
                    sb.Append(showspeaker ? "true" : "false");
                    sb.AppendLine("\") {");

                    sb.Append(lastspeaker);
                    sb.Append(" ");
                    sb.AppendLine(line.text);
                }
                else
                {
                    sb.Append(lastspeaker);
                    sb.Append(" ");
                    sb.AppendLine(line.text);
                    sb.Append("}");
                    string fval = "";
                    string lval = "";
                    if (tcmdnum == 0)
                    {
                        if (postset.first != -1)
                        {
                            fval = $"first={postset.first}";
                        }
                    }
                    if (tcmdnum == totalcomands - 1)
                    {
                        if (postset.last != -1)
                        {
                            lval = $"last={postset.last}";
                        }
                    }
                    if (fval != string.Empty || lval != string.Empty)
                    {
                        sb.Append("::postset(");
                        if (fval != string.Empty)
                        {
                            sb.Append(fval);
                            if (lval != string.Empty)
                            {
                                sb.Append(", ");
                            }
                        }
                        if (lval != string.Empty)
                        {
                            sb.Append(lval);
                        }
                        sb.Append(")");
                    }
                    sb.AppendLine();
                }
                lnum += 1;
            }

            if (lnum == 1)
            {
                sb.Append("}");

                string fval = "";
                string lval = "";
                if (tcmdnum == 0)
                {
                    if (postset.first != -1)
                    {
                        fval = $"first={postset.first}";
                    }
                }
                if (tcmdnum == totalcomands - 1)
                {
                    if (postset.last != -1)
                    {
                        lval = $"last={postset.last}";
                    }
                }
                if (fval != string.Empty || lval != string.Empty)
                {
                    sb.Append("::postset(");
                    if (fval != string.Empty)
                    {
                        sb.Append(fval);
                        if (lval != string.Empty)
                        {
                            sb.Append(", ");
                        }
                    }
                    if (lval != string.Empty)
                    {
                        sb.Append(lval);
                    }
                    sb.Append(")");
                }

                sb.AppendLine();
            }

            return sb.ToString();

        }

        private string AsComplex(ref int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("#tlit {".Indent(indentDepth, indentSpaces));

            indentDepth++;

            sb.AppendLine($"title={{{Caption}}}".Indent(indentDepth, indentSpaces));
            sb.AppendLine($"title={{{Caption}}}".Indent(indentDepth, indentSpaces));

            sb.AppendLine("content={".Indent(indentDepth, indentSpaces));

            indentDepth++;

            foreach (var line in Lines)
            {
                LiturgicalStatement.Create(line.hasspeaker ? line.speaker : "", line.text).Write(sb, ref indentDepth, indentSpaces);
                if (line != Lines.Last())
                {
                    sb.AppendLine();
                }
            }

            indentDepth--;
            sb.AppendLine("}".Indent(indentDepth, indentSpaces));
            indentDepth--;
            sb.AppendLine("}".Indent(indentDepth, indentSpaces));

            return sb.ToString();
        }
    }
}
