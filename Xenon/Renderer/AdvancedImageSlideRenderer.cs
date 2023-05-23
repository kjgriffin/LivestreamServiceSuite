using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.Text.Json;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class AdvancedImageSlideRenderer : ISlideRenderer, ISlideRenderer<AdvancedImagesSlideLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {
        public static string DATAKEY_IMAGES { get => "images"; }

        public ILayoutInfoResolver<AdvancedImagesSlideLayoutInfo> LayoutResolver { get => new AdvancedImagesSlideLayoutInfo(); }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<AdvancedImagesSlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }
        public (Image<Bgra32> main, Image<Bgra32> key) GetPreviewForLayout(string layoutInfo)
        {
            AdvancedImagesSlideLayoutInfo layout = JsonSerializer.Deserialize<AdvancedImagesSlideLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.RenderLayoutPreview(ibmp, ikbmp, shape);
            }

            foreach (var advimg in layout.Images)
            {
                CommonAdvancedImageDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, advimg);
            }

            return (ibmp, ikbmp);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.AdvancedImages)
            {
                AdvancedImagesSlideLayoutInfo layout = (this as ISlideRenderer<AdvancedImagesSlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        private RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, IAssetResolver projassets, AdvancedImagesSlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Liturgy" : layout.SlideType;

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.Render(ibmp, ikbmp, shape);
            }

            List<string> imageNames = slide.Data.GetOrDefault(DATAKEY_IMAGES, new List<string>()) as List<string>;

            int i = 0;
            foreach (var image in layout.Images)
            {
                if (i < imageNames.Count)
                    try
                    {
                        Image<Bgra32> src = Image.Load<Bgra32>(projassets.GetProjectAssetByName(imageNames[i]).CurrentPath);
                        CommonAdvancedImageDrawingBoxRenderer.Render(ibmp, ikbmp, image, src);
                    }
                    catch (Exception ex)
                    {
                        messages.Add(new XenonCompilerMessage
                        {
                            ErrorMessage = "Error loading image",
                            ErrorName = "Error Loading Asset",
                            Generator = "AdvancedImageSlideRenderer",
                            Inner = ex.Message,
                            Level = Compiler.XenonCompilerMessageType.Error,
                            Token = imageNames[i]
                        });
                        return null;
                    }
                i++;
            }

            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;
        }
    }
}
