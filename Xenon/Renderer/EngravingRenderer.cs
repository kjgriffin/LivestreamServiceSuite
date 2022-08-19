using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.Engraver.Visual;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    internal class EngravingRenderer : ISlideRenderer, ISlideRenderer<EngravingLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {

        public static string DATAKEY_VISUALS { get => "visuals"; }
        public static string DATAKEY_DEBUG_FLAGS { get => "debug-flags"; }

        public ILayoutInfoResolver<EngravingLayoutInfo> LayoutResolver { get => new EngravingLayoutInfo(); }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<EngravingLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.Engraving)
            {
                EngravingLayoutInfo layout = (this as ISlideRenderer<EngravingLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        public (Image<Bgra32> main, Image<Bgra32> key) GetPreviewForLayout(string layoutInfo)
        {
            EngravingLayoutInfo layout = JsonSerializer.Deserialize<EngravingLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            CommonDrawingBoxRenderer.Render(ibmp, ikbmp, layout.Engraving);

            return (ibmp, ikbmp);
        }


        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver asserResolver, EngravingLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Full" : layout.SlideType;

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            CommonDrawingBoxRenderer.Render(ibmp, ikbmp, layout.Engraving);

            if (slide.Data.TryGetValue(DATAKEY_VISUALS, out var visuals) && slide.Data.TryGetValue(DATAKEY_DEBUG_FLAGS, out var df))
            {
                HashSet<string> dflags = df as HashSet<string> ?? new HashSet<string>();
                foreach (IEngravingRenderable item in (visuals as IEnumerable<IEngravingRenderable>))
                {
                    item.Render(layout.Engraving.Box.Origin.X, layout.Engraving.Box.Origin.Y, ibmp, ikbmp, layout, dflags.Contains("bounds"));
                }
            }


            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;
        }


        private void RenderLine()
        {

        }



    }
}
