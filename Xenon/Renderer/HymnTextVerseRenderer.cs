using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers.ImageSharp;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class HymnTextVerseRenderer : ISlideRenderer, ISlideRenderer<TextHymnLayoutInfo>, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {
        public static string DATAKEY_HTITLE { get => "htitle"; }
        public static string DATAKEY_HNAME { get => "hname"; }
        public static string DATAKEY_HNUMBER { get => "hnum"; }
        public static string DATAKEY_HTUNE { get => "htune"; }
        public static string DATAKEY_HCONTENT { get => "hcontent"; }
        public static string DATAKEY_VINFO { get => "vinfo"; }
        public static string DATAKEY_COPYRIGHT { get => "copyright"; }
        public static string DATAKEY_DOXOLOGICAL { get => "doxological"; }

        [Obsolete]
        public SlideLayout Layouts { get; set; }
        public ILayoutInfoResolver<TextHymnLayoutInfo> LayoutResolver { get => new TextHymnLayoutInfo(); }

        public Task<(Image<Bgra32> main, Image<Bgra32> key)> GetPreviewForLayout(string layoutInfo)
        {
            TextHymnLayoutInfo layout = JsonSerializer.Deserialize<TextHymnLayoutInfo>(layoutInfo);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            foreach (var shape in layout?.Shapes)
            {
                CommonPolygonRenderer.RenderLayoutPreview(ibmp, ikbmp, shape);
            }

            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.HymnNameBox, "Hymn Name");
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.HymnTitleBox, "Title");
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.VerseInfoBox, "Verse #");
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.HymnNumberBox, "Number ###");
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.HymnTuneBox, "Tune");
            CommonTextBoxRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.CopyrightBox, "Copyright");
            CommonPoetryTextRenderer.RenderLayoutPreview(ibmp, ikbmp, layout.HymnContentBox);

            return Task.FromResult((ibmp, ikbmp));
        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<TextHymnLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public Task<RenderedSlide> VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide result)
        {
            if (slide.Format == SlideFormat.HymnTextVerse)
            {
                TextHymnLayoutInfo layout = (this as ISlideRenderer<TextHymnLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                var render = RenderSlide(slide, Messages, assetResolver, layout);
                return Task.FromResult(render);
            }
            return Task.FromResult(result);
        }

        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver assetResolver, TextHymnLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Full" : layout.SlideType;

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            // draw shapes
            foreach (var shape in layout?.Shapes)
            {
                CommonPolygonRenderer.Render(ibmp, ikbmp, shape);
            }

            // draw metadata
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.HymnNameBox, (string)slide.Data.GetValueOrDefault(DATAKEY_HNAME, ""));
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.HymnTitleBox, (string)slide.Data.GetValueOrDefault(DATAKEY_HTITLE, ""));
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.VerseInfoBox, (string)slide.Data.GetValueOrDefault(DATAKEY_VINFO, ""));
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.HymnNumberBox, (string)slide.Data.GetValueOrDefault(DATAKEY_HNUMBER, ""));
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.HymnTuneBox, (string)slide.Data.GetValueOrDefault(DATAKEY_HTUNE, ""));
            CommonTextBoxRenderer.Render(ibmp, ikbmp, layout.CopyrightBox, (string)slide.Data.GetValueOrDefault(DATAKEY_COPYRIGHT, ""));

            // draw lines
            List<string> lines = new List<string>();
            if (slide.Data.TryGetValue(DATAKEY_HCONTENT, out object lobj))
            {
                lines = lobj as List<string>;
            }

            bool doxological = (bool)(slide.Data.GetValueOrDefault(DATAKEY_DOXOLOGICAL, false));

            CommonPoetryTextRenderer.Render(ibmp, ikbmp, layout.HymnContentBox, lines, doxological, "D    ", "LSBSymbol");

            res.Bitmap = ibmp;
            res.KeyBitmap = ikbmp;
            return res;
        }
    }
}