using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xenon.Compiler.Formatter
{
    internal class XenonExpressionFormatter
    {
        public int IndentDepth { get; set; }
        public int IndentSpaces { get; set; }

        public string FastFormatProgram(string input)
        {
            ReadOnlySpan<char> text = input.AsSpan();

            // API spec
            // we're parsing a PROGRAM AST element

            // we're expecting EXPRESSIONS

            return input;
        }

        string FormatUntilExpressionStart(ReadOnlySpan<char> input, ref int depthLevel, out int index)
        {
            StringBuilder sb = new StringBuilder();
            // allow any new-lines
            // lines with whitespace are trimmed

            // comments are aligned at depthLevel

            // pre-proc are always placed at 0 depth

            // decorators are aligned at depthLevel (calls into ExpressionProper)
            // commands are aligned at depthLevel (calls into ExpressionProper)

            index = 0;
            ReadOnlySpan<char> text = input;
            while (index < text.Length)
            {
                // search for closest item we handle
                /*
                    /r/n
                    //
                    ///
                    /*
                    [
                    #
                */

                // remove whitespace
                // allow junk characters
                // search for hint we're entering block of interest

            }

            return input.ToString();
        }

    }
}
