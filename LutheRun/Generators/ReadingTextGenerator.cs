using Concord;

using LutheRun.Parsers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LutheRun.Generators
{
    public static class ReadingTextGenerator
    {
        public static string GenerateXenonComplexReading(BibleTranslations translation, string reference, int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();
            LSBReferenceUnpacker refdecoder = new LSBReferenceUnpacker();
            var sections = refdecoder.ParseSections(reference);
            IBibleAPI bible = BibleBuilder.BuildAPI(translation);

            foreach (var section in sections)
            {
                sb.AppendLine("<block>".Indent(indentDepth, indentSpaces));
                indentDepth++;
                foreach (var vlist in refdecoder.EnumerateVerses(section, bible))
                {
                    BuildVerseString(bible.GetVerse(vlist.Book, vlist.Chapter, vlist.Verse), sb, indentDepth, indentSpaces);
                }
                indentDepth--;
                sb.AppendLine("</block>".Indent(indentDepth, indentSpaces));
            }

            return sb.ToString();
        }

        private static void BuildVerseString(IBibleVerse verse, StringBuilder sb, int indentDepth, int indentSpaces)
        {
            sb.AppendLine($"<text style='superscript' rules='nbspn'>{verse.Number}</text>".Indent(indentDepth, indentSpaces));

            var hunks = verse.Text.Split("<br/>");

            foreach (var hunk in hunks)
            {
                string hunkEscape = hunk.Replace("}", "}}");
                sb.AppendLine($"<text>{hunkEscape}</text>".Indent(indentDepth, indentSpaces));
                if (hunk != hunks.Last())
                {
                    sb.AppendLine("<text xml:space=\"preserve\">  </text>".Indent(indentDepth, indentSpaces));
                }
            }
        }

    }
}
