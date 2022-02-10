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
    internal class ShapeAndTextRenderer : ISlideRenderer, ISlideRenderer<ShapeAndTextLayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>
    {
        public ILayoutInfoResolver<ShapeAndTextLayoutInfo> LayoutResolver { get => new ShapeAndTextLayoutInfo(); }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<ShapeAndTextLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.ShapesAndTexts)
            {
                ShapeAndTextLayoutInfo layout = (this as ISlideRenderer<ShapeAndTextLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            ShapeAndTextLayoutInfo layout = JsonSerializer.Deserialize<ShapeAndTextLayoutInfo>(layoutInfo);

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

            int i = 1;
            foreach (var textbox in layout.Textboxes)
            {
                TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, textbox, $"Example Text {i++}");
            }

            return (b, k);
        }


        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, ShapeAndTextLayoutInfo layout)
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
            if (slide.Data.TryGetValue("shape-and-text-strings", out object val))
            {
                List<string> strings = val as List<string> ?? new List<string>();
                int i = 0;
                foreach (var textbox in layout.Textboxes)
                {
                    if (i < strings.Count)
                    {
                        TextBoxRenderer.Render(gfx, kgfx, strings[i], textbox);
                    }
                    i++;
                }
            }

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }
    }
}
