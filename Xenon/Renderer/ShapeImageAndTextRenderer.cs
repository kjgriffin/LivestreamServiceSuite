using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using GDI = System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class ShapeImageAndTextRenderer : ISlideRenderer, ISlideRenderer<ShapeImageAndTextLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {
        public static string DATAKEY_BKGDIMAGES { get => "assets.background"; }
        public static string DATAKEY_FGDIMAGES { get => "assets.foreground"; }
        public static string DATAKEY_TEXTS { get => "shape-and-text-strings"; }
        public static string DATAKEY_FALLBACKLAYOUT { get => "fallback-layout"; }
        public ILayoutInfoResolver<ShapeImageAndTextLayoutInfo> LayoutResolver { get => new ShapeImageAndTextLayoutInfo(); }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<ShapeImageAndTextLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.CustomDraw)
            {
                ShapeImageAndTextLayoutInfo layout = (this as ISlideRenderer<ShapeImageAndTextLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        public (GDI.Bitmap main, GDI.Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            ShapeImageAndTextLayoutInfo layout = JsonSerializer.Deserialize<ShapeImageAndTextLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var image in layout.Images)
            {
                CommonDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, image);
            }

            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.RenderLayoutPreview(ibmp, ikbmp, shape);
            }

            int i = 1;
            foreach (var textbox in layout.Textboxes)
            {
                CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, textbox, $"Example Text {i++}");
            }

            foreach (var image in layout.Branding)
            {
                CommonDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, image);
            }

            return (ibmp.ToBitmap(), ikbmp.ToBitmap());
        }


        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, ShapeImageAndTextLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Liturgy" : layout.SlideType;

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            if (slide.Data.TryGetValue(DATAKEY_BKGDIMAGES, out object a))
            {
                var assets = a as List<ProjectAsset> ?? new List<ProjectAsset>();
                int i = 0;

                foreach (var image in layout.Images)
                {
                    if (i < assets.Count)
                    {
                        if (assets[i] != null && assets[i].Type == AssetType.Image)
                        {
                            try
                            {
                                Image<Bgra32> src = Image.Load<Bgra32>(assets[i].CurrentPath);
                                CommonDrawingBoxRenderer.Render(ibmp, ikbmp, image, src);
                            }
                            catch (Exception)
                            {
                                // let if fail silently?
                            }
                        }
                    }
                }
            }


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

            if (slide.Data.TryGetValue(DATAKEY_FGDIMAGES, out a))
            {
                var assets = a as List<ProjectAsset> ?? new List<ProjectAsset>();
                int i = 0;

                foreach (var image in layout.Branding)
                {
                    if (i < assets.Count)
                    {
                        if (assets[i] != null && assets[i].Type == AssetType.Image)
                        {
                            try
                            {
                                Image<Bgra32> src = Image.Load<Bgra32>(assets[i].CurrentPath);
                                CommonDrawingBoxRenderer.Render(ibmp, ikbmp, image, src);
                            }
                            catch (Exception)
                            {
                                // let if fail silently?
                            }
                        }
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
