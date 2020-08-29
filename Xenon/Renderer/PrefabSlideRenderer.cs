using Xenon.Compiler;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Media.Imaging;
using Xenon.Helpers;

namespace Xenon.Renderer
{
    class PrefabSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide)
        {
            RenderedSlide res = new RenderedSlide();
            res.AssetPath = "";
            res.MediaType = MediaType.Image;
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(Layouts.PrefabLayout.Size.Width, Layouts.PrefabLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.White);


            Bitmap src = null;

            switch ((PrefabSlides)slide.Data["prefabtype"])
            {
                case PrefabSlides.Copyright:
                    src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/copyright.PNG")).ConvertToBitmap();
                    break;
                case PrefabSlides.ViewServices:
                    src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/services.PNG")).ConvertToBitmap();
                    break;
                case PrefabSlides.ViewSeries:
                    src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/series.PNG")).ConvertToBitmap();
                    break;
                case PrefabSlides.ApostlesCreed:
                    src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/apostlescreed.PNG")).ConvertToBitmap();
                    break;
                case PrefabSlides.LordsPrayer:
                    src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/lordsprayer.PNG")).ConvertToBitmap();
                    break;
            }



            if (src != null)
            {
                gfx.DrawImage(src, new Rectangle(new Point(0, 0), Layouts.PrefabLayout.Size));
            }

            res.Bitmap = bmp;
            return res;
        }
    }
}
