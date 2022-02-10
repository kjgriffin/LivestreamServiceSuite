using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using Xenon.SlideAssembly;

namespace Xenon.Renderer
{
    class TwoPartTitleSlideRenderer : ISlideRenderer, ISlideRenderer<_2TitleSlideLayoutInfo>, ISlideLayoutPrototypePreviewer<ALayoutInfo>
    {
        public ILayoutInfoResolver<_2TitleSlideLayoutInfo> LayoutResolver { get => new _2TitleSlideLayoutInfo(); }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            _2TitleSlideLayoutInfo layout = JsonSerializer.Deserialize<_2TitleSlideLayoutInfo>(layoutInfo);

            Bitmap b = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap k = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(b);
            Graphics kgfx = Graphics.FromImage(k);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            DrawingBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.Banner);

            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.MainText);
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.SubText);

            return (b, k);
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

            Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            //gfx.FillRectangle(Brushes.Black, layout.Key);
            //kgfx.FillRectangle(new SolidBrush(slide.Colors["keytrans"]), layout.Key);

            DrawingBoxRenderer.Render(gfx, kgfx, layout.Banner);


            string orientation = (string)slide.Data.GetOrDefault("orientation", "horizontal");
            string maintext = (string)slide.Data.GetOrDefault("maintext", "");
            string subtext = (string)slide.Data.GetOrDefault("subtext", "");

            if (orientation == "horizontal")
            {
                TextBoxRenderer.Render(gfx, kgfx, maintext, layout._LegacyHorizontalAlignment_MainText);
                TextBoxRenderer.Render(gfx, kgfx, subtext, layout._LegacyHorizontalAlignment_SubText);
            }
            else if (orientation == "vertical")
            {
                TextBoxRenderer.Render(gfx, kgfx, maintext, layout._LegacyVerticalAlignment_MainText);
                TextBoxRenderer.Render(gfx, kgfx, subtext, layout._LegacyVerticalAlignment_SubText);
            }
            else // use the active layout
            {
                TextBoxRenderer.Render(gfx, kgfx, maintext, layout.MainText);
                TextBoxRenderer.Render(gfx, kgfx, subtext, layout.SubText);
            }


            /*

            //Font bf = new Font("Arial", 36, FontStyle.Bold);

            var lineheight = gfx.MeasureStringCharacters(slide.Lines[0].Content[0].Data, ref bf, Layouts.AnthemTitleLayout.Key);

            if ((string)slide.Data["orientation"] == "vertical")
            {
                gfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, layout.Line1.Move(layout.Key.Location), GraphicsHelper.CenterAlign);
                kgfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, layout.Line1.Move(layout.Key.Location), GraphicsHelper.CenterAlign);
                gfx.DrawString(slide.Lines[1].Content[0].Data, layout.Font, Brushes.White, layout.Line2.Move(layout.Key.Location), GraphicsHelper.CenterAlign);
                kgfx.DrawString(slide.Lines[1].Content[0].Data, layout.Font, Brushes.White, layout.Line2.Move(layout.Key.Location), GraphicsHelper.CenterAlign);
            }
            else
            {
                int ycord = (int)((layout.Key.Height / 2) - (lineheight.Height / 2));
                gfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, layout.MainLine.Move(layout.Key.Location).Move(0, ycord), GraphicsHelper.LeftVerticalCenterAlign);
                kgfx.DrawString(slide.Lines[0].Content[0].Data, bf, Brushes.White, layout.MainLine.Move(layout.Key.Location).Move(0, ycord), GraphicsHelper.LeftVerticalCenterAlign);
                gfx.DrawString(slide.Lines[1].Content[0].Data, layout.Font, Brushes.White, layout.MainLine.Move(layout.Key.Location).Move(0, ycord), GraphicsHelper.RightVerticalCenterAlign);
                kgfx.DrawString(slide.Lines[1].Content[0].Data, layout.Font, Brushes.White, layout.MainLine.Move(layout.Key.Location).Move(0, ycord), GraphicsHelper.RightVerticalCenterAlign);
            }
            */




            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
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
