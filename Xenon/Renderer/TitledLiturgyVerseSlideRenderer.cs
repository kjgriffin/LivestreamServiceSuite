using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class TitledLiturgyVerseSlideRenderer : ISlideRenderer<TitledLiturgyVerseSlideLayoutInfo>, ISlideRenderer, ISlideLayoutPrototypePreviewer<ASlideLayoutInfo>
    {

        public static string DATAKEY_MODE { get => "mode"; }
        public static string DATAKEY_LINES { get => "lines"; }
        public static string DATAKEY_TITLE { get => "title"; }
        public static string DATAKEY_REF { get => "reference"; }
        public static string DATAKEY_DRAWSPEAKER { get => "drawspeakers"; }

        public ILayoutInfoResolver<TitledLiturgyVerseSlideLayoutInfo> LayoutResolver { get => new TitledLiturgyVerseSlideLayoutInfo(); }

        public Task<(SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32> main, SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32> key)> GetPreviewForLayout(string layoutInfo)
        {
            TitledLiturgyVerseSlideLayoutInfo layout = JsonSerializer.Deserialize<TitledLiturgyVerseSlideLayoutInfo>(layoutInfo);

            Bitmap b = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap k = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(b);
            Graphics kgfx = Graphics.FromImage(k);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            DrawingBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.Banner);

            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.TitleBox);
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.RefBox);
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.ContentTextbox);

            return Task.FromResult((b.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>(), k.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>()));
        }

        public bool IsValidLayoutJson(string json)
        {
            return ISlideLayoutPrototypePreviewer<TitledLiturgyVerseSlideLayoutInfo>._InternalDefaultIsValidLayoutJson(json);
        }

        public RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages, TitledLiturgyVerseSlideLayoutInfo layout)
        {
            if (slide.Data.TryGetValue(DATAKEY_MODE, out object mode))
            {
                if ((string)mode == "legacy")
                {
                    return Internal_RenderSlide_Legacy(slide, messages, layout);
                }
            }
            return Internal_RenderSlide(slide, messages, layout);
        }


        public RenderedSlide Internal_RenderSlide_Legacy(Slide slide, List<XenonCompilerMessage> messages, TitledLiturgyVerseSlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = string.IsNullOrWhiteSpace(layout?.SlideType) ? "Liturgy" : layout.SlideType;


            // draw it

            // for now just draw the layout
            Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());


            DrawingBoxRenderer.Render(gfx, kgfx, layout.Banner);


            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data[DATAKEY_TITLE], layout.TitleBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data[DATAKEY_REF], layout.RefBox);



            Font bf = new Font(layout.ContentTextbox.Font.GetFont(), FontStyle.Bold);
            Font itf = new Font(layout.ContentTextbox.Font.GetFont(), FontStyle.Italic);

            //gfx.DrawString((string)slide.Data["title"], bf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            //kgfx.DrawString((string)slide.Data["title"], bf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.LeftVerticalCenterAlign);
            //gfx.DrawString((string)slide.Data["reference"], itf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);
            //kgfx.DrawString((string)slide.Data["reference"], itf, Brushes.White, layout.TitleLine.Move(layout.Key.Location), GraphicsHelper.RightVerticalCenterAlign);


            List<LiturgyTextLine> Lines = (List<LiturgyTextLine>)slide.Data[DATAKEY_LINES];

            float alltextheight = 0;
            foreach (var line in Lines)
            {
                alltextheight += line.Height;
            }

            float interspace = (layout.ContentTextbox.Textbox.Size.Height - alltextheight) / (Lines.Count + 1);

            float vspace = interspace;
            int linenum = 0;

            string lastspeaker = "";

            Font flsbregular = new Font("LSBSymbol", layout.ContentTextbox.Font.Size, (FontStyle)layout.ContentTextbox.Font.Style);


            foreach (var line in Lines)
            {
                float xoffset = 0;
                // center the text
                xoffset = (layout.ContentTextbox.Textbox.Size.Width / 2) - (line.Width / 2);

                // draw speaker
                if ((string)slide.Data[DATAKEY_DRAWSPEAKER] == "true" && lastspeaker != line.Speaker)
                {
                    SizeF speakersize = gfx.MeasureStringCharacters(line.Speaker, ref flsbregular, new RectangleF(0, 0, 100, 100));
                    float jog = 0.07f * (gfx.DpiY * speakersize.Height / 72);
                    xoffset = (layout.ContentTextbox.Textbox.Size.Width / 2) - ((line.Width) + speakersize.Width) / 2;
                    gfx.DrawString(line.Speaker, flsbregular, Brushes.Teal, layout.ContentTextbox.Textbox.GetRectangle().Move((int)xoffset, (int)(vspace + interspace * linenum)).Move(0, (int)-jog).Location, GraphicsHelper.DefaultStringFormat());
                    kgfx.DrawString(line.Speaker, flsbregular, Brushes.White, layout.ContentTextbox.Textbox.GetRectangle().Move((int)xoffset, (int)(vspace + interspace * linenum)).Move(0, (int)-jog).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += speakersize.Width;
                }

                lastspeaker = line.Speaker;

                xoffset += 30;

                float localmaxh = 0;
                foreach (var word in line.Words)
                {
                    var f = layout.ContentTextbox.Font.GetFont();
                    SizeF wsize = gfx.MeasureStringCharacters(word.Value, ref f, layout.ContentTextbox.Textbox.GetRectangle());

                    gfx.DrawString(word.Value, layout.ContentTextbox.Font.GetFont(), Brushes.White, layout.ContentTextbox.Textbox.GetRectangle().Move((int)xoffset, (int)(vspace + interspace * linenum)).Location, GraphicsHelper.DefaultStringFormat());
                    kgfx.DrawString(word.Value, layout.ContentTextbox.Font.GetFont(), Brushes.White, layout.ContentTextbox.Textbox.GetRectangle().Move((int)xoffset, (int)(vspace + interspace * linenum)).Location, GraphicsHelper.DefaultStringFormat());
                    xoffset += wsize.Width;
                    localmaxh = Math.Max(localmaxh, wsize.Height);
                }
                linenum++;
                vspace += localmaxh;
            }


            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            res.KeyBitmap = kbmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            return res;
        }


        public RenderedSlide Internal_RenderSlide(Slide slide, List<XenonCompilerMessage> messages, TitledLiturgyVerseSlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Liturgy";


            // draw it

            // for now just draw the layout
            Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());


            DrawingBoxRenderer.Render(gfx, kgfx, layout.Banner);


            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data[DATAKEY_TITLE], layout.TitleBox);
            TextBoxRenderer.Render(gfx, kgfx, (string)slide.Data[DATAKEY_REF], layout.RefBox);


            List<LiturgyTextLine> Lines = (List<LiturgyTextLine>)slide.Data[DATAKEY_LINES];

            float alltextheight = 0;
            foreach (var line in Lines)
            {
                alltextheight += line.Height;
            }

            float interspace = (layout.ContentTextbox.Textbox.Size.Height - alltextheight) / (Lines.Count + 1);

            float vspace = interspace;

            Font flsbregular = new Font("LSBSymbol", layout.ContentTextbox.Font.Size, (FontStyle)layout.ContentTextbox.Font.Style);

            StringBuilder sb = new StringBuilder();
            foreach (var line in Lines)
            {
                sb.AppendLine(string.Concat(line.Words));
            }
            TextBoxRenderer.Render(gfx, kgfx, sb.ToString(), layout.ContentTextbox);

            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            res.KeyBitmap = kbmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            return res;
        }

        public Task<RenderedSlide> VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, ISlideRendertimeInfoProvider info, List<XenonCompilerMessage> Messages, RenderedSlide result)
        {
            if (slide.Format == SlideFormat.LiturgyTitledVerse)
            {
                TitledLiturgyVerseSlideLayoutInfo layout = (this as ISlideRenderer<TitledLiturgyVerseSlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                var render = RenderSlide(slide, Messages, layout);
                return Task.FromResult(render);
            }
            return Task.FromResult(result);
        }
    }
}
