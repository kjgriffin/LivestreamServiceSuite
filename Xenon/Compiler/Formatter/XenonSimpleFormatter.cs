using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Xenon.Compiler.Formatter
{
    public static class XenonSimpleFormatter
    {

        enum XenonMode
        {
            Normal,

            SLComment,
            MLComment,

            SingleTickString,
            DoubleTickString,
            TrippleTickString,

            Command,

            Params,
            Body,

        }


        public static string Format(string input)
        {
            StringBuilder sb = new StringBuilder();

            const int spaces = 4;

            // track state
            int depth = 0;
            XenonMode mode = XenonMode.Normal;


            // 1. start inspecting the input string character by character
            //      - if we find something interesting modify state
            // otherwise, just spit it back out into the new string

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                char cnext = i + 1 < input.Length ? input[i + 1] : char.MaxValue;

                // ESCAPE COMMENTS
                if (mode == XenonMode.SLComment)
                {
                    // just dump it until a new-line
                    while (i < input.Length - 1 && (input[i].ToString() + input[i + 1].ToString()) != Environment.NewLine)
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    mode = XenonMode.Normal;
                    continue;
                }
                else if (mode == XenonMode.MLComment)
                {
                    // dump until */ ending found
                    while (i < input.Length - 1 && (input[i].ToString() + input[i + 1].ToString()) != "*/")
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    sb.Append("*/");
                    mode = XenonMode.Normal;
                    i += 1;
                    continue;
                }
                else if (mode == XenonMode.SingleTickString)
                {
                    // dump until ' ending found
                    while (i < input.Length && input[i] != '\'')
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    sb.Append("'");
                    mode = XenonMode.Normal;
                    continue;
                }
                else if (mode == XenonMode.DoubleTickString)
                {
                    // dump until " ending found
                    while (i < input.Length && input[i] != '"')
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    sb.Append("\"");
                    mode = XenonMode.Normal;
                    continue;
                }
                else if (mode == XenonMode.SingleTickString)
                {
                    // dump until ``` ending found
                    while (i < input.Length - 2 && input[i] != '`' && input[i + 1] != '`' && input[i + 2] != '`')
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    sb.Append("```");
                    mode = XenonMode.Normal;
                    i += 1;
                    continue;
                }


                // FIND COMMENTS
                if (c == '/')
                {
                    if (cnext == '/') // single line comment
                    {
                        mode = XenonMode.SLComment;
                        sb.Append("//");
                        i += 1;
                        continue;
                    }
                    else if (cnext == '*') // enter multiline comment block
                    {
                        mode = XenonMode.MLComment;
                        sb.Append("/*");
                        i += 1;
                        continue;
                    }
                }
                // FIND STRINGS
                else if (c == '"')
                {
                    mode = XenonMode.DoubleTickString;
                    sb.Append("\"");
                    continue;
                }
                else if (c == '\'')
                {
                    mode = XenonMode.SingleTickString;
                    sb.Append("'");
                    continue;
                }
                else if (c == '`' && cnext == '`' && i + 2 < input.Length && input[i + 2] == '`')
                {
                    mode = XenonMode.TrippleTickString;
                    sb.Append("```");
                    continue;
                }
                // FIND OPEN CONTENT


                // START COMMANDS ON NEW LINE
                else if (c == '#')
                {
                    sb.Append("".PadLeft(depth * spaces));
                    sb.Append(c);

                    // just dump it until a new-line
                    while (i < input.Length - 1 && Regex.Match(input[i].ToString(), @"\w").Success)
                    {
                        sb.Append(input[i]);
                        i++;
                    }
                    // gobble spaces
                    while (i < input.Length - 1 && Regex.Match(input[i].ToString(), @" ").Success)
                    {
                        i++;
                    }

                    mode = XenonMode.Command;
                }

                // PARAMS
                else if (c == '(')
                {
                    sb.Append(c);
                    mode = XenonMode.Params;
                }
                else if (c == ')')
                {
                    sb.Append(c);
                    mode = XenonMode.Command;
                }


                // INDENT BRACES
                else if (c == '{')
                {
                    // check if escaped

                    sb.AppendLine();
                    sb.Append("".PadLeft(depth * spaces));
                    sb.AppendLine(c.ToString());

                    depth++;
                    if (mode == XenonMode.Command)
                    {
                        mode = XenonMode.Body;
                    }
                }
                else if (c == '}')
                {
                    // check if escaped
                    depth--;

                    sb.AppendLine();
                    sb.Append("".PadLeft(depth * spaces));
                    sb.AppendLine(c.ToString());

                }

                // IGNORE WHITESPACE
                //else if (char.IsWhiteSpace(c))
                //{
                //    // do nothing...
                //}


                else
                {
                    // otherwise dump it out
                    sb.Append(c);
                }
            }




            return sb.ToString();

            static void ForceNewLineIfNeeded(StringBuilder sb, int depth, bool indent)
            {
                string s = sb.ToString();
                int x = s.LastIndexOf(Environment.NewLine);
                int r = x;
                bool newline = false;
                if (x > -1)
                {
                    for (int j = x + Environment.NewLine.Length; j < s.Length; j++)
                    {
                        if (!char.IsWhiteSpace(s[j]))
                        {
                            newline = true;
                            r = j;
                        }
                    }
                }
                if (newline)
                {
                    if (r > -1)
                    {
                        sb.Remove(r, s.Length - r);
                    }
                    sb.AppendLine();
                }
                if (indent)
                {
                    sb.Append("".PadLeft(depth * spaces));
                }

            }
        }


    }
}
