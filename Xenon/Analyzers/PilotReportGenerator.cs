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

            Dictionary<string, HashSet<string>> usedPosPresets = new Dictionary<string, HashSet<string>>();
            Dictionary<string, HashSet<string>> usedZPresets = new Dictionary<string, HashSet<string>>();

            foreach (var slide in proj?.Slides)
            {
                if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
                {
                    foreach (var line in ((string)pilot).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var match = Regex.Match(line, @"\[(?<camname>.*)\]\((?<pstname>.*)\)@\d+((\|.*)|(<(?<zpst>.*))>)");
                        var cname = match.Groups["camname"].Value;
                        var pname = match.Groups["pstname"].Value;
                        var zname = match.Groups["zpst"].Value;
                        if (usedPosPresets.TryGetValue(cname, out var presets))
                        {
                            presets.Add(pname);
                        }
                        else
                        {
                            usedPosPresets[cname] = new HashSet<string> { pname };
                        }
                        if (!string.IsNullOrEmpty(zname))
                        {
                            if (usedZPresets.TryGetValue(cname, out var zpresets))
                            {
                                zpresets.Add(zname);
                            }
                            else
                            {
                                usedZPresets[cname] = new HashSet<string> { zname };
                            }
                        }
                    }
                }
            }

            return "POS: { " + string.Join(Environment.NewLine, usedPosPresets.Select(kvp => $"{kvp.Key}\n\t{string.Join("\n\t", kvp.Value.ToList())}")) + "}" + Environment.NewLine + "ZOOM: { " + string.Join(Environment.NewLine, usedZPresets.Select(kvp => $"{kvp.Key}\n\t{string.Join("\n\t", kvp.Value.ToList())}")) + "}";

        }

    }
}
