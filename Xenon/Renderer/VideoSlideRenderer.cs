using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xenon.Renderer
{
    class VideoSlideRenderer
    {
        public RenderedSlide RenderSlide(SlideAssembly.Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            string rendertype = "Video";
            // check if path exists
            if (slide.Data.ContainsKey("key-type"))
            {
                if ((string)slide.Data["key-type"] == "chroma")
                {
                    rendertype = "ChromaKeyVideo";
                }
            }
            RenderedSlide res = new RenderedSlide()
            {
                AssetPath = slide.Lines[0].Content[0].Data,
                MediaType = SlideAssembly.MediaType.Video,
                RenderedAs = rendertype
            };
            if (!System.IO.File.Exists(res.AssetPath))
            {
                messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = $"Could not find file {res.AssetPath}", ErrorName = "Missing Video File", Level = Compiler.XenonCompilerMessageType.Error });
                throw new FileNotFoundException();
            }
            return res;
        }
    }
}
