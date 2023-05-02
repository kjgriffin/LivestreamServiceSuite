using System.Collections.Generic;
using System.IO;

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
            if (slide.Data.TryGetValue("key-file", out object keyfile))
            {
                if (!System.IO.File.Exists((string)keyfile))
                {
                    messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = $"Could not find file {res.AssetPath}", ErrorName = "Missing Video File", Level = Compiler.XenonCompilerMessageType.Error });
                    throw new FileNotFoundException();
                }
                else
                {
                    res.KeyAssetPath = (string)keyfile;
                    res.MediaType = SlideAssembly.MediaType.Video_KeyedVideo;
                }
            }

            res.KeyBitmap = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32>(1920, 1080, new SixLabors.ImageSharp.PixelFormats.Bgra32(255, 255, 255, 255));
            // for now videos are fully opaque all the time
            return res;
        }
    }
}
