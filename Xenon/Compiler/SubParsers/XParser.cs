
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Compiler.SubParsers
{

    class TextBlockPara
    {
        public List<TextRun> Content { get; set; }

        public List<TextBlurb> ContentBlurbs(TextboxLayout layout)
        {
            // further split the content of each Content TextRun into words

            // for now we'll enforce keeping punctuation attached to any words it is attached to
            // eg. 'This is a sentence.' should be split into ['This', 'is', 'a', 'sentence.']
            // we'll also mark whitespace as such
            // this can give the layout engine extra help deciding what to do
            const string split = "(\\s)";

            List<TextBlurb> blurbs = new List<TextBlurb>();

            int linestyle = layout.Font.Style;
            float linesize = layout.Font.Size;
            string linefont = layout.Font.Name;

            foreach (var run in Content)
            {
                var matches = Regex.Split(run.Text, split);
                foreach (var word in matches.Where(x => !string.IsNullOrEmpty(x)))
                {
                    blurbs.Add(new TextBlurb(Point.Empty,
                        word,
                        string.IsNullOrWhiteSpace(run.AltFont) ? linefont : run.AltFont,
                        run.AltStyle != -1 ? run.AltStyle : linestyle,
                        linesize,
                        word == " ",
                        word == Environment.NewLine,
                        !string.IsNullOrWhiteSpace(run.AltColorHex) ? run.AltColorHex : layout.FColor.Hex,
                        run.StyleMod.Contains("superscript") ? 1 : run.StyleMod.Contains("subscript") ? -1 : 0,
                        run.Rules
                        ));
                }
            }

            return blurbs;
        }

    }




    /// <summary>
    /// A Parser designed to support general xml-ish formatted text.
    /// </summary>
    internal class XParser
    {
        public static List<TextBlockPara> ParseXText(string input)
        {
            List<TextBlockPara> paras = new List<TextBlockPara>();
            var doc = ToXML(input);

            foreach (XmlNode line in doc.GetElementsByTagName("block"))
            {
                TextBlockPara part = new TextBlockPara();
                List<TextRun> content = new List<TextRun>();
                foreach (XmlNode run in line.ChildNodes)
                {
                    content.Add(TextRun.Create(run));
                }

                if (content.Any())
                {
                    content.First().AddRule("br");
                }

                part.Content = content;

                paras.Add(part);
            }

            return paras;
        }

        private static XmlDocument ToXML(string input)
        {
            XmlDocument doc = new XmlDocument();
            var doctext = $"<content>{input}</content>";

            doc.LoadXml(doctext);

            return doc;
        }

        public static List<string> ToFormattedXMLLines(string input)
        {
            XmlDocument doc = new XmlDocument();
            var doctext = $"<content>{input}</content>";

            doc.LoadXml(doctext);

            StringBuilder sb = new StringBuilder();
            using (var writer = new XmlTextWriter(new StringWriter(sb)))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                writer.IndentChar = ' ';

                doc.Save(writer);
            }


            string dstr = sb.ToString();
            // need to remove the content tags...

            dstr = dstr.Remove(0, 8);

            List<string> lines = dstr.Split(Environment.NewLine).ToList();

            lines.RemoveAt(0);
            lines.RemoveAt(0);
            lines.Remove(lines.Last());

            return lines.Select(x => x.Remove(0, 4)).ToList();
        }



    }

}
