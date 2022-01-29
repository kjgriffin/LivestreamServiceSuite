using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xenon.Compiler;
using Xenon.Renderer.ImageFilters;
using System.Linq;
using Xenon.LayoutInfo;
using Xenon.AssetManagment;

namespace Xenon.Renderer
{
    class SlideRenderer : IAssetResolver
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
        //StitchedImageRenderer sir = new StitchedImageRenderer();

        private List<ISlideRenderer> Renderers = new List<ISlideRenderer>
        {
            new StitchedImageRenderer(),
            new TwoPartTitleSlideRenderer(),
            new TitledLiturgyVerseSlideRenderer(),
            new ShapeAndTextRenderer(),
            new ShapeImageAndTextRenderer(),
            new ResponsiveLiturgyRenderer(),
        };

        public ProjectAsset GetProjectAssetByName(string assetName)
        {
            return _project.Assets.FirstOrDefault(a => a.Name == assetName);
        }


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
            //tpsr.Layouts = proj.Layouts;
            //tlvsr.Layouts = proj.Layouts;
        }

        public RenderedSlide RenderSlide(Slide s, List<XenonCompilerMessage> Messages)
        {
            //var res = _RenderSlide(s, Messages);
            var res = _ApplyRenderers(s, Messages);
            res.Number = s.Number;
            res.OverridingBehaviour = s.OverridingBehaviour;

            // attach Postset
            if (s.Data.ContainsKey("postset"))
            {
                res.IsPostset = true;
                res.Postset = (int)s.Data["postset"];
            }

            // we can premultiple here if nessecary
            bool renderpremultiplied = true;
            if (_project.ProjectVariables.TryGetValue("global.rendermode.alpha", out List<string> rendermode))
            {
                // this should handle precedence of auto-compatibility upgrades
                if (rendermode.Any(s => s == "legacy"))
                {
                    renderpremultiplied = false;
                }
                if (rendermode.Any(s => s == "premultiplied"))
                {
                    renderpremultiplied = true;
                }
            }
            if (renderpremultiplied)
            {
                res.Bitmap = res.Bitmap?.PreMultiplyWithAlphaFast(res.KeyBitmap);
            }


            return res;
        }

        private RenderedSlide _ApplyRenderers(Slide slide, List<XenonCompilerMessage> Messages)
        {
            RenderedSlide result = null;
            foreach (var render in Renderers)
            {
                // Will let every renderer visit the slide
                // this will enable future features
                // in addition to what we have now 'full slide renderers' we may want to add 'post renderers' that modify exisitng slides
                // this would allow that... though we'd need to ensure a pass order
                render.VisitSlideForRendering(slide, this, Messages, ref result);
            }
            // TODO: remove once we switch everything over to the new way
            if (result == null)
            {
                return _RenderSlide(slide, Messages);
            }
            return result ?? RenderedSlide.Default();
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
                    //return tlvsr.RenderSlide(slide, Messages);
                    return null;
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
                case SlideFormat.StitchedImage:
                    //return sir.RenderSlide(slide, Messages, _project.Assets);
                    return null;
                case SlideFormat.Reading:
                    return rsr.RenderSlide(_project.Layouts.ReadingLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.SermonTitle:
                    return ssr.RenderSlide(_project.Layouts.SermonLayout.GetRenderInfo(), slide, Messages);
                case SlideFormat.AnthemTitle:
                    return atsr.RenderSlide(slide, Messages);
                case SlideFormat.TwoPartTitle:
                    //return tpsr.RenderSlide(slide, Messages);
                    return null;
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
