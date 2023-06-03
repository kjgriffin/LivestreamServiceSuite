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


        internal List<Slide> ApplyNesscarySubstitutions()
        {
            foreach (var s in _slides)
            {
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
