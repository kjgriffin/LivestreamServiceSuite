using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.AssetManagment;
using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class ShapeImageAndTextRenderer : ISlideRenderer, ISlideRenderer<ShapeImageAndTextLayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>
    {
        public static string DATAKEY_BKGDIMAGES { get => "assets.background"; }
        public static string DATAKEY_FGDIMAGES { get => "assets.foreground"; }
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

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            ShapeImageAndTextLayoutInfo layout = JsonSerializer.Deserialize<ShapeImageAndTextLayoutInfo>(layoutInfo);

            Bitmap b = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap k = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(b);
            Graphics kgfx = Graphics.FromImage(k);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            foreach (var image in layout.Images)
            {
                DrawingBoxRenderer.RenderLayoutPreview(gfx, kgfx, image);
            }

            foreach (var shape in layout.Shapes)
            {
                PolygonRenderer.RenderLayoutPreview(gfx, kgfx, shape);
            }

            int i = 1;
            foreach (var textbox in layout.Textboxes)
            {
                TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, textbox, $"Example Text {i++}");
            }

            foreach (var image in layout.Branding)
            {
                DrawingBoxRenderer.RenderLayoutPreview(gfx, kgfx, image);
            }


            return (b, k);
        }


        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, ShapeImageAndTextLayoutInfo layout)
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
                            DrawingBoxRenderer.Render(gfx, kgfx, image);
                            try
                            {
                                Bitmap src = new Bitmap(assets[i].CurrentPath);
                                gfx.DrawImage(src, image.Box.GetRectangleF(), new RectangleF(PointF.Empty, src.Size), GraphicsUnit.Pixel);
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
                            DrawingBoxRenderer.Render(gfx, kgfx, image);
                            try
                            {
                                Bitmap src = new Bitmap(assets[i].CurrentPath);
                                gfx.DrawImage(src, image.Box.GetRectangleF(), new RectangleF(PointF.Empty, src.Size), GraphicsUnit.Pixel);
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


            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }
    }
}
