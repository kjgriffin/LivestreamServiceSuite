using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Xenon.Compiler
{
    class XMLErrorGenerator
    {
        public void AddXMLNotes(string source, XenonErrorLogger logger)
        {

            IEnumerable<string> lines = source.Split(new string[] { Environment.NewLine, "\r\n", "\n" }, StringSplitOptions.None);

            int lnum = 0;
            foreach (var line in lines)
            {
                // TODO: make this more robust as we expand its use
                var m = Regex.Match(line, @"/// </MANUAL_UPDATE name=\'(?<name>.*)\'>");
                if (m.Success)
                {
                    logger.Log(new XenonCompilerMessage() { ErrorName = $"Manual Update Required for {m.Groups["name"].Value}", ErrorMessage = $"Found command marked for manual update of {m.Groups["name"].Value}.", Generator = "XMLErrorGenerator", Inner = "", Level = XenonCompilerMessageType.ManualIntervention, Token = (m.Groups["name"].Value, lnum)});
                }
                lnum += 1;
            }

        }
    }
}
