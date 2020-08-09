using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SlideCreater.Renderer
{
    public class SlideRenderer
    {
        Project _project { get; set; }



        LiturgySlideRenderer lsr = new LiturgySlideRenderer();
        VideoSlideRenderer vsr = new VideoSlideRenderer();
        ImageSlideRenderer isr = new ImageSlideRenderer();
        ReadingSlideRenderer rsr = new ReadingSlideRenderer();
        SermonTitleSlideRenderer ssr = new SermonTitleSlideRenderer();
        HymnTextVerseRenderer hvsr = new HymnTextVerseRenderer();

        public SlideRenderer(Project proj)
        {
            _project = proj;
            lsr.Layouts = proj.Layouts;
            isr.Layout = proj.Layouts;
            rsr.Layouts = proj.Layouts;
            ssr.Layouts = proj.Layouts;
            hvsr.Layouts = proj.Layouts;
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
                case SlideFormat.Liturgy:
                    return lsr.RenderSlide(_project.Layouts.LiturgyLayout.GetRenderInfo(), slide);
                case SlideFormat.Video:
                    return vsr.RenderSlide(slide);
                case SlideFormat.UnscaledImage:
                    return isr.RenderImageSlide(slide);
                case SlideFormat.ScaledImage:
                    return isr.RenderImageSlide(slide);
                case SlideFormat.LiturgyImage:
                    return isr.RenderImageSlide(slide);
                case SlideFormat.Reading:
                    return rsr.RenderSlide(_project.Layouts.ReadingLayout.GetRenderInfo(), slide);
                case SlideFormat.SermonTitle:
                    return ssr.RenderSlide(_project.Layouts.SermonLayout.GetRenderInfo(), slide);
                case SlideFormat.HymnTextVerse:
                    return hvsr.RenderSlide(_project.Layouts.TextHymnLayout.GetRenderInfo(), slide);
                default:
                    return RenderedSlide.Default();
            }
        }


    }
}
