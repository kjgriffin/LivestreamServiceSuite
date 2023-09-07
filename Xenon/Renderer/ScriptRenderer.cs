using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class ScriptRenderer : ISlideRenderer
    {
        public static string DATAKEY_SCRIPTSOURCE_TARGET { get => "source"; }

        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, ISlideRendertimeInfoProvider info)
        {
            // only at render time can we solve some variables
            // for some commands we now can jump to slides
            // by now we can finally resolve the number
            string src = (string)slide.Data[DATAKEY_SCRIPTSOURCE_TARGET];
            //src = CommonTextContentSlideVariableReplacer.ReplaceVariablesInText(src, info);

            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Text;
            res.AssetPath = "";
            res.RenderedAs = "Action";
            res.Text = src;
            return res;
        }

        public Task<RenderedSlide> VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide operand)
        {
            if (slide.Format == SlideFormat.Script)
            {
                var render = RenderSlide(slide, Messages, info);
                return Task.FromResult(render);
            }
            return Task.FromResult(operand);
        }
    }
}
