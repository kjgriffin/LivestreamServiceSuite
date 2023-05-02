using System;
using System.Collections.Generic;
using System.Linq;

namespace Xenon.Compiler.SubParsers
{
    internal class PilotParser
    {
        internal static string SanatizePilotCommands(string input)
        {
            input = input.Trim();

            List<string> commands = new List<string>();
            // split by either newline or by ;
            var semicolonblocks = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in semicolonblocks)
            {
                var lines = block.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                commands.AddRange(lines);
            }

            return string.Join(Environment.NewLine, commands.Select(cmd => $"{cmd};".Trim()));
        }


    }
}
