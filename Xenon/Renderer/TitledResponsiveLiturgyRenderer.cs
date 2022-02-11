using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class TitledResponsiveLiturgyRenderer : ISlideRenderer, ISlideRenderer<TitledResponsiveLiturgySlideLayoutInfo>, ISlideLayoutPrototypePreviewer<TitledResponsiveLiturgySlideLayoutInfo>
    {

        public static string DATAKEY_CONTENTBLOCKS { get => "content-blocks"; }
        public static string DATAKEY_TITLETEXT { get => "title-text-lines"; }

        public ILayoutInfoResolver<TitledResponsiveLiturgySlideLayoutInfo> LayoutResolver { get => new TitledResponsiveLiturgySlideLayoutInfo(); }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            TitledResponsiveLiturgySlideLayoutInfo layout = JsonSerializer.Deserialize<TitledResponsiveLiturgySlideLayoutInfo>(layoutInfo);

            Bitmap b = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap k = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(b);
            Graphics kgfx = Graphics.FromImage(k);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            foreach (var shape in layout.Shapes)
            {
                PolygonRenderer.RenderLayoutPreview(gfx, kgfx, shape);
            }

            foreach (var tbox in layout.TitleBoxes)
            {
                TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, tbox);
            }

            TextBoxRenderer.RenderLayoutGhostPreview(gfx, kgfx, layout.ContentBox);

            // preview a liturgy line in each textbox
            //_ = layout.LiturgyLineProto;

            return (b, k);

        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<TitledResponsiveLiturgySlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
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

            Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            // Draw All Shapes
            foreach (var shape in layout.Shapes)
            {
                PolygonRenderer.Render(gfx, kgfx, shape);
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
                        TextBoxRenderer.Render(gfx, kgfx, tlines[i], tbox);
                    }
                    i++;
                }
            }


            TextBoxRenderer.RenderUnFilled(gfx, kgfx, layout.ContentBox);

            // Draw All Text
            // for now only draw into first textbox
            var tb = layout.ContentBox; // TODO: this assumes the layout has one.... may not entirley be true (though at that point, like- really?)
            if (slide.Data.TryGetValue(DATAKEY_CONTENTBLOCKS, out object objlines))
            {
                List<SizedTextBlurb> words = (objlines as List<SizedTextBlurb>) ?? new List<SizedTextBlurb>();

                foreach (var word in words)
                {
                    word.Render(gfx, kgfx, tb.FColor.GetColor(), Color.White, tb.Font.Name, tb.Font.Size, (FontStyle)tb.Font.Style);
                }
            }


            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;

        }
    }
}
