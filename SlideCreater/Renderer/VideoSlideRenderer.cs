using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlideCreater.Renderer
{
    public class VideoSlideRenderer
    {
        public RenderedSlide RenderSlide(SlideAssembly.Slide slide)
        {
            RenderedSlide res = new RenderedSlide()
            {
                AssetPath = slide.Lines[0].Content[0].Data,
                MediaType = SlideAssembly.MediaType.Video,
                RenderedAs = "Video"
            };
            return res;
        }
    }
}
