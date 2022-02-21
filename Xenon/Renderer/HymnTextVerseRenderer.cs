using Xenon.LayoutEngine;
using Xenon.SlideAssembly;
using System.Drawing;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using System.Collections.Generic;
using Xenon.Compiler;
using System;
using System.Text.Json;
using Xenon.Renderer.Helpers;
using System.Linq;

namespace Xenon.Renderer
{
    class HymnTextVerseRenderer : ISlideRenderer, ISlideRenderer<TextHymnLayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>
    {
        public static string DATAKEY_HTITLE { get => "htitle"; }
        public static string DATAKEY_HNAME { get => "hname"; }
        public static string DATAKEY_HNUMBER { get => "hnum"; }
        public static string DATAKEY_HTUNE { get => "htune"; }
        public static string DATAKEY_HCONTENT { get => "hcontent"; }
        public static string DATAKEY_VINFO { get => "vinfo"; }
        public static string DATAKEY_COPYRIGHT { get => "copyright"; }

        [Obsolete]
        public SlideLayout Layouts { get; set; }
        public ILayoutInfoResolver<TextHymnLayoutInfo> LayoutResolver { get => new TextHymnLayoutInfo(); }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            TextHymnLayoutInfo layout = JsonSerializer.Deserialize<TextHymnLayoutInfo>(layoutInfo);

            Bitmap b = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap k = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(b);
            Graphics kgfx = Graphics.FromImage(k);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            foreach (var shape in layout?.Shapes)
            {
                PolygonRenderer.RenderLayoutPreview(gfx, kgfx, shape);
            }

            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.HymnNameBox, "Hymn Name");
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.HymnTitleBox, "Title");
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.VerseInfoBox, "Verse #");
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.HymnNumberBox, "Number ###");
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.HymnTuneBox, "Tune");
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.CopyrightBox, "Copyright");
            PoetryTextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.HymnContentBox);

            return (b, k);
        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<TextHymnLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.HymnTextVerse)
            {
                TextHymnLayoutInfo layout = (this as ISlideRenderer<TextHymnLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        private RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, IAssetResolver assetResolver, TextHymnLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Full" : layout.SlideType;

            //Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            //Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            CommonSlideRenderer.Render(out var ibmp, out var ikbmp, layout);

            Bitmap bmp = ibmp.ToBitmap();
            Bitmap kbmp = ikbmp.ToBitmap();

            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            //gfx.Clear(layout.BackgroundColor.GetColor());
            //kgfx.Clear(layout.KeyColor.GetColor());

            // draw metadata

            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data.GetValueOrDefault(DATAKEY_HNAME, ""), layout.HymnNameBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data.GetValueOrDefault(DATAKEY_HTITLE, ""), layout.HymnTitleBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data.GetValueOrDefault(DATAKEY_VINFO, ""), layout.VerseInfoBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data.GetValueOrDefault(DATAKEY_HNUMBER, ""), layout.HymnNumberBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data.GetValueOrDefault(DATAKEY_HTUNE, ""), layout.HymnTuneBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data.GetValueOrDefault(DATAKEY_COPYRIGHT, ""), layout.CopyrightBox);

            // draw lines

            List<string> lines = new List<string>();
            if (slide.Data.TryGetValue(DATAKEY_HCONTENT, out object lobj))
            {
                lines = lobj as List<string>;
            }

            PoetryTextBoxRenderer.Render(gfx, kgfx, lines, layout.HymnContentBox);

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }
    }
}