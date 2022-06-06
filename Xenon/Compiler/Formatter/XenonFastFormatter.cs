using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Compiler.AST;

namespace Xenon.Compiler.Formatter
{
    public class XenonFastFormatter
    {

        public static Task<string> CompileReformat(string source, int indent)
        {
            XenonErrorLogger _logger = new XenonErrorLogger();
            Lexer _lexer = new Lexer(_logger);
            XenonASTProgram p = new XenonASTProgram();
            return Task.Run(() =>
            {
                try
                {
                    // TODO: need to be able to recover comments....
                    var preproc = _lexer.StripComments(source);
                    _lexer.Tokenize(preproc);

                    p = (XenonASTProgram)p.Compile(_lexer, _logger, null);

                    StringBuilder sb = new StringBuilder();
                    int indentDepth = 0;
                    p.DecompileFormatted(sb, ref indentDepth, indent);
                    return sb.ToString();
                }
                catch (Exception)
                {
                    return source;
                }
            });
        }


        public static string Reformat(string source, int indent)
        {
            StringBuilder sb = new StringBuilder();

            // we want open/closing braces on seperate lines
            // scope/some commands are the only valid indentation locations

            int ilevel = 0;

            // break it up somehow...

            var lines = source.Split(Environment.NewLine).ToList();

            // unless we detect we're in a context where it matters
            bool preserveWhitespace = false;
            bool cmdNext = false;
            bool expectCmd = true;
            foreach (var _line in lines)
            {
                string line = preserveWhitespace ? _line : _line.Trim();

                var lchunks = Regex.Split(line, "([#{}])");
                // go through the chunks
                // if we detect something worthy of indentation/formatting we need to act

                bool NLreq = true;

                bool atStart = true;
                for (int i = 0; i < lchunks.Length; i++)
                {
                    var lchunk = i - 1 > 0 ? lchunks[i - 1] : "";
                    var chunk = lchunks[i];
                    var nextchunk = i + 1 < lchunks.Length ? lchunks[i + 1] : "";


                    // we want to kick {/} to their own line if they belong to a command
                    // otherwise we won't touch them

                    // when kicking we want to increase indentation

                    // when we find we're in a 'text()={}' segment we need to be whitespace sensitive


                    if (atStart)
                    {
                        // TODO: indent...
                        sb.Append("".PadLeft(Math.Max(ilevel * indent, 0)));
                        atStart = false;
                    }

                    if (string.IsNullOrEmpty(chunk))
                    {
                        continue;
                    }

                    if (chunk == "#" && expectCmd)
                    {
                        cmdNext = true;
                    }

                    if (chunk == "text")
                    {
                        expectCmd = false;
                    }

                    if (chunk == "}" && !expectCmd)
                    {
                        expectCmd = true;
                    }
                    else if (chunk == "}")
                    {
                        sb.AppendLine();
                        ilevel--;
                        sb.AppendLine(chunk.PadLeft(Math.Max(ilevel * indent + chunk.Length, 0))); // TODO: indent the line correctly
                        NLreq = false;
                        atStart = true;
                        continue;
                    }

                    if (chunk == "{" && cmdNext)
                    {
                        cmdNext = false;
                        sb.AppendLine();
                        sb.AppendLine(chunk.PadLeft(Math.Max(ilevel * indent + chunk.Length, 0))); // TODO: indent the line correctly
                        ilevel++;
                        NLreq = false;
                        atStart = true;
                        continue;
                    }
                    sb.Append(chunk);
                }
                if (NLreq)
                {
                    sb.AppendLine();
                }

            }


            return sb.ToString().TrimEnd();
        }

    }
}
