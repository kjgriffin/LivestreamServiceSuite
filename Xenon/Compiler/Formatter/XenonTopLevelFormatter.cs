using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Compiler.Formatter
{
    public static class XenonTopLevelFormatter
    {


        public static string FormatTopLevel(string input)
        {
            StringBuilder output = new StringBuilder();

            StringBuilder capture = new StringBuilder();

            // only handle top-level problems
            // comments, block comments and top-level commands
            // (once we're inside one of these pass them off to a dsl formatter

            int indentSpaces = 4;
            int indentDepth = 0;

            int ci = 0;

            ReadOnlySpan<char> text = input.AsSpan();

            while (ci < text.Length)
            {
                capture.Clear();

                // allow new-lines
                if (text.IsMatch(ci, Environment.NewLine))
                {
                    output.AppendLine();
                    ci += Environment.NewLine.Length;
                }
                // search for xml comment
                else if (text.IsMatch(ci, "///"))
                {
                    ci += 3;
                    capture.Append("///");
                    while (!text.IsMatch(ci, Environment.NewLine) && ci < text.Length)
                    {
                        capture.Append(text[ci]);
                        ci++;
                    }
                    // xml comments start on a new-line
                    if (!output.EndsWith(Environment.NewLine))
                    {
                        output.Append(Environment.NewLine);
                    }
                    // xml comments are indented
                    output.Append(capture.ToString().Indent(indentDepth, indentSpaces));
                    // trailing new-line handled above
                }
                // search for block comment
                else if (text.IsMatch(ci, "/*"))
                {
                    // ignore everything (and don't bother formatting until we get to the end
                    ci += 2;
                    capture.Append("/*");
                    while (!text.IsMatch(ci, "*/") && ci < text.Length)
                    {
                        capture.Append(text[ci]);
                        ci++;
                    }
                    // block comments start on new-lines
                    // xml comments start on a new-line
                    if (!output.EndsWith(Environment.NewLine))
                    {
                        output.Append(Environment.NewLine);
                    }
                    output.Append(capture.ToString());

                    // trailing new-line handled above
                }
                // search for single line comment
                else if (text.IsMatch(ci, "//"))
                {
                    // match indentation?
                    ci += 2;
                    capture.Append("//");
                    while (!text.IsMatch(ci, Environment.NewLine) && ci < text.Length)
                    {
                        capture.Append(text[ci]);
                        ci++;
                    }
                    // comments are indented if they start on a new-line
                    output.Append(capture.ToString().Indent(indentDepth, indentSpaces));
                    // trailing new-line handled above

                }
                // search for command
                else if (text.IsMatch(ci, "#"))
                {
                    string cmd = "";

                    // grab the command word
                    while (!text.IsMatch(ci, " ", "(") && ci < text.Length)
                    {
                        capture.Append(text[ci]);
                    }
                    cmd = capture.ToString();
                    capture.Clear();

                    // parse any argument lists as required

                    if (text.IsMatch(ci, "("))
                    {

                    }
                    if (text.IsMatch(ci, "["))
                    {

                    }
                    if (text.IsMatch(ci, "<"))
                    {

                    }

                    // parse any block openings as required


                    // postset

                    // pilot


                }

                //else
                //{
                //output.Append(text[ci]);
                //ci++;
                //}

            }

            return output.ToString();
        }

        private static bool IsMatch(this ReadOnlySpan<char> input, int startIndex, string pattern)
        {
            if (pattern.Length + startIndex < input.Length)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (input[startIndex + i] != pattern[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool IsMatch(this ReadOnlySpan<char> input, int startIndex, params string[] patterns)
        {
            var ordered = patterns.OrderByDescending(x => x);

            foreach (var pattern in ordered)
            {
                if (pattern.Length + startIndex < input.Length)
                {
                    bool ok = true;
                    for (int i = 0; i < pattern.Length; i++)
                    {
                        if (input[startIndex + i] != pattern[i])
                        {
                            ok = false;
                        }
                    }
                    if (ok)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private static bool EndsWith(this StringBuilder sb, string pattern)
        {
            if (pattern.Length < sb.Length)
            {
                return sb.ToString(sb.Length - pattern.Length, pattern.Length) == pattern;
            }
            return false;
        }
        private static string Indent(this string str, int indentdepth, int indentspace)
        {
            return str.PadLeft(indentdepth * indentspace + str.Length);
        }




    }
}
