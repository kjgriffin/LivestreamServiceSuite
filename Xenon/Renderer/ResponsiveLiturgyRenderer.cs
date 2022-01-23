using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class ResponsiveLiturgyRenderer : ISlideRenderer, ISlideRenderer<ResponsiveLiturgySlideLayoutInfo>, ISlideLayoutPrototypePreviewer<ResponsiveLiturgySlideLayoutInfo>
    {
        public const string DATAKEY = "response-lines";
        public ILayoutInfoResolver<ResponsiveLiturgySlideLayoutInfo> LayoutResolver { get => new ResponsiveLiturgySlideLayoutInfo(); }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<ResponsiveLiturgySlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            ResponsiveLiturgySlideLayoutInfo layout = JsonSerializer.Deserialize<ResponsiveLiturgySlideLayoutInfo>(layoutInfo);

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

            foreach (var textbox in layout.Textboxes)
            {
                TextBoxRenderer.RenderLayoutGhostPreview(gfx, kgfx, textbox);
            }

            // preview a liturgy line in each textbox
            _ = layout.LiturgyLineProto;

            return (b, k);

        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.ResponsiveLiturgy)
            {
                ResponsiveLiturgySlideLayoutInfo layout = (this as ISlideRenderer<ResponsiveLiturgySlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, ResponsiveLiturgySlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";

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

            // Draw All Text
            if (slide.Data.ContainsKey(DATAKEY))
            {
                var lines = (List<object>)slide.Data[DATAKEY];



            }


            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }

    }
}
