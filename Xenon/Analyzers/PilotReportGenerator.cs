using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Compiler.AST;
using Xenon.SlideAssembly;

namespace Xenon.Analyzers
{
    public class PilotReportGenerator
    {

        public static string GeneratePilotPresetReport(Project proj)
        {
            StringBuilder sb = new StringBuilder();

            Dictionary<string, HashSet<string>> usedPresets = new Dictionary<string, HashSet<string>>();

            foreach (var slide in proj?.Slides)
            {
                if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
                {
                    foreach (var line in ((string)pilot).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var match = Regex.Match(line, @"\[(?<camname>.*)\]\((?<pstname>.*)\)");
                        var cname = match.Groups["camname"].Value;
                        var pname = match.Groups["pstname"].Value;
                        if (usedPresets.TryGetValue(cname, out var presets))
                        {
                            presets.Add(pname);
                        }
                        else
                        {
                            usedPresets[cname] = new HashSet<string> { pname };
                        }
                    }
                }
            }

            return string.Join(Environment.NewLine, usedPresets.Select(kvp => $"{kvp.Key}\n\t{string.Join("\n\t", kvp.Value.ToList())}"));

        }

    }
}
