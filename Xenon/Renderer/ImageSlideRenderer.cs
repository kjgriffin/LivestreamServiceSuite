using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media;
using Xenon.Helpers;

namespace Xenon.Renderer
{
    class ImageSlideRenderer
    {

        public SlideLayout Layout { get; set; }

        public RenderedSlide RenderImageSlide(Slide slide, List<Compiler.XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = slide.Asset;

            Bitmap sourceimage;
            try
            {
                sourceimage = new Bitmap(slide.Asset);
            }
            catch (Exception)
            {
                throw new Exception($"Unable to load image <{slide.Asset}> for slide {slide}");
            }

            if (slide.Format == SlideFormat.UnscaledImage)
            {
                res.Bitmap = RenderUnscaled(sourceimage);
                res.RenderedAs = "Full";
            }
            else if (slide.Format == SlideFormat.ScaledImage)
            {
                res.Bitmap = RenderUniformScale(sourceimage);
                res.RenderedAs = "Full";
            }
            else if (slide.Format == SlideFormat.LiturgyImage)
            {
                res.Bitmap = RenderLiturgyImage(sourceimage);
                res.RenderedAs = "Liturgy";
            }

            return res;

        }

        private Bitmap RenderUnscaled(Bitmap sourceimage)
        {
            Bitmap bmp = new Bitmap(Layout.FullImageLayout.Size.Width, Layout.FullImageLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.DrawImage(sourceimage, 0, 0, Layout.FullImageLayout.Size.Width, Layout.FullImageLayout.Size.Height);
            return bmp;
        }

        private Bitmap RenderUniformScale(Bitmap sourceimage)
        {
            Bitmap bmp = new Bitmap(Layout.FullImageLayout.Size.Width, Layout.FullImageLayout.Size.Height);

            Graphics gfx = Graphics.FromImage(bmp);

            double scale = 1;
            Point p;
            Size s;

            double xscale = 1;
            double yscale = 1;

            xscale = (double)Layout.FullImageLayout.Size.Width / sourceimage.Width;
            yscale = (double)Layout.FullImageLayout.Size.Height / sourceimage.Height;

            scale = xscale < yscale ? xscale : yscale;

            s = new Size((int)(sourceimage.Width * scale), (int)(sourceimage.Height * scale));
            p = new Point((Layout.FullImageLayout.Size.Width - s.Width) / 2, (Layout.FullImageLayout.Size.Height - s.Height) / 2);

            gfx.Clear(System.Drawing.Color.White);
            gfx.DrawImage(sourceimage, new Rectangle(p, s), new Rectangle(new Point(0, 0), sourceimage.Size), GraphicsUnit.Pixel);
            return bmp;

        }

        private Bitmap RenderLiturgyImage(Bitmap sourceimage)
        {
            Bitmap bmp = new Bitmap(Layout.LiturgyLayout.Size.Width, Layout.LiturgyLayout.Size.Height);

            Graphics gfx = Graphics.FromImage(bmp);

            double scale = 1;
            Point p;
            Size s;

            double xscale = 1;
            double yscale = 1;

            xscale = (double)Layout.LiturgyLayout.Key.Width / sourceimage.Width;
            yscale = (double)Layout.LiturgyLayout.Key.Height / sourceimage.Height;

            scale = xscale < yscale ? xscale : yscale;

            s = new Size((int)(sourceimage.Width * scale), (int)(sourceimage.Height * scale));
            p = new Point((Layout.LiturgyLayout.Key.Width - s.Width) / 2, (Layout.LiturgyLayout.Key.Height - s.Height) / 2).Add(Layout.LiturgyLayout.Key.Location);

            gfx.Clear(System.Drawing.Color.Black);
            gfx.FillRectangle(System.Drawing.Brushes.White, Layout.LiturgyLayout.Key);
            gfx.DrawImage(sourceimage, new Rectangle(p, s), new Rectangle(new Point(0, 0), sourceimage.Size), GraphicsUnit.Pixel);
            return bmp;

        }


    }
}
