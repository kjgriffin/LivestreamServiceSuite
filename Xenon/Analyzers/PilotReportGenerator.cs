using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.Compiler.AST;
using Xenon.SlideAssembly;

namespace Xenon.Analyzers
{
    public class PilotReportGenerator
    {

        internal static void YellAboutUnkownPilotCommands(Project proj, XenonErrorLogger logger)
        {

            var x = proj?.Slides.Select(x => (x.NonRenderedMetadata[XenonASTExpression.DATAKEY_PILOT_SOURCECODE_LOOKUP])).ToList();

            foreach (var slide in proj?.Slides)
            {
                if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
                {
                    foreach (var line in ((string)pilot).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var match = Regex.Match(line, @"^(run|emgrun)\[(?<camname>.*)\]\((?<pstname>.*)\)@(?<speed>\d+)((\|.*)|(:(?<zpst>.*)));");
                        var cname = match.Groups["camname"].Value;
                        var pname = match.Groups["pstname"].Value;
                        var zname = match.Groups["zpst"].Value;
                        var speed = match.Groups["speed"].Value;
                        int.TryParse(speed, out int ptspeed);

                        slide.NonRenderedMetadata.TryGetValue(XenonASTExpression.DATAKEY_PILOT_SOURCECODE_LOOKUP, out var slref);
                        int sourcelineref = (int)slref;

                        // if no-match warn about possibly invalid command
                        if (!match.Success)
                        {
                            logger.Log(new XenonCompilerMessage
                            {
                                ErrorName = $"Pilot Command Invalid slide[{slide.Number}]",
                                ErrorMessage = $"Pilot Command `{line}` does not match the expected format for a 'run' or 'emgrun' type command.",
                                Generator = "PilotReportGenerator",
                                Inner = "",
                                Level = XenonCompilerMessageType.Warning, // technically it could be an older format, so just warn
                                Token = new Token("", sourcelineref),
                            });
                            continue;
                        }

                        // warn if speed is invalid 
                        if (ptspeed < 0 || ptspeed > 0x18)
                        {
                            logger.Log(new XenonCompilerMessage
                            {
                                ErrorName = $"Pilot Command Invalid Drive Speed ({ptspeed}) [{slide.Number}]",
                                ErrorMessage = $"Drive speeds must be in the range[0, 24]",
                                Generator = "PilotReportGenerator",
                                Inner = "",
                                Level = XenonCompilerMessageType.Error,
                                Token = new Token("", sourcelineref),
                            });
                        }

                        if (!proj.CCPUConfig.Clients.Any(x => x.Name == cname))
                        {
                            logger.Log(new XenonCompilerMessage
                            {
                                ErrorName = $"Pilot Command for camera ({cname}) has no client target configured [{slide.Number}]",
                                ErrorMessage = $"The CCU Config doesn't contain a client with the name '{cname}'",
                                Generator = "PilotReportGenerator",
                                Inner = "",
                                Level = XenonCompilerMessageType.Error,
                                Token = new Token("", sourcelineref),
                            });
                        }
                        else
                        {
                            if (!proj.CCPUConfig.KeyedPresets.TryGetValue(cname, out var presets) || !presets.ContainsKey(pname))
                            {
                                logger.Log(new XenonCompilerMessage
                                {
                                    ErrorName = $"Pilot Command for camera ({cname}) to preset ({pname}) doesn't exist [{slide.Number}]",
                                    ErrorMessage = $"The CCU Config doesn't contain a preset with the name ({pname}) for ({cname})",
                                    Generator = "PilotReportGenerator",
                                    Inner = "",
                                    Level = XenonCompilerMessageType.Error,
                                    Token = new Token("", sourcelineref),
                                });
                            }
                            if (!proj.CCPUConfig.KeyedZooms.TryGetValue(cname, out var zpresets) || !zpresets.ContainsKey(zname))
                            {
                                logger.Log(new XenonCompilerMessage
                                {
                                    ErrorName = $"Pilot Command for camera ({cname}) to zoom preset ({zname}) doesn't exist [{slide.Number}]",
                                    ErrorMessage = $"The CCU Config doesn't contain a zoom preset with the name ({zname}) for ({cname})",
                                    Generator = "PilotReportGenerator",
                                    Inner = "",
                                    Level = XenonCompilerMessageType.Error,
                                    Token = new Token("", sourcelineref),
                                });
                            }

                        }

                    }
                }
            }

        }


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
                        var match = Regex.Match(line, @"\[(?<camname>.*)\]\((?<pstname>.*)\)@\d+((\|.*)|((?<zpst>.*)))");
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
