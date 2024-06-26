﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler;
using Xenon.Compiler.AST;
using Xenon.Helpers;
using Xenon.Renderer.ImageFilters;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class SlideRenderer : IAssetResolver, ISlideRendertimeInfoProvider
    {
        public static string VARNAME_SLIDE_PREMULTIPLEY_OVERRIDE { get => "rendermode.premultiply"; }
        public static string DATAKEY_SLIDE_PREMULTIPLY_OVERRIDE { get => "premultiply.override"; }

        Project _project { get; set; }



        LiturgySlideRenderer lsr = new LiturgySlideRenderer();
        VideoSlideRenderer vsr = new VideoSlideRenderer();
        ImageSlideRenderer isr = new ImageSlideRenderer();
        ReadingSlideRenderer rsr = new ReadingSlideRenderer();
        SermonTitleSlideRenderer ssr = new SermonTitleSlideRenderer();
        PrefabSlideRenderer psr = new PrefabSlideRenderer();
        LiturgyVerseSlideRenderer lvsr = new LiturgyVerseSlideRenderer();
        AnthemTitleSlideRenderer atsr = new AnthemTitleSlideRenderer();
        CopySlideRenderer csr = new CopySlideRenderer();
        ImageFilterRenderer ifr = new ImageFilterRenderer();

        private List<ISlideRenderer> Renderers = new List<ISlideRenderer>
        {
            new StitchedImageRenderer(),
            new TitledLiturgyVerseSlideRenderer(),
            new TitledResponsiveLiturgyRenderer(),
            new ShapeAndTextRenderer(),
            new ShapeImageAndTextRenderer(),
            new ResponsiveLiturgyRenderer(),
            new HymnTextVerseRenderer(),
            new AdvancedImageSlideRenderer(),
            new ComplexShapeImageAndTextRenderer(),
            new RawTextRenderer(),
            new ScriptRenderer(),
            new PrefabSlideRenderer(),
            new HTMLSlideRenderer(),
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
            psr.Layouts = proj.Layouts;
            lvsr.Layouts = proj.Layouts;
            atsr.Layouts = proj.Layouts;
        }

        public async Task<RenderedSlide> RenderSlide(Slide s, List<XenonCompilerMessage> Messages)
        {
            //var res = _RenderSlide(s, Messages);
            var res = await _ApplyRenderers(s, Messages);
            res.Number = s.Number;
            res.OverridingBehaviour = s.OverridingBehaviour;
            if (s.NonRenderedMetadata.TryGetValue(XenonASTExpression.DATAKEY_CMD_SOURCECODE_LOOKUP, out var sourceCodeLookup))
            {
                res.SourceLineRef = (int)sourceCodeLookup;
            }
            if (s.NonRenderedMetadata.TryGetValue(XenonASTExpression.DATAKEY_CMD_SOURCEFILE_LOOKUP, out var sourceFileLookup))
            {
                res.SourceFileRef = (string)sourceFileLookup;
            }

            // attach Postset
            if (s.Data.ContainsKey("postset"))
            {
                res.IsPostset = true;
                res.Postset = (int)s.Data["postset"];
            }

            // attach Pilot
            if (s.Data.TryGetValue(XenonASTExpression.DATAKEY_PILOT, out var pilot))
            {
                res.Pilot = (string)pilot;
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
            if (s.Data.ContainsKey(ShapeImageAndTextRenderer.DATAKEY_PREMULTIPLY_OVERRIDE))
            {
                renderpremultiplied = (bool)s.Data[ShapeImageAndTextRenderer.DATAKEY_PREMULTIPLY_OVERRIDE];
            }
            // let anyone set this on the scope
            if (s.Data.ContainsKey(SlideRenderer.DATAKEY_SLIDE_PREMULTIPLY_OVERRIDE))
            {
                renderpremultiplied = (bool)s.Data[SlideRenderer.DATAKEY_SLIDE_PREMULTIPLY_OVERRIDE];
            }

            if (renderpremultiplied)
            {
                res.Bitmap = res.Bitmap?.PreMultiplyWithAlphaFast(res.KeyBitmap);
            }


            res.BitmapPNGMS = res.Bitmap?.ToPNGStream();
            res.KeyPNGMS = res.KeyBitmap?.ToPNGStream();


            return res;
        }

        private async Task<RenderedSlide> _ApplyRenderers(Slide slide, List<XenonCompilerMessage> Messages)
        {
            RenderedSlide result = null;
            foreach (var render in Renderers)
            {
                // Will let every renderer visit the slide
                // this will enable future features
                // in addition to what we have now 'full slide renderers' we may want to add 'post renderers' that modify exisitng slides
                // this would allow that... though we'd need to ensure a pass order
                result = await render.VisitSlideForRendering(slide, this, this, Messages, result);
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
                    //return isr.RenderImageSlide(slide, Messages);
                    return null;
                case SlideFormat.StitchedImage:
                    //return sir.RenderSlide(slide, Messages, _project.Assets);
                    return null;
                case SlideFormat.Reading:
                    //return rsr.RenderSlide(_project.Layouts.ReadingLayout.GetRenderInfo(), slide, Messages);
                    return null;
                case SlideFormat.SermonTitle:
                    //return ssr.RenderSlide(_project.Layouts.SermonLayout.GetRenderInfo(), slide, Messages);
                    return null;
                case SlideFormat.AnthemTitle:
                    //return atsr.RenderSlide(slide, Messages);
                    return null;
                case SlideFormat.TwoPartTitle:
                    //return tpsr.RenderSlide(slide, Messages);
                    return null;
                case SlideFormat.HymnTextVerse:
                    //return hvsr.RenderSlide(_project.Layouts.TextHymnLayout.GetRenderInfo(), slide, Messages);
                    return null;
                case SlideFormat.Prefab:
                //return psr.RenderSlide(slide, Messages);
                case SlideFormat.Script:
                    //return sr.RenderSlide(slide, Messages);
                    return null;
                case SlideFormat.ResourceCopy:
                    return csr.RenderSlide(slide, Messages);
                default:
                    return RenderedSlide.Default();
            }
        }

        public int FindSlideNumber(string reference)
        {
            // parse reference
            var match = Regex.Match(reference, @"%slide\.num\.(?<label>.*)\.(?<num>\d+)%");
            if (match.Success)
            {
                int num = int.Parse(match.Groups["num"].Value);
                string label = match.Groups["label"].Value;

                // peek into the project
                // find all slides exposing a slide label
                // dump 'er in
                List<Slide> candidates = new List<Slide>();

                foreach (var slide in _project.Slides)
                {
                    if (slide.Data.TryGetValue(XenonASTExpression.DATAKEY_CMD_SOURCESLIDENUM_LABELS, out var d))
                    {
                        var data = d as List<string>;
                        if (data != null && data.Contains(label))
                        {
                            candidates.Add(slide);
                        }
                    }
                }

                // try find slide
                var orderedSlides = candidates.OrderBy(x => x.Number).ToList();
                if (num < 0)
                {
                    orderedSlides.Reverse();
                    num -= 1;
                }
                if (num < orderedSlides.Count)
                {
                    return orderedSlides[num].Number;
                }

            }

            return 0;
        }

        public int FindCameraID(string camName)
        {
            return _project.BMDSwitcherConfig.Routing.FirstOrDefault(x => x.LongName.ToLower() == camName)?.PhysicalInputId ?? 0;
        }
    }
}
