using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Compiler.SubParsers
{
    internal class TextBlockParser
    {


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
                Lexer.Consume();

                if (trimmode == "trim")
                {
                    result = sb.ToString().Trim();
                }
                else if (trimmode.Contains("trimlines"))
                {
                    var lines = sb.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                    result = string.Join(Environment.NewLine, lines);
                }
                else if (trimmode.Contains("trimat:`"))
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
                else
                {
                    result = sb.ToString();
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
            Lexer.Consume();
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
