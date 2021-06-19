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

        public static IEnumerable<T> ItemAsEnumerable<T>(this T item)
        {
            yield return item;
        }

    }
}
