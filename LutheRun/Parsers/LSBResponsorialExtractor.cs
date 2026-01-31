using AngleSharp.Dom;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LutheRun.Parsers
{
    internal static class LSBResponsorialExtractor
    {

        internal interface ILineWritable
        {
            void Write(StringBuilder sb, ref int indentDepth, int indentSpaces);
        }

        internal class LiturgicalStatement : ILineWritable
        {
            public string Speaker { get; set; }
            public List<TextBlock> TextSegments { get; set; } = new List<TextBlock>();

            public void Write(StringBuilder sb, ref int indentDepth, int indentSpaces)
            {
                sb.Append($"<line speaker='{Speaker}'>".Indent(indentDepth, indentSpaces));
                indentDepth++;
                foreach (var run in TextSegments)
                {
                    sb.AppendLine();
                    run.Write(sb, ref indentDepth, indentSpaces);
                }
                sb.AppendLine();
                indentDepth--;
                sb.Append("</line>".Indent(indentDepth, indentSpaces));
            }

            public static LiturgicalStatement Create(string speaker, string content)
            {
                return new LiturgicalStatement
                {
                    Speaker = speaker,
                    TextSegments = new List<TextBlock>()
                    {
                        new TextBlock(content),
                    }
                };
            }
        }

        internal class Paragraph : ILineWritable
        {
            public List<TextBlock> TextSegments { get; set; } = new List<TextBlock>();

            public void Write(StringBuilder sb, ref int indentDepth, int indentSpaces)
            {
                sb.Append($"<block>".Indent(indentDepth, indentSpaces));
                indentDepth++;
                foreach (var run in TextSegments)
                {
                    sb.AppendLine();
                    run.Write(sb, ref indentDepth, indentSpaces);
                }
                sb.AppendLine();
                indentDepth--;
                sb.Append("</block>".Indent(indentDepth, indentSpaces));
            }

        }



        internal struct TextBlock
        {
            public string Text { get; private set; }
            public string AltFont { get; private set; } = "";
            public bool WhitespaceSensitive { get; private set; } = false;
            public string RawAttr { get; private set; } = "";
            public string RawStyle { get; private set; } = "";

            public TextBlock(string text, bool isbold = false, bool issymbol = false, bool whitespacesensitive = false)
            {
                Text = text;
                if (isbold)
                {
                    RawStyle = "bold";
                }
                AltFont = issymbol ? "LSBSymbol" : "";
                WhitespaceSensitive = whitespacesensitive;
            }
            public TextBlock(string text, string rawstyle = "", string altfont = "", string rawattr = "")
            {
                Text = text;
                RawStyle = rawstyle;
                AltFont = altfont;
                WhitespaceSensitive = false;
                RawAttr = rawattr;
            }
            public TextBlock(string text)
            {
                Text = text;
            }


            public void Write(StringBuilder sb, ref int indentDepth, int indentSpaces)
            {
                string font = string.IsNullOrWhiteSpace(AltFont) ? "" : $" altfont='{AltFont}'";
                string style = string.IsNullOrWhiteSpace(RawStyle) ? "" : $" style='{RawStyle}'";
                string rat = string.IsNullOrWhiteSpace(RawAttr) ? "" : $" {RawAttr}";

                string ws = WhitespaceSensitive ? " xml:space=\"preserve\"" : "";


                sb.Append($"<text{font}{style}{ws}{rat}>".Indent(indentDepth, indentSpaces));
                sb.Append(Text);
                sb.Append("</text>");
            }
        }

        public static string GenerateSource(List<ILineWritable> lines, ref int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var line in lines)
            {
                line.Write(sb, ref indentDepth, indentSpaces);
                if (lines.IndexOf(line) < lines.Count - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public static string ExtractResponsivePoetry(List<IElement> p_elements, ref int indentDepth, int indentSpaces)
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
                var classListItems = ptags.ClassList.Where(t => t.StartsWith("lsb")).Select(t => t.Split(new string[] { "_", "-" }, StringSplitOptions.RemoveEmptyEntries)).ToList();
                if (ptags.ClassList.Contains("lsb-responsorial-continued_poetry") || classListItems.Any(x => x.Contains("poetry") && x.Contains("continued")))
                {
                    // keep adding to the same 'logical line'
                    snum = 1;
                    line = lastline;
                    // add some leading whitespace
                    // 2 spaces... ????
                    line.TextSegments.Add(new TextBlock("  ", false, false, whitespacesensitive: true));
                }
                else if (ptags.ClassList.Contains("lsb-responsorial_poetry") || classListItems.Any(x => x.Contains("poetry")))
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
                        // for responsive poetry this is used to indicate within a p tag, that there's some logical requirement to start a new line
                        // we'll borrow existing functionality instead and just end and start a new line (with the same speaker)
                        //if (!(string.IsNullOrEmpty(line.Speaker) && line.TextSegments.Any(x => string.IsNullOrWhiteSpace(x.Text))) && line != lastline)
                        //{
                        //    var speakerctd = line.Speaker;
                        //    Lines.Add(line);
                        //    line = new LiturgicalStatement();
                        //    line.Speaker = speakerctd;
                        //}

                        // for Psalm poetry- 1 space seems best
                        line.TextSegments.Add(new TextBlock(" ", whitespacesensitive: true));
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
            return GenerateSource(Lines.Select(x => x as ILineWritable).ToList(), ref indentDepth, indentSpaces);

        }

        public static string ExtractPoetryReading(List<IElement> p_elements, ref int indentDepth, int indentSpaces, bool keepVerseNumbering = true)
        {
            List<Paragraph> Lines = new List<Paragraph>();

            // 1. Iterate over childern (expect all to be <p> tags)
            foreach (var ptag in p_elements.Where(c => c.LocalName == "p"))
            {
                Paragraph rline = new Paragraph();

                string rcontent = ptag.InnerHtml;

                // expected output
                // <p>
                // <text>content</text>
                // <text style='superscript' rules='non-break-next'>12</text>
                // <text>more content</text>
                // </p>

                // replace whitespace with marker
                // replace verse with marker

                // then split
                // then dump into textblocks


                string wformat = Regex.Replace(rcontent, "<span class=\"Apple-tab-span\" style=\"white-space:pre; mso-tab-count: 1;\">\\s<\\/span>", " "); // we'll handle tabs with single space
                string vformat = Regex.Replace(wformat, "<sup class=\"verse-number\">(?<vnum>\\d+)</sup>", m => $" #[{m.Groups["vnum"].Value}] ");

                // seem to also need to remove &nbsp; since that got slipped in
                vformat = Regex.Replace(vformat, @"&nbsp;", " ");

                // may want to preserve/handle <br> ....
                // either remove or keep/translate to double space...
                var segments = vformat.Split("<br>", StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < segments.Length; i++)
                {
                    // split out into verses...
                    var chunks = segments[i].Split("#", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var chunk in chunks)
                    {
                        // check for verse number
                        string ctext = chunk;
                        var match = Regex.Match(chunk, @"^\[(?<vnum>\d+)\]");
                        if (match.Success)
                        {
                            ctext = Regex.Replace(chunk, @"^\[\d+\]\s+", "");
                            rline.TextSegments.Add(new TextBlock(match.Groups["vnum"].Value, rawstyle: "superscript", rawattr: "rules='nbspn'"));
                        }

                        if (!string.IsNullOrWhiteSpace(ctext))
                        {
                            rline.TextSegments.Add(new TextBlock(ctext));
                        }
                    }

                    if (i < segments.Length - 1)
                    {
                        rline.TextSegments.Add(new TextBlock("  ", whitespacesensitive: true));
                    }

                }

                Lines.Add(rline);

            }
            return GenerateSource(Lines.Select(x => x as ILineWritable).ToList(), ref indentDepth, indentSpaces);

        }


        public static string ExtractResponsiveLiturgy(List<IElement> p_elements, ref int indentDepth, int indentSpaces)
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
            return GenerateSource(Lines.Select(x => x as ILineWritable).ToList(), ref indentDepth, indentSpaces);
        }

        public static string ExtractResponsiveLiturgy(IElement sourceHTML, ref int indentDepth, int indentSpaces)
        {
            return ExtractResponsiveLiturgy(sourceHTML.Children.Where(c => c.LocalName == "p").ToList(), ref indentDepth, indentSpaces);
        }

    }
}
