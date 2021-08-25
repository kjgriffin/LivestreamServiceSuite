using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace LutheRun
{
    static class Helpers
    {

        public static string StringTogether(this IEnumerable<string> s, string seperator = "")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in s)
            {
                sb.Append(str);
                if (!string.IsNullOrEmpty(seperator))
                {
                    sb.Append(seperator);
                }
            }
            if (!string.IsNullOrEmpty(seperator))
            {
                sb.Remove(sb.Length - seperator.Length, seperator.Length);
            }
            return sb.ToString();
        }

        public static string StrippedText(this IElement element)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> lines = element.Text().Split(new string[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string s = Regex.Replace(line, @"\s", " ").Trim();
                s = Regex.Replace(s, @"(?<ender>[;,.!?:])", "${ender} ");
                s = Regex.Replace(s, @"\s+", " ").Trim();
                if (s != string.Empty)
                {
                    sb.AppendLine(s);
                }
            }

            return sb.ToString().Trim();
        }

        public static List<string> ParagraphText(this IElement paragraph)
        {
            paragraph.RemoveChild(paragraph.FirstChild);
            paragraph.RemoveChild(paragraph.FirstChild);
            string s = paragraph.InnerHtml;
            s = s.Replace("<br>", Environment.NewLine);
            s = s.Replace("&nbsp;", " ");

            return s.Split(Environment.NewLine).ToList();
        }

        public static List<string> ParagraphText2(this IElement paragraph)
        {
            // get all the text of the paragraph
            string s = paragraph.InnerHtml;

            s = s.Replace("<br>", Environment.NewLine);
            s = s.Replace("&nbsp;", " ");

            s = Regex.Replace(s, "<span.*\\/span>", "");

            return s.Split(Environment.NewLine).ToList();
        }

        public static List<(bool hasspeaker, string speaker, string value)> ExtractTextAsLiturgy(this IElement element)
        {
            List<(bool hasspeaker, string speaker, string value)> lines = new List<(bool hasspeaker, string speaker, string value)>();

            // perform a recursive decent over the element tree
            // be context-aware and extract text performing replacements as nessecary

            bool foundspeaker = false;
            bool expectspeaker = false;
            string speaker = "";
            StringBuilder sb = new StringBuilder();

            List<IElement> flattree = element.ElementTreeToFlatList();

            foreach (var node in flattree)
            {
                if (node.LocalName == "lsb-content")
                {
                    continue;
                }
                if (node.LocalName == "p")
                {
                    // begin new line
                    // add old line
                    if (sb.Length > 0)
                    {
                        lines.Add((foundspeaker, speaker, sb.ToString()));
                        sb.Clear();
                        speaker = "";
                        foundspeaker = false;
                    }

                    if (node.ClassList.Contains("lsb-responsorial-continued"))
                    {
                        expectspeaker = false;
                    }
                    else
                    {
                        expectspeaker = true;
                    }
                }
                else if (node.LocalName == "br")
                {
                    sb.AppendLine();
                }
                else
                {
                    if (node.ClassList.Contains("lsb-symbol"))
                    {
                        if (expectspeaker)
                        {
                            speaker = node.Text();
                            expectspeaker = false;
                            foundspeaker = true;
                        }
                        else
                        {
                            string s = node.Text().Trim();
                            if (s.Length > 0)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append(" ");
                                }
                                sb.Append(s);
                            }
                        }
                    }
                    else if (node.ClassList.Contains("Apple-tab-span"))
                    {
                        continue;
                    }
                    else
                    {
                        string s = node.Text().Trim();
                        if (s.Length > 0)
                        {
                            if (sb.Length > 0)
                            {
                                sb.Append(" ");
                            }
                            sb.Append(s);
                        }
                    }
                }
            }

            if (sb.Length > 0)
            {
                lines.Add((foundspeaker, speaker, sb.ToString()));
                sb.Clear();
            }

            return lines;
        }

        public static List<(bool hasspeaker, string speaker, string value)> ExtractTextAsLiturgicalPoetry(this IElement element)
        {
            List<(bool hasspeaker, string speaker, string value)> lines = new List<(bool hasspeaker, string speaker, string value)>();

            // perform a recursive decent over the element tree
            // be context-aware and extract text performing replacements as nessecary

            bool foundspeaker = false;
            bool expectspeaker = false;
            string speaker = "";
            StringBuilder sb = new StringBuilder();

            List<IElement> flattree = element.ElementTreeToFlatList();

            foreach (var node in flattree)
            {
                if (node.LocalName == "lsb-content")
                {
                    continue;
                }
                if (node.LocalName == "p")
                {
                    // begin new line
                    // add old line
                    if (sb.Length > 0)
                    {
                        lines.Add((foundspeaker, speaker, sb.ToString().Trim()));
                        sb.Clear();
                        speaker = "";
                        foundspeaker = false;
                    }

                    if (node.ClassList.Contains("lsb-responsorial_poetry"))
                    {
                        expectspeaker = true;
                    }

                    var textlines = node.ParagraphText2();
                    foreach (var tl in textlines)
                    {
                        lines.Add((foundspeaker, speaker, tl.Trim()));
                    }
                }
                else if (node.LocalName == "br")
                {
                    // for poetry, line breaks seem to be well placed, so we'll borrow LSB's nicely layed out lines and hope they fit nicely
                    if (sb.Length > 0)
                    {
                        lines.Add((foundspeaker, speaker, sb.ToString().Trim()));
                        sb.Clear();
                        // here we'll let speakers continue until we definitivly find a new one... this may not work with symbol detection
                    }
                }
                else
                {
                    if (node.ClassList.Contains("lsb-symbol"))
                    {
                        if (expectspeaker)
                        {
                            speaker = node.Text();
                            expectspeaker = false;
                            foundspeaker = true;
                        }
                        else
                        {
                            AddNodeText(sb, node);
                        }
                    }
                    else if (node.ClassList.Contains("Apple-tab-span"))
                    {
                        continue;
                    }
                    else
                    {
                        AddNodeText(sb, node);
                    }
                }
            }

            if (sb.Length > 0)
            {
                lines.Add((foundspeaker, speaker, sb.ToString().Trim()));
                sb.Clear();
            }

            return lines;
        }

        private static void AddNodeText(StringBuilder sb, IElement node)
        {
            string nt = node.Text();
            string s = nt.Trim();
            if (s.Length > 0)
            {
                if (sb.Length > 0 && char.IsWhiteSpace(nt.FirstOrDefault()))
                {
                    sb.Append(" ");
                }
                sb.Append(s);
                if (sb.Length > 0 && nt.Length > 1 && char.IsWhiteSpace(nt.LastOrDefault()))
                {
                    sb.Append(" ");
                }
            }
        }

        public static (int first, int last, int all) ExtractPostsetValues(this string cmd)
        {
            var match = Regex.Match(cmd, @"::postset\(\s*(?<par1>\w+)\s*=\s*(?<val1>\d+)(,\s*(?<par2>\w+)\s*=\s*(?<val2>\d+))?(,\s*(?<par3>\w+)\s*=\s*(?<val3>\d+))?\s*\)");
            Dictionary<string, int> extractedvalues = new Dictionary<string, int>();
            if (match.Success)
            {
                if (match.Groups["par1"].Success)
                {
                    extractedvalues[match.Groups["par1"].Value] = Convert.ToInt32(match.Groups["val1"].Value);
                }
                if (match.Groups["par2"].Success)
                {
                    extractedvalues[match.Groups["par2"].Value] = Convert.ToInt32(match.Groups["val2"].Value);
                }
                if (match.Groups["par3"].Success)
                {
                    extractedvalues[match.Groups["par3"].Value] = Convert.ToInt32(match.Groups["val3"].Value);
                }
            }

            int first = -1;
            int last = -1;
            int all = -1;

            if (extractedvalues.ContainsKey("first"))
            {
                extractedvalues.TryGetValue("first", out first);
            }
            if (extractedvalues.ContainsKey("last"))
            {
                extractedvalues.TryGetValue("last", out last);
            }
            if (extractedvalues.ContainsKey("all"))
            {
                extractedvalues.TryGetValue("all", out all);
            }

            return (first, last, all);
        }

        public static List<IElement> ElementTreeToFlatList(this IElement root)
        {
            List<IElement> list = new List<IElement>();

            list.Add(root);

            foreach (var child in root.Children)
            {
                list.AddRange(ElementTreeToFlatList(child));
            }

            return list;
        }


        public static IEnumerable<T> ItemAsEnumerable<T>(this T item)
        {
            yield return item;
        }

    }
}
