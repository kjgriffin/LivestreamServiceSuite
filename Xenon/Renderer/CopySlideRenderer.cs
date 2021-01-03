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
            res.RenderedAs = "Liturgy";
            res.Name = Path.GetFileNameWithoutExtension(slide.Asset);
            res.CopyExtension = Path.GetExtension(slide.Asset);
            return res;
        }





    }
}
