using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace SlideCreater.Renderer
{
    class ImageSlideRenderer
    {

        public SlideLayout Layout { get; set; }

        public RenderedSlide RenderFullImageSlide(Slide slide)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = slide.Asset;
            res.RenderedAs = "Full";

            Bitmap sourceimage;
            try
            {
                sourceimage = new Bitmap(slide.Asset);
            }
            catch (IOException)
            {
                throw new Exception("Unable to load image");
            }

            if (slide.Format == SlideFormat.UnscaledImage)
            {
                res.Bitmap = RenderUnscaled(sourceimage);
            }
            else if (slide.Format == SlideFormat.ScaledImage)
            {
                res.Bitmap = RenderUniformScale(sourceimage);
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


    }
}
