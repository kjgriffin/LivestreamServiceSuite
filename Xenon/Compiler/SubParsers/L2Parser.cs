using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Xml;

using Xenon.Helpers;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.Compiler.SubParsers
{


    class ResponsiveStatement
    {
        public string Speaker { get; set; }
        public List<TextRun> Content { get; set; }


        public TextBlurb SpeakerBlurb(LiturgyTextboxLayout layout)
        {
            int style = layout.SpeakerFont.Style;
            return new TextBlurb(Point.Empty, Speaker, layout.SpeakerFont.Name, style, 36, hexColor: layout.SpeakerColor.Hex);
        }

        public List<TextBlurb> ContentBlurbs(LiturgyTextboxLayout layout)
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

            if (layout.LineFonts.TryGetValue(Speaker, out LWJFont font) == true)
            {
                linestyle = font.Style;
                linesize = font.Size;
                linefont = font.Name;
            }


            foreach (var run in Content)
            {
                var matches = Regex.Split(run.Text, split);
                foreach (var word in matches)
                {
                    blurbs.Add(new TextBlurb(Point.Empty,
                        word,
                        string.IsNullOrWhiteSpace(run.AltFont) ? linefont : run.AltFont,
                        run.AltStyle != -1 ? run.AltStyle : linestyle,
                        linesize,
                        word == " ",
                        word == Environment.NewLine,
                        !string.IsNullOrWhiteSpace(run.AltColorHex) ? run.AltColorHex : layout.FColor.Hex));
                }
            }

            return blurbs;
        }

    }

    class TextRun
    {
        public string Text { get; private set; }
        public string AltFont { get; private set; }
        public int AltStyle { get; private set; }
        public string AltColorHex { get; private set; }

        public TextRun(string text, string fname = "", int fstyle = -1, string altcolhex = null)
        {
            Text = text;
            AltFont = fname;
            AltStyle = fstyle;
            AltColorHex = altcolhex;
        }

        public static TextRun Create(XmlNode node)
        {
            if (node.LocalName != "text")
            {
                return null;
            }
            // text should be a leaf node, so just fill its content and parse its attributes
            var font = node.Attributes.GetNamedItem("altfont");
            var style = node.Attributes.GetNamedItem("style");
            var color = node.Attributes.GetNamedItem("color");

            int fstyle = -1;
            if (!string.IsNullOrEmpty(style?.Value))
            {
                fstyle = 0;
                var styles = style.Value.Split(';');
                if (styles.Contains("regular"))
                {
                    fstyle = 0;
                }
                if (styles.Contains("bold"))
                {
                    fstyle |= (int)FontStyle.Bold;
                }
                if (styles.Contains("italic"))
                {
                    fstyle |= (int)FontStyle.Italic;
                }
                if (styles.Contains("underline"))
                {
                    fstyle |= (int)FontStyle.Underline;
                }
            }

            return new TextRun(node.InnerText, font?.Value ?? "", fstyle, color?.Value ?? null);
        }

    }


    /// <summary>
    /// A Parser designed to support the <see cref="LanguageKeywordCommand.Liturgy2"/> command.
    /// </summary>
    internal class L2Parser
    {
        public static List<ResponsiveStatement> ParseLiturgyStatements(string input)
        {
            List<ResponsiveStatement> liturgy = new List<ResponsiveStatement>();
            var doc = ToXML(input);

            foreach (XmlNode line in doc.GetElementsByTagName("line"))
            {
                ResponsiveStatement part = new ResponsiveStatement();
                part.Speaker = line.Attributes.GetNamedItem("speaker").Value;
                List<TextRun> content = new List<TextRun>();
                foreach (XmlNode run in line.ChildNodes)
                {
                    content.Add(TextRun.Create(run));
                }
                part.Content = content;
                liturgy.Add(part);
            }

            return liturgy;
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
