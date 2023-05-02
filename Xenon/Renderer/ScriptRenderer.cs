using System.Collections.Generic;

using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class ScriptRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Text;
            res.AssetPath = "";
            res.RenderedAs = "Action";
            res.Text = (string)slide.Data["source"];
            return res;
        }





    }
}
