using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlideCreater.Renderer
{
    public class SlideRenderer
    {
        Project _project { get; set; }



        LiturgySlideRenderer lsr = new LiturgySlideRenderer();

        public SlideRenderer(Project proj)
        {
            _project = proj;
            lsr.Layouts = proj.Layouts;
        }

        public RenderedSlide RenderSlide(int slidenum)
        {
            if (slidenum >= _project.Slides.Count)
            {
                throw new ArgumentOutOfRangeException("slidenum");
                //return RenderedSlide.Default();
            }
            return RenderSlide(_project.Slides[slidenum]);
        }

        public RenderedSlide RenderSlide(Slide slide)
        {
            // use an appropriate slide render for the task
            switch (slide.Format)
            {
                case "LITURGY":
                    return lsr.RenderSlide(_project.Layouts.LiturgyLayout.GetRenderInfo(), slide);
                default:
                    return RenderedSlide.Default();
            }
        }


    }
}
