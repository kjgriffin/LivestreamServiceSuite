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
using Xenon.LayoutEngine.L2;

namespace Xenon.Renderer
{
    internal class ComplexShapeImageAndTextRenderer : ISlideRenderer, ISlideRenderer<ComplexShapeImageAndTextLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {
        public static string DATAKEY_BKGDIMAGES { get => "assets.background"; }
        public static string DATAKEY_FGDIMAGES { get => "assets.foreground"; }
        public static string DATAKEY_TEXTS { get => "shape-and-text-strings"; }
        public static string DATAKEY_COMPLEX_TEXT { get => "complex-text"; }
        public static string DATAKEY_FALLBACKLAYOUT { get => "fallback-layout"; }
        public ILayoutInfoResolver<ComplexShapeImageAndTextLayoutInfo> LayoutResolver { get => new ComplexShapeImageAndTextLayoutInfo(); }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<ComplexShapeImageAndTextLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.ComplexText)
            {
                ComplexShapeImageAndTextLayoutInfo layout = (this as ISlideRenderer<ComplexShapeImageAndTextLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        public (Image<Bgra32> main, Image<Bgra32> key) GetPreviewForLayout(string layoutInfo)
        {
            ComplexShapeImageAndTextLayoutInfo layout = JsonSerializer.Deserialize<ComplexShapeImageAndTextLayoutInfo>(layoutInfo);

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
            foreach (var textbox in layout.ComplexBoxes)
            {
                CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, textbox, $"Complex Text {i++}");
            }

            i = 1;
            foreach (var textbox in layout.Textboxes)
            {
                CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, textbox, $"Example Text {i++}");
            }

            foreach (var image in layout.Branding)
            {
                CommonDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, image);
            }

            return (ibmp, ikbmp);
        }


        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, ComplexShapeImageAndTextLayoutInfo layout)
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
                                CommonAdvancedImageDrawingBoxRenderer.Render(ibmp, ikbmp, image, src);
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

            // Draw complex text
            if (slide.Data.TryGetValue(DATAKEY_COMPLEX_TEXT, out object ctxt))
            {
                List<SizedTextBlurb> words = (List<SizedTextBlurb>)ctxt;
                var tb = layout.ComplexBoxes.First(); // for now only render the first one
                foreach (var word in words)
                {
                    word.Render(ibmp, ikbmp, tb);
                }
            }

            // Draw all other text
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
                                CommonAdvancedImageDrawingBoxRenderer.Render(ibmp, ikbmp, image, src);
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
