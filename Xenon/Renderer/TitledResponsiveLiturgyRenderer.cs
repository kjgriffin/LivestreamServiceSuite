using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Collections.Generic;
using System.Text.Json;

using Xenon.Compiler;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class TitledResponsiveLiturgyRenderer : ISlideRenderer, ISlideRenderer<TitledResponsiveLiturgySlideLayoutInfo>, ISlideLayoutPrototypePreviewer<TitledResponsiveLiturgySlideLayoutInfo>
    {

        public static string DATAKEY_CONTENTBLOCKS { get => "content-blocks"; }
        public static string DATAKEY_TITLETEXT { get => "title-text-lines"; }

        public ILayoutInfoResolver<TitledResponsiveLiturgySlideLayoutInfo> LayoutResolver { get => new TitledResponsiveLiturgySlideLayoutInfo(); }

        public (Image<Bgra32> main, Image<Bgra32> key) GetPreviewForLayout(string layoutInfo)
        {
            TitledResponsiveLiturgySlideLayoutInfo layout = JsonSerializer.Deserialize<TitledResponsiveLiturgySlideLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.RenderLayoutPreview(ibmp, ikbmp, shape);
            }

            foreach (var tbox in layout.TitleBoxes)
            {
                CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, tbox);
            }

            CommonTextBoxRenderer.RenderLayoutGhostPreview(ibmp, ikbmp, layout.ContentBox);

            // preview a liturgy line in each textbox
            //_ = layout.LiturgyLineProto;

            return (ibmp, ikbmp);
        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<TitledResponsiveLiturgySlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.ResponsiveLiturgyTitledVerse)
            {
                TitledResponsiveLiturgySlideLayoutInfo layout = (this as ISlideRenderer<TitledResponsiveLiturgySlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderedSlide(slide, Messages, assetResolver, layout);
            }
        }

        private RenderedSlide RenderedSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver assetResolver, TitledResponsiveLiturgySlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Liturgy" : layout.SlideType;

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            // Draw All Shapes
            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.Render(ibmp, ikbmp, shape);
            }

            // draw titles
            if (slide.Data.TryGetValue(DATAKEY_TITLETEXT, out object objtitles))
            {
                List<string> tlines = (objtitles as List<string>) ?? new List<string>();
                int i = 0;
                foreach (var tbox in layout.TitleBoxes)
                {
                    if (i < tlines.Count)
                    {
                        CommonTextBoxRenderer.Render(ibmp, ikbmp, tbox, tlines[i]);
                    }
                    i++;
                }
            }


            CommonTextBoxRenderer.RenderUnfilled(ibmp, ikbmp, layout.ContentBox);

            // Draw All Text
            // for now only draw into first textbox
            var tb = layout.ContentBox; // TODO: this assumes the layout has one.... may not entirley be true (though at that point, like- really?)
            if (slide.Data.TryGetValue(DATAKEY_CONTENTBLOCKS, out object objlines))
            {
                List<SizedTextBlurb> words = (objlines as List<SizedTextBlurb>) ?? new List<SizedTextBlurb>();

                foreach (var word in words)
                {
                    word.Render(ibmp, ikbmp, layout.ContentBox);
                }
            }


            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;

        }
    }
}
