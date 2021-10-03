using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;
using System.IO;

namespace Xenon.Renderer
{
    class CopySlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            // only do audio for now
            res.MediaType = MediaType.Audio;
            res.RenderedAs = "Resource";
            if (slide.Data.TryGetValue("resource.name", out object name))
            {
                res.Name = (string)name;
            }
            else
            {
                res.Name = Path.GetFileNameWithoutExtension(slide.Asset);
                messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = $"Resource name was not specified. Will use original file name {res.Name}, but this may not produce the expected result.", ErrorName = "Resource Name Mismatch", Generator = "CopySlideRenderer", Inner = "", Level = Compiler.XenonCompilerMessageType.Warning, Token = "" });
            }
            res.CopyExtension = Path.GetExtension(slide.Asset);
            res.AssetPath = slide.Asset;
            return res;
        }





    }
}
