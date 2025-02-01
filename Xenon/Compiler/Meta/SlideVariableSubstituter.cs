using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Compiler.AST;
using Xenon.Renderer;
using Xenon.SlideAssembly;

namespace Xenon.Compiler.Meta
{
    internal class SlideVariableSubstituter : ISlideRendertimeInfoProvider
    {
        public int FindSlideNumber(string reference)
        {
            // parse reference
            var match = Regex.Match(reference, @"%slide\.num\.(?<label>.*)\.(?<num>\d+)%");
            if (match.Success)
            {
                int num = int.Parse(match.Groups["num"].Value);
                string label = match.Groups["label"].Value;

                // peek into the project
                // find all slides exposing a slide label
                // dump 'er in
                List<Slide> candidates = new List<Slide>();

                foreach (var slide in _slides)
                {
                    if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_CMD_SOURCESLIDENUM_LABELS, out var d))
                    {
                        var data = d as List<string>;
                        if (data != null && data.Contains(label))
                        {
                            candidates.Add(slide);
                        }
                    }
                }

                // try find slide
                var orderedSlides = candidates.OrderBy(x => x.Number).ToList();
                if (num < 0)
                {
                    orderedSlides.Reverse();
                    num -= 1;
                }
                if (num < orderedSlides.Count)
                {
                    return orderedSlides[num].Number;
                }

            }

            return 0;
        }

        List<Slide> _slides { get; }
        BMDSwitcherConfigSettings _bmdConfig { get; }

        public SlideVariableSubstituter(List<Slide> slides, IntegratedPresenter.BMDSwitcher.Config.BMDSwitcherConfigSettings bMDSwitcherConfig)
        {
            _slides = slides;
            _bmdConfig = bMDSwitcherConfig;
        }


        internal class UnresolvedText
        {
            public static string DATAKEY_UNRESOLVEDTEXT { get => "unresolved-text"; }
            public string Raw { get; set; }
            public string DKEY { get; set; }
        }

        internal class UnresolvedScript
        {
            public static string DATAKEY_NAMEDSCRIPT_ID { get => "named-script-id"; }
            public static string DATAKEY_NAMEDSCRIPT_SOURCE { get => "named-script-source"; }
            public static string DATAKEY_UNRESOLVEDSCRIPT { get => "unresolved-script"; }
            public static string DATAKEY_DEFAULT_ARGUMENTS { get => "default-arguments"; }
            public string InvokedScriptID { get; set; }
            public Dictionary<string, string> Arguments { get; set; }
            public string DKEY { get; set; }
        }


        internal List<Slide> ApplyNesscarySubstitutions(XenonErrorLogger masterlog)
        {
            foreach (var s in _slides)
            {
                // we have all the slides

                // 1. resolve #callscript
                //  - if the slide invokes a named script, find the named script slide that was generated
                //  - pull the data from the namedscript
                //  - at this point replace %param% with the invoked arguments
                //  - deposit all this into the unresolved text, and handle as usual with the next process
                if (s.Data.TryGetValue(UnresolvedScript.DATAKEY_UNRESOLVEDSCRIPT, out var objScript))
                {
                    UnresolvedScript unresolvedScript = (UnresolvedScript)objScript;

                    // find called script
                    var first = _slides.FirstOrDefault(sld => sld.NonRenderedMetadata.TryGetValue(UnresolvedScript.DATAKEY_NAMEDSCRIPT_ID, out var oid) && (string)oid == unresolvedScript.InvokedScriptID);

                    if (first == null)
                    {
                        masterlog.Log(new XenonCompilerMessage
                        {
                            ErrorName = "Failed to resolved called script",
                            ErrorMessage = $"Scripted block attempted to call script with name {unresolvedScript.InvokedScriptID} but no #namescript exists",
                            Generator = "SlideVariableSubstituter",
                            Level = XenonCompilerMessageType.Error,
                            // pity we can't reconstruct more info about this??
                            SrcFile = s.NonRenderedMetadata[XenonASTExpression.DATAKEY_CMD_SOURCEFILE_LOOKUP] as string,
                        });
                        // bail out and hope the log is enough??
                        continue;
                    }

                    string source = string.Empty;
                    Dictionary<string, string> defaultArgs = new Dictionary<string, string>();
                    if (first.NonRenderedMetadata.TryGetValue(UnresolvedScript.DATAKEY_DEFAULT_ARGUMENTS, out var dargs))
                    {
                        defaultArgs = dargs as Dictionary<string, string> ?? new Dictionary<string, string>();
                    }
                    if (first.NonRenderedMetadata.TryGetValue(UnresolvedScript.DATAKEY_NAMEDSCRIPT_SOURCE, out var src))
                    {
                        source = src as string;
                        // resolve parameters by injecting arguments

                        var matches = Regex.Matches(source, @"%param\.(?<paramname>.*?)%");
                        foreach (Match match in matches)
                        {
                            var pkey = match.Groups["paramname"].Value;
                            if (unresolvedScript.Arguments.TryGetValue(pkey, out var argValue))
                            {
                                source = source.Replace(match.Value, argValue);
                            }
                            else if (defaultArgs.TryGetValue(pkey, out argValue))
                            {
                                source = source.Replace(match.Value, argValue);
                            }
                            else
                            {
                                masterlog.Log(new XenonCompilerMessage
                                {
                                    ErrorName = "Failed to resolve parameter",
                                    ErrorMessage = $"Namedscript expects parameter <{pkey}> but no such parameter was provided",
                                    Generator = "SlideVariableSubstituter",
                                    Level = XenonCompilerMessageType.Error,
                                    // pity we can't reconstruct more info about this??
                                    SrcFile = s.NonRenderedMetadata[XenonASTExpression.DATAKEY_CMD_SOURCEFILE_LOOKUP] as string,
                                });
                            }
                        }
                    }

                    XenonASTAsScripted.InjectScriptData(s, source);
                }


                if (s.Data.TryGetValue(UnresolvedText.DATAKEY_UNRESOLVEDTEXT, out var raw))
                {
                    // I think we leave it alone...
                    UnresolvedText obj = raw as UnresolvedText;
                    if (obj != null)
                    {
                        var replaced = CommonTextContentSlideVariableReplacer.ReplaceVariablesInText(obj.Raw, this);
                        s.Data[obj.DKEY] = replaced;
                    }
                }
            }
            return _slides;
        }

        public int FindCameraID(string camName)
        {
            return _bmdConfig.Routing.FirstOrDefault(x => x.LongName.ToLower() == camName)?.PhysicalInputId ?? 0;
        }
    }
}
