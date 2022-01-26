using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Xml;

using Xenon.Helpers;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;

namespace Xenon.Compiler.SubParsers
{


    class ResponsiveStatement
    {
        // TODO: perhaps make this extensible
        static readonly string[] BOLDSPEAKERS = new[] { "C", "R" };

        public string Speaker { get; set; }
        public List<TextRun> Content { get; set; }


        public TextBlurb SpeakerBlurb(ResponsiveLiturgySlideLayoutInfo layout)
        {
            FontStyle style = (FontStyle)layout.Textboxes.First().SpeakerFont.Style;
            if (BOLDSPEAKERS.Contains(Speaker))
            {
                style = FontStyle.Bold;
            }
            return new TextBlurb(Point.Empty, Speaker, layout.Textboxes.First().SpeakerFont.Name, style, 36);
        }

        public List<TextBlurb> ContentBlurbs(ResponsiveLiturgySlideLayoutInfo layout)
        {
            // further split the content of each Content TextRun into words

            // for now we'll enforce keeping punctuation attached to any words it is attached to
            // eg. 'This is a sentence.' should be split into ['This', 'is', 'a', 'sentence.']
            // we'll also mark whitespace as such
            // this can give the layout engine extra help deciding what to do
            const string split = "(\\s)";

            List<TextBlurb> blurbs = new List<TextBlurb>();

            foreach (var run in Content)
            {
                var matches = Regex.Split(run.Text, split);
                foreach (var word in matches)
                {
                    blurbs.Add(new TextBlurb(Point.Empty,
                        word,
                        string.IsNullOrWhiteSpace(run.AltFont) ? layout.Textboxes.First().Font.Name : run.AltFont,
                        run.FStyle | (FontStyle)layout.Textboxes.First().Font.Style,
                        layout.Textboxes.First().Font.Size,
                        word == " ",
                        word == Environment.NewLine));
                }
            }

            return blurbs;
        }

    }

    class TextRun
    {
        public string Text { get; private set; }
        public string AltFont { get; private set; }
        public FontStyle FStyle { get; private set; }

        public TextRun(string text, string fname = "", FontStyle fstyle = FontStyle.Regular)
        {
            Text = text;
            AltFont = fname;
            FStyle = fstyle;
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

            FontStyle fstyle = FontStyle.Regular;
            if (!string.IsNullOrEmpty(style?.Value))
            {
                var styles = style.Value.Split(';');
                if (styles.Contains("bold"))
                {
                    fstyle |= FontStyle.Bold;
                }
                if (styles.Contains("italic"))
                {
                    fstyle |= FontStyle.Italic;
                }
            }

            return new TextRun(node.InnerText, font?.Value ?? "", fstyle);
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



    }
}
