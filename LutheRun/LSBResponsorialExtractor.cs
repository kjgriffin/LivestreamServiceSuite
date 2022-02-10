using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun
{
    internal static class LSBResponsorialExtractor
    {

        internal class LiturgicalStatement
        {
            public string Speaker { get; set; }
            public List<TextBlock> TextSegments { get; set; } = new List<TextBlock>();

            public void Write(StringBuilder sb)
            {
                sb.Append($"<line speaker='{Speaker}'>");
                foreach (var run in TextSegments)
                {
                    run.Write(sb);
                }
                sb.AppendLine();
                sb.Append("</line>");
            }
        }

        internal struct TextBlock
        {
            public string Text { get; private set; }
            public bool Bold { get; private set; } = false;
            public bool SpecialSymbol { get; private set; } = false;
            public bool WhitespaceSensitive { get; private set; } = false;
            public TextBlock(string text, bool isbold = false, bool issymbol = false, bool whitespacesensitive = false)
            {
                Text = text;
                Bold = isbold;
                SpecialSymbol = issymbol;
                WhitespaceSensitive = whitespacesensitive;
            }

            public void Write(StringBuilder sb)
            {
                string font = SpecialSymbol ? "altfont='LSBSymbol'" : "";
                string bold = Bold ? "style='bold'" : "";
                string prefix = SpecialSymbol || Bold ? " " : "";
                string ws = WhitespaceSensitive ? "xml:space=\"preserve\"" : "";
                sb.AppendLine();
                sb.Append("  ");
                sb.Append($"<text{prefix}{font}{(Bold ? " " : "")}{bold}{(WhitespaceSensitive ? " " : "")}{ws}>");
                sb.Append(Text);
                sb.Append("</text>");
            }
        }

        public static string GenerateSource(List<LiturgicalStatement> lines)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var line in lines)
            {
                line.Write(sb);
                if (lines.IndexOf(line) < lines.Count - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public static string ExtractResponsiveLiturgy(List<IElement> p_elements)
        {
            List<LiturgicalStatement> Lines = new List<LiturgicalStatement>();
            /* Expected Structure: 
             * - each spoken line wraped in a paragraph
             * - first span identifies the speaker
             * - next span for break
             * - subsequent spans for content (optional br identifes good places to chunk large segments)
             */

            LiturgicalStatement lastline = null;
            // 1. Iterate over childern (expect all to be <p> tags)
            foreach (var ptags in p_elements.Where(c => c.LocalName == "p"))
            {
                LiturgicalStatement line = new LiturgicalStatement();
                int snum = 0;
                // build a Liturgy-Response
                // go through all childern
                if (ptags.ClassList.Contains("lsb-responsorial-continued"))
                {
                    // keep adding to the same 'logical line'
                    snum = 1;
                    line = lastline;
                    // add some leading whitespace
                    // 2 spaces... ????
                    line.TextSegments.Add(new TextBlock("  ", false, false, whitespacesensitive: true));
                }
                else if (ptags.ClassList.Contains("lsb-responsorial"))
                {
                }
                else
                {
                    // ignore it
                    continue;
                }

                foreach (var line_item in ptags.Children)
                {
                    // Expect the speaker
                    if (snum == 0)
                    {
                        line.Speaker = line_item.TextContent;
                    }
                    else if (snum > 0 && line_item.ClassList.Contains("Apple-tab-span"))
                    {
                        // ignore it
                    }
                    else if (line_item.LocalName == "br")
                    {
                        // handle br by forcing space
                        // not sure it this really is the best way... but perhaps??
                        // at worst we'll get extra space?
                        // seems like LSB uses <br> for logical breaks though, so extra space may not be the worst idea

                        // single or double space???
                        line.TextSegments.Add(new TextBlock("  ", whitespacesensitive: true));
                    }
                    else if (line_item.LocalName == "span")
                    {
                        // is text content of some sort

                        bool specialChar = line_item.ClassList.Contains("lsb-symbol");
                        bool boldFont = line_item.Attributes.Any(x => x.LocalName == "font-weight" && x.Value == "bold");

                        line.TextSegments.Add(new TextBlock(line_item.TextContent, boldFont, specialChar));
                    }
                    snum++;
                }

                if (!(string.IsNullOrEmpty(line.Speaker) && line.TextSegments.Any(x => string.IsNullOrWhiteSpace(x.Text))) && line != lastline)
                {
                    Lines.Add(line);
                }
                lastline = line;
            }
            return GenerateSource(Lines);
        }

        public static string ExtractResponsiveLiturgy(IElement sourceHTML)
        {
            return ExtractResponsiveLiturgy(sourceHTML.Children.Where(c => c.LocalName == "p").ToList());
        }

    }
}
