using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutEngine.L2;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.Renderer.Helpers.ImageSharp;
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

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.RenderLayoutPreview(ibmp, ikbmp, shape);
            }

            foreach (var textbox in layout.Textboxes)
            {
                CommonTextBoxRenderer.RenderLayoutGhostPreview(ibmp, ikbmp, textbox);
            }

            // preview a liturgy line in each textbox
            //_ = layout.LiturgyLineProto;

            return (ibmp.ToBitmap(), ikbmp.ToBitmap());

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

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            // Draw All Shapes
            foreach (var shape in layout.Shapes)
            {
                CommonPolygonRenderer.Render(ibmp, ikbmp, shape);
            }

            // Draw All Textboxes (without text)
            foreach (var textbox in layout.Textboxes)
            {
                CommonTextBoxRenderer.RenderUnfilled(ibmp, ikbmp, textbox);
            }

            // Draw All Text
            // for now only draw into first textbox
            var tb = layout.Textboxes.First(); // TODO: this assumes the layout has one.... may not entirley be true (though at that point, like- really?)
            if (slide.Data.ContainsKey(DATAKEY))
            {
                List<SizedTextBlurb> words = (List<SizedTextBlurb>)slide.Data[DATAKEY];

                foreach (var word in words)
                {
                    //word.Render(gfx, kgfx, tb.FColor.GetColor(), Color.White, tb.Font.Name, tb.Font.Size, (FontStyle)tb.Font.Style);
                    //word.Render(ibmp, ikbmp, tb.FColor.ToColor(), SixLabors.ImageSharp.Color.FromRgb((byte)tb.FColor.Alpha, (byte)tb.FColor.Alpha, (byte)tb.FColor.Alpha), tb.Font.Name, tb.Font.Size, (SixLabors.Fonts.FontStyle)tb.Font.Style);
                    word.Render(ibmp, ikbmp, tb);
                }
            }


            res.Bitmap = ibmp.ToBitmap();
            res.KeyBitmap = ikbmp.ToBitmap();
            return res;
        }

    }
}
