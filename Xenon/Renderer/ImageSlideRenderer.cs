using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
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
            else if (slide.Format == SlideFormat.AutoscaledImage)
            {
                bool invert = false;
                if (slide.Data.ContainsKey("invert-color"))
                {
                    invert = (bool)slide.Data["invert-color"];
                }
                res.Bitmap = RenderAutoScaled(sourceimage, invert);
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

        private Bitmap RenderAutoScaled(Bitmap sourceimage, bool invertblackandwhite)
        {
            /* inspect the image (assumes white background)
                find edge most (top, left, right, bottom) non-white pixels to determine effective image size
                crop this image out
                apply border to fit into action safe region: https://eks.tv/title-safe-still-matters/#:~:text=These%20are%20areas%20on%20the,and%2090%25%20for%20action%20safe.&text=Unless%20you%20don't%20care,won't%20see%20your%20text.
                will use 93% space
            */
            int topbound = 0;
            int bottombound = sourceimage.Height - 1;
            int leftbound = 0;
            int rightbound = sourceimage.Width - 1;

            topbound = GraphicsHelper.SearchBitmapForColor(sourceimage, Color.White, 1).y;
            bottombound = GraphicsHelper.SearchBitmapForColor(sourceimage, Color.White, 2).y;
            leftbound = GraphicsHelper.SearchBitmapForColor(sourceimage, Color.White, 3).x;
            rightbound = GraphicsHelper.SearchBitmapForColor(sourceimage, Color.White, 4).x;


            Bitmap bmp = new Bitmap(Layout.FullImageLayout.Size.Width, Layout.FullImageLayout.Size.Height);

            double scale = 1;
            Point p;
            Size s;

            double xscale = 1;
            double yscale = 1;

            double fillsize = 0.93;

            xscale = ((double)Layout.FullImageLayout.Size.Width * fillsize) / (rightbound - leftbound);
            yscale = ((double)Layout.FullImageLayout.Size.Height * fillsize) / (bottombound - topbound);

            scale = xscale < yscale ? xscale : yscale;

            s = new Size((int)((rightbound - leftbound) * scale), (int)((bottombound - topbound) * scale));
            p = new Point((Layout.FullImageLayout.Size.Width - s.Width) / 2, (Layout.FullImageLayout.Size.Height - s.Height) / 2);

            Graphics gfx = Graphics.FromImage(bmp);
            if (invertblackandwhite)
            {
                gfx.Clear(Color.Black);
            }
            else
            {
                gfx.Clear(Color.White);
            }
            Bitmap img = sourceimage;
            if (invertblackandwhite)
            {
                img = InvertImage(sourceimage);
            }
            gfx.DrawImage(img, new Rectangle(p, s), leftbound, topbound, (rightbound - leftbound), (bottombound - topbound), GraphicsUnit.Pixel);

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

            double fillsize = 0.93;

            Bitmap trimmed = sourceimage.TrimBitmap(Color.White);

            xscale = (double)(Layout.LiturgyLayout.Key.Width * fillsize) / trimmed.Width;
            yscale = (double)(Layout.LiturgyLayout.Key.Height * fillsize) / trimmed.Height;

            scale = xscale < yscale ? xscale : yscale;

            s = new Size((int)(trimmed.Width * scale), (int)(trimmed.Height * scale));
            p = new Point((Layout.LiturgyLayout.Key.Width - s.Width) / 2, (Layout.LiturgyLayout.Key.Height - s.Height) / 2).Add(Layout.LiturgyLayout.Key.Location);

            gfx.Clear(System.Drawing.Color.Gray);
            gfx.FillRectangle(System.Drawing.Brushes.Black, Layout.LiturgyLayout.Key);
            gfx.DrawImage(InvertImage(trimmed), new Rectangle(p, s), new Rectangle(new Point(0, 0), trimmed.Size), GraphicsUnit.Pixel);
            return bmp;

        }

        private Bitmap InvertImage(Bitmap source)
        {
            /*
                https://stackoverflow.com/questions/33024881/invert-image-faster-in-c-sharp
             */
            Bitmap res = new Bitmap(source);

            for (int y = 0; y < res.Height; y++)
            {
                for (int x = 0; x < res.Width; x++)
                {
                    Color inv = res.GetPixel(x, y);
                    inv = Color.FromArgb(255, 255 - inv.R, 255 - inv.G, 255 - inv.B);
                    res.SetPixel(x, y, inv);
                }
            }
            return res;
        }


    }
}
