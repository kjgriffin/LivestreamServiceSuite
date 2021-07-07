using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Compiler;
using Xenon.Renderer.ImageFilters;

namespace Xenon.Renderer
{
    class SlideRenderer
    {
        Project _project { get; set; }



        LiturgySlideRenderer lsr = new LiturgySlideRenderer();
        VideoSlideRenderer vsr = new VideoSlideRenderer();
        ImageSlideRenderer isr = new ImageSlideRenderer();
        ReadingSlideRenderer rsr = new ReadingSlideRenderer();
        SermonTitleSlideRenderer ssr = new SermonTitleSlideRenderer();
        HymnTextVerseRenderer hvsr = new HymnTextVerseRenderer();
        PrefabSlideRenderer psr = new PrefabSlideRenderer();
        LiturgyVerseSlideRenderer lvsr = new LiturgyVerseSlideRenderer();
        AnthemTitleSlideRenderer atsr = new AnthemTitleSlideRenderer();
        TwoPartTitleSlideRenderer tpsr = new TwoPartTitleSlideRenderer();
        TitledLiturgyVerseSlideRenderer tlvsr = new TitledLiturgyVerseSlideRenderer();
        CopySlideRenderer csr = new CopySlideRenderer();
        ScriptRenderer sr = new ScriptRenderer();
        ImageFilterRenderer ifr = new ImageFilterRenderer();
        StitchedImageRenderer sir = new StitchedImageRenderer();

        public SlideRenderer(Project proj)
        {
            _project = proj;
            lsr.Layouts = proj.Layouts;
            isr.Layout = proj.Layouts;
            rsr.Layouts = proj.Layouts;
            ssr.Layouts = proj.Layouts;
            hvsr.Layouts = proj.Layouts;
            psr.Layouts = proj.Layouts;
            lvsr.Layouts = proj.Layouts;
            atsr.Layouts = proj.Layouts;
            tpsr.Layouts = proj.Layouts;
            tlvsr.Layouts = proj.Layouts;
        }

        public RenderedSlide RenderSlide(Slide s, List<XenonCompilerMessage> Messages)
        {
            var res = _RenderSlide(s, Messages);
            res.Number = s.Number;
            
            // attach Postset
            if (s.Data.ContainsKey("postset"))
            {
                res.IsPostset = true;
                res.Postset = (int)s.Data["postset"];
            }

            return res;
        }

        private RenderedSlide _RenderSlide(Slide slide, List<XenonCompilerMessage> Messages)
        {
            // use an appropriate slide render for the task
            switch (slide.Format)
            {
                case SlideFormat.Liturgy:
                    return lsr.RenderSlide(_project.Layouts.LiturgyLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.LiturgyVerse:
                    return lvsr.RenderSlide(_project.Layouts.LiturgyLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.LiturgyTitledVerse:
                    return tlvsr.RenderSlide(slide, Messages);
                case SlideFormat.Video:
                    return vsr.RenderSlide(slide, Messages);
                case SlideFormat.UnscaledImage:
                    return isr.RenderImageSlide(slide, Messages);
                case SlideFormat.ScaledImage:
                    return isr.RenderImageSlide(slide, Messages);
                case SlideFormat.AutoscaledImage:
                    return isr.RenderImageSlide(slide, Messages);
                case SlideFormat.FilterImage:
                    return ifr.RenderImageSlide(slide, Messages);
                case SlideFormat.LiturgyImage:
                    return isr.RenderImageSlide(slide, Messages);
                case SlideFormat.StichedImage:
                    return sir.RenderSlide(slide, Messages, _project.Assets);
                case SlideFormat.Reading:
                    return rsr.RenderSlide(_project.Layouts.ReadingLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.SermonTitle:
                    return ssr.RenderSlide(_project.Layouts.SermonLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.AnthemTitle:
                    return atsr.RenderSlide(slide, Messages);
                case SlideFormat.TwoPartTitle:
                    return tpsr.RenderSlide(slide, Messages);
                case SlideFormat.HymnTextVerse:
                    return hvsr.RenderSlide(_project.Layouts.TextHymnLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.Prefab:
                    return psr.RenderSlide(slide, Messages);
                case SlideFormat.Script:
                    return sr.RenderSlide(slide, Messages);
                case SlideFormat.ResourceCopy:
                    return csr.RenderSlide(slide, Messages);
                default:
                    return RenderedSlide.Default();
            }
        }


    }
}
