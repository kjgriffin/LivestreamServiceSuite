using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class TwoPartTitleSlideRenderer : ISlideRenderer, ISlideRenderer<_2TitleSlideLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {

        public static string DATAKEY_MAINTEXT { get => "maintext"; }
        public static string DATAKEY_SUBTEXT { get => "subtext"; }
        public static string DATAKEY_ORIENTATION { get => "orientation"; }

        public ILayoutInfoResolver<_2TitleSlideLayoutInfo> LayoutResolver { get => new _2TitleSlideLayoutInfo(); }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            _2TitleSlideLayoutInfo layout = JsonSerializer.Deserialize<_2TitleSlideLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            CommonDrawingBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.Banner);

            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.MainText);
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.SubText);

            return (ibmp.ToBitmap(), ikbmp.ToBitmap());
        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<_2TitleSlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, IAssetResolver assetResolver, _2TitleSlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Liturgy" : layout.SlideType;


            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            // this really should be changed to shapes
            CommonDrawingBoxRenderer.Render(ibmp, ikbmp, layout.Banner);


            string orientation = (string)slide.Data.GetOrDefault(DATAKEY_ORIENTATION, "horizontal");
            string maintext = (string)slide.Data.GetOrDefault(DATAKEY_MAINTEXT, "");
            string subtext = (string)slide.Data.GetOrDefault(DATAKEY_SUBTEXT, "");


            // TODO: DEPRECATE this -> force user to explicitly use layout
            if (orientation == "horizontal")
            {
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout._LegacyHorizontalAlignment_MainText, maintext);
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout._LegacyHorizontalAlignment_SubText, subtext);
            }
            else if (orientation == "vertical")
            {
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout._LegacyVerticalAlignment_MainText, maintext);
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout._LegacyVerticalAlignment_SubText, subtext);
            }
            else // use the active layout
            {
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.MainText, maintext);
                CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.SubText, subtext);
            }


            res.Bitmap = ibmp.ToBitmap();
            res.KeyBitmap = ikbmp.ToBitmap();
            return res;
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.TwoPartTitle)
            {
                _2TitleSlideLayoutInfo layout = (this as ISlideRenderer<_2TitleSlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }
    }
}
