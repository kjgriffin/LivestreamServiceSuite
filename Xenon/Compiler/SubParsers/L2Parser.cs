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

namespace Xenon.Compiler.SubParsers
{


    class ResponsiveStatement
    {
        public string Speaker { get; set; }
        public List<TextRun> Content { get; set; }
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
