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
            //_ = layout.LiturgyLineProto;

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

            // Draw All Textboxes (without text)
            foreach (var textbox in layout.Textboxes)
            {
                TextBoxRenderer.RenderUnFilled(gfx, kgfx, textbox);
            }

            // Draw All Text
            // for now only draw into first textbox
            var tb = layout.Textboxes.First(); // TODO: this assumes the layout has one.... may not entirley be true (though at that point, like- really?)
            if (slide.Data.ContainsKey(DATAKEY))
            {
                List<SizedTextBlurb> words = (List<SizedTextBlurb>)slide.Data[DATAKEY];

                foreach (var word in words)
                {
                    // just plunk 'er down
                    //using (Font f = new Font(word.AltFont, word.FontSize, word.FontStyle))
                    //{
                    //    gfx.DrawString(word.Text, f, new SolidBrush(tb.FColor.GetColor()), word.Pos);

                    //    Color grayalpha = Color.FromArgb(255, tb.FColor.Alpha, tb.FColor.Alpha, tb.FColor.Alpha);
                    //    kgfx.DrawString(word.Text, f, new SolidBrush(grayalpha), word.Pos);
                    //}
                    word.Render(gfx, kgfx, tb.FColor.GetColor(), Color.White, tb.Font.Name, tb.Font.Size, (FontStyle)tb.Font.Style);
                }
            }


            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }

    }
}
