using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LutheRun
{
    class LSBElementHymn : ILSBElement
    {

        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";
        public string Copyright { get; private set; } = "";

        private bool IsText { get; set; } = false;

        private List<HymnTextVerse> TextVerses = new List<HymnTextVerse>();

        public static LSBElementHymn Parse(IElement element)
        {
            // all hymns have caption and subcaption
            var res = new LSBElementHymn();
            res.Caption = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent ?? "";
            res.SubCaption = element.QuerySelectorAll(".subcaption-text").FirstOrDefault()?.TextContent ?? "";

            // then parse the lsb-content (could be either text or image)
            var content = element.Children.Where(c => c.LocalName == "lsb-content").FirstOrDefault();

            foreach (var child in content.Children)
            {
                if (child.ClassList.Contains("numbered-stanza"))
                {
                    res.IsText = true;
                    HymnTextVerse verse = new HymnTextVerse();
                    verse.Number = child.QuerySelectorAll(".stanza-number").FirstOrDefault()?.TextContent ?? "";
                    //var s = Regex.Replace(child.TextContent, @"^\d+", "");
                    //verse.Lines = s.Split(new string[] { "\r\n", "\r", "\n", Environment.NewLine, ".", ",", ":", ";", "!", "?", "    " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    //res.TextVerses.Add(verse);
                    verse.Lines = child.ParagraphText();
                    res.TextVerses.Add(verse);
                }
                else if (child.ClassList.Contains("image"))
                {

                }
                else if (child.ClassList.Contains("copyright"))
                {
                    res.Copyright = child.StrippedText();
                }
            }

            return res;
        }

        public string DebugString()
        {
            if (IsText)
            {
                return $"///XENON DEBUG::Parsed as LSB_ELEMENT_HYMN-TEXT. Caption:'{Caption}' SubCaption:'{SubCaption}''";
            }
            return "";
        }

        public string XenonAutoGen()
        {
            if (IsText)
            {
                return XenonAutoGenTextHymn();
            }
            return "";
        }


        private string XenonAutoGenTextHymn()
        {
            StringBuilder sb = new StringBuilder();
            var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");
            string title = "Hymn";
            string name = match.Groups["name"]?.Value.Trim() ?? "";
            string number = "LSB " + match.Groups["number"]?.Value.Trim() ?? "";
            string tune = "";
            string copyright = Copyright;
            sb.Append($"#texthymn(\"{title}\", \"{name}\", \"{tune}\", \"{number}\", \"{copyright}\") {{\r\n");

            foreach (var verse in TextVerses)
            {
                string verseinsert = verse.Number != string.Empty ? $"(Verse {verse.Number})" : "";
                sb.AppendLine($"#verse{verseinsert} {{");
                foreach (var line in verse.Lines)
                {
                    sb.AppendLine(line.Trim());
                }
                sb.AppendLine("}");
            }
            sb.Append("}");
            return sb.ToString();
        }


        class HymnTextVerse
        {
            public string Number { get; set; } = "";
            public List<string> Lines { get; set; } = new List<string>();
        }


    }

}
