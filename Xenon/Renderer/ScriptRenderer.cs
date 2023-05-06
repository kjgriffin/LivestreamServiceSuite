using System.Collections.Generic;
using System.Collections.Specialized;

using Xenon.Compiler;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class ScriptRenderer : ISlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, ISlideRendertimeInfoProvider info)
        {
            // only at render time can we solve some variables
            // for some commands we now can jump to slides
            // by now we can finally resolve the number
            string src = (string)slide.Data["source"];
            src = CommonTextContentSlideVariableReplacer.ReplaceVariablesInText(src, info);

            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Text;
            res.AssetPath = "";
            res.RenderedAs = "Action";
            res.Text = src;
            return res;
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.Script)
            {
                result = RenderSlide(slide, Messages, info);
            }
        }
    }
}
