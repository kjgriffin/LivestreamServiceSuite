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

        private void RenderNormalVerse(HymnTextVerseRenderInfo renderInfo, Slide slide, RenderedSlide res, Bitmap bmp, Bitmap kbmp, Graphics gfx, Graphics kgfx)
        {
            gfx.Clear(Color.White);
            kgfx.Clear(Color.White);

            // debug

            //gfx.DrawRectangle(Pens.Green, Layouts.TextHymnLayout.CopyrightBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NameBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.NumberBox);
            //gfx.DrawRectangle(Pens.Blue, Layouts.TextHymnLayout.TitleBox);
            //gfx.DrawRectangle(Pens.Red, Layouts.TextHymnLayout.TextBox);

            // draw name
            string name = (string)slide.Data["name"];
            gfx.DrawString(name, renderInfo.NameFont, Brushes.Black, Layouts.TextHymnLayout.NameBox, GraphicsHelper.CenterAlign);

            // draw title
            string titleandsubname = (string)slide.Data["sub-name"] == "" ? (string)slide.Data["title"] : $"{(string)slide.Data["title"]}\r\n{(string)slide.Data["sub-name"]}";
            gfx.DrawString(titleandsubname, renderInfo.TitleFont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

            // draw sub-title
            //gfx.DrawString((string)slide.Data["sub-name"], renderInfo.TitleFont, Brushes.Black, Layouts.TextHymnLayout.TitleBox, GraphicsHelper.CenterAlign);

            // draw number and tune
            string numberandtune = (string)slide.Data["tune"] == "" ? $"{(string)slide.Data["number"]}" : $"{(string)slide.Data["number"]}\r\n{(string)slide.Data["tune"]}";
            gfx.DrawString(numberandtune, renderInfo.NumberFont, Brushes.Black, Layouts.TextHymnLayout.NumberBox, GraphicsHelper.CenterAlign);

            // draw copyright
            gfx.DrawString((string)slide.Data["copyright"], renderInfo.CopyrightFont, Brushes.Gray, Layouts.TextHymnLayout.CopyrightBox, GraphicsHelper.CenterAlign);

            // draw text
            // compute interline spacing



            double lineheight = gfx.MeasureString(slide.Lines[0].Content[0].Data, renderInfo.VerseFont).Height;
            double interspace = (Layouts.TextHymnLayout.TextBox.Height - (slide.Lines.Count * lineheight)) / (slide.Lines.Count + 1);

            double offset = 0;
            Rectangle vline = new Rectangle(0, Layouts.TextHymnLayout.TextBox.Y, Layouts.TextHymnLayout.TextBox.Width, (int)lineheight).Move(new Point(0, (int)interspace));

            foreach (var line in slide.Lines)
            {
                gfx.DrawString(line.Content[0].Data, renderInfo.VerseFont, Brushes.Black, vline.Move(new Point(0, (int)offset)), GraphicsHelper.CenterAlign);
                offset += interspace + lineheight;
            }

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
        }

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

            Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

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