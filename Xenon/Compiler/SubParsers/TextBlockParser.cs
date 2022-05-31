using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xenon.Compiler.SubParsers
{
    internal class TextBlockParser
    {


        public static (List<string> modes, List<string> lines) ReformatTextBlock(string input)
        {
            List<string> modes = new List<string>();
            // we assume the input string is wanted...
            if (char.IsWhiteSpace(input.First()))
            {
                modes.Add("notrim");
            }
            else
            {
                modes.Add("pretrim");
            }

            string escapedinput = Regex.Replace(input, "//", @"\//");

            return (modes, escapedinput.Split(Environment.NewLine).Where(x => !string.IsNullOrWhiteSpace(x)).ToList());
        }

        public static bool TryParseTextBlock(Lexer Lexer, out string result)
        {
            result = String.Empty;
            if (Lexer.Inspect("text"))
            {
                Lexer.Consume();

                string trimmode = "";
                if (Lexer.Inspect("("))
                {
                    Lexer.Consume();
                    trimmode = Lexer.ConsumeUntil(")");
                    Lexer.Consume();
                }

                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("=");
                Lexer.GobbleWhitespace();
                Lexer.GobbleandLog("{");

                if (trimmode.Contains("pretrim") && !trimmode.Contains("notrim"))
                {
                    Lexer.GobbleWhitespace();
                }

                StringBuilder sb = new StringBuilder();
                while (!Lexer.Inspect("}"))
                {
                    sb.Append(Lexer.Consume());
                    // allow string escaping
                    if (Lexer.Peek() == "}" && Lexer.PeekNext() == "}")
                    {
                        Lexer.Consume();
                        sb.Append(Lexer.Consume());
                    }
                }
                Lexer.Consume();

                if (trimmode.Contains("notrim"))
                {
                    result = sb.ToString();
                }
                else
                {
                    result = sb.ToString().Trim();

                    if (trimmode.Contains("trimlines"))
                    {
                        var lines = sb.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                        result = string.Join(Environment.NewLine, lines);
                    }
                    if (trimmode.Contains("trimat:`"))
                    {
                        var lines = sb.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                        var tmp = new List<string>();
                        foreach (var line in lines)
                        {
                            // remove all leading whitespace up to first instance of `. Won't allow double escaping. If you really need that you'll have to manually handle the trimming
                            int index = line.IndexOf('`');
                            tmp.Add(line.Remove(0, index != -1 ? Math.Min(index + 1, line.Length) : 0));
                        }
                        result = string.Join(Environment.NewLine, tmp);
                    }
                }

                return true;
            }
            return false;
        }

        public static List<string> ParseTextBlockLines(Lexer Lexer)
        {
            Lexer.GobbleWhitespace();
            string trimmode = "";
            if (Lexer.Inspect("("))
            {
                Lexer.Consume();
                trimmode = Lexer.ConsumeUntil(")");
                Lexer.Consume();
                Lexer.GobbleWhitespace();
            }

            Lexer.GobbleandLog("{");

            if (trimmode.Contains("pretrim"))
            {
                Lexer.GobbleWhitespace();
            }

            StringBuilder sb = new StringBuilder();
            while (!Lexer.Inspect("}"))
            {
                sb.Append(Lexer.Consume());
                // allow string escaping
                if (Lexer.Peek() == "}" && Lexer.PeekNext() == "}")
                {
                    Lexer.Consume();
                    sb.Append(Lexer.Consume());
                }
            }
            //Lexer.Consume();
            Lexer.GobbleWhitespace();
            Lexer.GobbleandLog("}");

            var lines = sb.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            if (trimmode.Contains("trimat:`"))
            {
                var tmp = new List<string>();
                foreach (var line in lines)
                {
                    // remove all leading whitespace up to first instance of `. Won't allow double escaping. If you really need that you'll have to manually handle the trimming
                    int index = line.IndexOf('`');
                    tmp.Add(line.Remove(0, index != -1 ? Math.Min(index + 1, line.Length) : 0));
                }
                return tmp;
            }


            return lines.Select(x => trimmode.Contains("trimlines") ? x.Trim() : x).ToList();
        }


    }
}
