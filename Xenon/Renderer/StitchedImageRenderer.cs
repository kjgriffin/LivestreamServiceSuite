using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.SlideAssembly;
using System.Linq;
using Xenon.Helpers;
using Xenon.LayoutEngine;
using Xenon.AssetManagment;
using Xenon.Compiler;
using Xenon.LayoutInfo;
using Xenon.Renderer.Helpers;
using System.Text.Json;

namespace Xenon.Renderer
{
    class StitchedImageRenderer : ISlideRenderer<StitchedImageSlideLayoutInfo>, ISlideRenderer, ISlideLayoutPrototypePreviewer<ALayoutInfo>
    {

        public RenderedSlide RenderSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages, IAssetResolver projassets, StitchedImageSlideLayoutInfo layout)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap kbmp = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            List<LSBImageResource> imageresources = (List<LSBImageResource>)slide.Data.GetOrDefault("ordered-images", new List<LSBImageResource>());
            string title = (string)slide.Data.GetOrDefault("title", "");
            string hymnname = (string)slide.Data.GetOrDefault("hymnname", "");
            string number = (string)slide.Data.GetOrDefault("number", "");


            // create new bitmap for the image for now...
            // compute max bounds of new bitmap
            // 1920x100 box for title info
            // find max width of images and compare
            int twidth = (int)gfx.MeasureString(hymnname, layout.NameBox.Font.GetFont()).Width;
            int width = Math.Max(twidth, imageresources.Select(i => i.Size.Width).Max());
            int height = imageresources.Select(i => i.Size.Height).Aggregate((a, b) => a + b); // + 30?
            Bitmap sbmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(sbmp);

            g.Clear(layout.MusicBox.FillColor.GetColor());


            //int tboxheight = 120;
            //int cboxheight = 30;


            // draw all images
            int hoff = 0;
            foreach (var image in imageresources)
            {
                // load image
                try
                {
                    Bitmap b = new Bitmap(projassets.GetProjectAssetByName(image.AssetRef).CurrentPath);
                    int x = (width - b.Width) / 2;
                    g.DrawImage(b, x, hoff, b.Width, b.Height);
                    //g.DrawRectangle(Pens.Red, x, hoff, b.Width, b.Height);
                    hoff += b.Height;
                }
                catch (Exception ex)
                {
                    messages.Add(new Compiler.XenonCompilerMessage() { ErrorMessage = "Error loading image", ErrorName = "Error Loading Asset", Generator = "StitchedImageRenderer", Inner = ex.Message, Level = Compiler.XenonCompilerMessageType.Error, Token = image.AssetRef });
                    return null;
                }
            }

            var hdrawn = ImageFilters.ImageFilters.UniformStretch(sbmp, sbmp, new ImageFilters.UniformStretchFilterParams() { Fill = layout.MusicBox.FillColor.GetColor(), KFill = layout.MusicBox.KeyColor.GetColor(), Height = layout.MusicBox.Box.Size.Height, Width = layout.MusicBox.Box.Size.Width });


            gfx.Clear(layout.BackgroundColor.GetColor());


            DrawingBoxRenderer.Render(gfx, kgfx, layout.MusicBox);

            gfx.DrawImage(hdrawn.b, layout.MusicBox.Box.Origin.X, layout.MusicBox.Box.Origin.Y);

            // draw titles

            TextBoxRenderer.Render(gfx, kgfx, hymnname, layout.NameBox);
            TextBoxRenderer.Render(gfx, kgfx, title, layout.TitleBox);
            TextBoxRenderer.Render(gfx, kgfx, number, layout.NumberBox);
            
            //gfx.DrawString(hymnname, layout.NameBox.Font.GetFont(), new SolidBrush(layout.NameBox.FColor.GetColor()), layout.NameBox.Textbox.GetRectangle(), GraphicsHelper.CenterAlign);
            //gfx.DrawString(title, layout.TitleBox.Font.GetFont(), new SolidBrush(layout.TitleBox.FColor.GetColor()), layout.TitleBox.Textbox.GetRectangle(), GraphicsHelper.LeftVerticalCenterAlign);
            //gfx.DrawString(number, layout.NumberBox.Font.GetFont(), new SolidBrush(layout.NumberBox.FColor.GetColor()), layout.NumberBox.Textbox.GetRectangle(), GraphicsHelper.RightVerticalCenterAlign);

            // draw copyright
            if (slide.Data.ContainsKey("copyright-text"))
            {
                string copyrighttune = (string)slide.Data.GetOrDefault("copyright-tune", "");
                string copyrighttext = (string)slide.Data.GetOrDefault("copyright-text", "");
                gfx.DrawString(copyrighttune, layout.CopyrightBox.Font.GetFont(), new SolidBrush(layout.CopyrightBox.FColor.GetColor()), new Rectangle(layout.CopyrightBox.Textbox.Origin.X, layout.CopyrightBox.Textbox.Origin.Y, layout.CopyrightBox.Textbox.Size.Width, layout.CopyrightBox.Textbox.Size.Height / 2), GraphicsHelper.LeftVerticalCenterAlign);
                //g.DrawRectangle(Pens.Orange, 0, height - 30, width, 15);
                //g.DrawRectangle(Pens.Orange, 0, height - 15, width, 15);
                gfx.DrawString(copyrighttext, layout.CopyrightBox.Font.GetFont(), new SolidBrush(layout.CopyrightBox.FColor.GetColor()), new Rectangle(layout.CopyrightBox.Textbox.Origin.X, layout.CopyrightBox.Textbox.Origin.Y + layout.CopyrightBox.Textbox.Size.Height / 2, layout.CopyrightBox.Textbox.Size.Width, layout.CopyrightBox.Textbox.Size.Height / 2), GraphicsHelper.LeftVerticalCenterAlign);
            }
            else
            {
                string copyright = (string)slide.Data.GetOrDefault("copyright", "");
                //gfx.DrawString(copyright, layout.CopyrightBox.Font.GetFont(), new SolidBrush(layout.CopyrightBox.FColor.GetColor()), layout.CopyrightBox.Textbox.GetRectangle(), GraphicsHelper.LeftVerticalCenterAlign);
                //g.DrawRectangle(Pens.Blue, 0, height - 20, width, 20);
                TextBoxRenderer.Render(gfx, kgfx, copyright, layout.CopyrightBox);
            }




            //var resized = (b: bmp, k: bmp) ;// ImageFilters.ImageFilters.UniformStretch(bmp, kbmp, new ImageFilters.UniformStretchFilterParams() { Fill = Color.White, KFill = Color.White, Height = (int)(1080 * .97), Width = (int)(1920 * .97) });

            //var bordered = ImageFilters.ImageFilters.CenterOnBackground(resized.b, resized.k, new ImageFilters.CenterOnBackgroundFilterParams() { Fill = Color.White, KFill = Color.White, Width = 1920, Height = 1080 });

            //res.Bitmap = bordered.b;
            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;
            return res;
        }

        public void VisitSlideForRendering(Slide slide, IAssetResolver assetResolver, List<XenonCompilerMessage> Messages, ref RenderedSlide result)
        {
            if (slide.Format == SlideFormat.StichedImage)
            {
                StitchedImageSlideLayoutInfo layout = (this as ISlideRenderer<StitchedImageSlideLayoutInfo>).LayoutResolver.GetLayoutInfo(slide);
                result = RenderSlide(slide, Messages, assetResolver, layout);
            }
        }

        public (Bitmap main, Bitmap key) GetPreviewForLayout(string layoutInfo)
        {
            StitchedImageSlideLayoutInfo layout = JsonSerializer.Deserialize<StitchedImageSlideLayoutInfo>(layoutInfo);

            Bitmap b = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);
            Bitmap k = new Bitmap(layout.SlideSize.Width, layout.SlideSize.Height);

            Graphics gfx = Graphics.FromImage(b);
            Graphics kgfx = Graphics.FromImage(k);

            gfx.Clear(layout.BackgroundColor.GetColor());
            kgfx.Clear(layout.KeyColor.GetColor());

            DrawingBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.MusicBox);

            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.TitleBox);
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.NameBox);
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.NumberBox);
            TextBoxRenderer.RenderLayoutPreview(gfx, kgfx, layout.CopyrightBox);

            return (b, k);
        }

        ILayoutInfoResolver<StitchedImageSlideLayoutInfo> ISlideRenderer<StitchedImageSlideLayoutInfo>.LayoutResolver { get => new StitchedImageSlideLayoutInfo(); }
    }
}
