using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Helpers;

namespace Xenon.Compiler.SubParsers
{


    class ResponsiveStatement
    {
        public string Speaker { get; set; }
        public List<Word> Content { get; set; }
    }

    struct Word
    {
        public string Text { get; private set; }
        public string AltFont { get; private set; }
        public FontStyle FStyle { get; private set; }

        public Word (string text, string fname, FontStyle fstyle)
        {
            Text = text;
            AltFont = fname;
            FStyle = fstyle;
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

            var rawlines = SplitIntoLines(input);


            return liturgy;
        }

        private static List<string> SplitIntoLines(string input)
        {
            List<string> lines = new List<string>();

            char[] all = input.ToCharArray();

            int index = 0;
            int sindex = 0;
            int eindex = 0;
            while (index < input.End())
            {
                // look for a <line> tag
                var match = Regex.Match(input.SubstringToEnd(index), @"$<line");

                if (match.Success) {
                    sindex = index;
                }


            }

            return lines;
        }

        private static void ParseIntoTags(string input)
        {

        }



    }
}
