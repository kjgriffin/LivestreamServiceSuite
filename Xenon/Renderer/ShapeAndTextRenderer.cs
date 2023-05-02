using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Collections.Generic;
using System.Text.Json;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class ShapeAndTextRenderer : ISlideRenderer, ISlideRenderer<ShapeAndTextLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {
        public static string DATAKEY_TEXTS { get => "shape-and-text-strings"; }
        public static string DATAKEY_FALLBACKLAYOUT { get => "fallback-layout"; }

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

        public (Image<Bgra32> main, Image<Bgra32> key) GetPreviewForLayout(string layoutInfo)
        {
            ShapeAndTextLayoutInfo layout = JsonSerializer.Deserialize<ShapeAndTextLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.RenderLayoutPreview(ibmp, ikbmp, shape);
            }

            int i = 1;
            foreach (var textbox in layout.Textboxes)
            {
                CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, textbox, $"Example Text {i++}");
            }

            return (ibmp, ikbmp);
        }


        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, ShapeAndTextLayoutInfo layout)
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

            // Draw All Text
            if (slide.Data.TryGetValue(DATAKEY_TEXTS, out object val))
            {
                List<string> strings = val as List<string> ?? new List<string>();
                int i = 0;
                foreach (var textbox in layout.Textboxes)
                {
                    if (i < strings.Count)
                    {
                        CommonTextBoxRenderer.Render(ibmp, ikbmp, textbox, strings[i]);
                    }
                    i++;
                }
            }

            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;
        }
    }
}
