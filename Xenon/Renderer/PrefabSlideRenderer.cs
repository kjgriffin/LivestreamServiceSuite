using Xenon.Compiler;
using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Media.Imaging;
using Xenon.Helpers;
using System.Linq.Expressions;

namespace Xenon.Renderer
{
    class PrefabSlideRenderer
    {
        public SlideLayout Layouts { get; set; }

        public RenderedSlide RenderSlide(Slide slide, List<XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.AssetPath = "";
            res.MediaType = MediaType.Image;
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(Layouts.PrefabLayout.Size.Width, Layouts.PrefabLayout.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.Clear(Color.White);


            Bitmap src = null;

            try
            {

                switch ((PrefabSlides)slide.Data["prefabtype"])
                {
                    case PrefabSlides.Copyright:
                        //src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/copyright.PNG")).ConvertToBitmap();
                        src = ImageResources.PrefabSlides.copyright_png;
                        break;
                    case PrefabSlides.ViewServices:
                        //src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/services.PNG")).ConvertToBitmap();
                        src = ImageResources.PrefabSlides.viewservices_png;
                        break;
                    case PrefabSlides.ViewSeries:
                        //src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/series.PNG")).ConvertToBitmap();
                        throw new MissingFieldException();
                        break;
                    case PrefabSlides.ApostlesCreed:
                        //src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/apostlescreed.PNG")).ConvertToBitmap();
                        src = ImageResources.PrefabSlides.apostlescreed_png;
                        break;
                    case PrefabSlides.LordsPrayer:
                        //src = new BitmapImage(new Uri("pack://application:,,,/ImageResources/lordsprayer.PNG")).ConvertToBitmap();
                        src = ImageResources.PrefabSlides.lordsprayer_png;
                        break;
                }

            }
            catch (Exception ex)
            {
                res.Bitmap = bmp;
                string tmp = "KEY ERROR on data attribute";
                object a;
                slide.Data.TryGetValue("prefabtype", out a);
                if (a is PrefabSlides && a != null)
                {
                    tmp = ((PrefabSlides)a).Convert();
                }
                messages.Add(new XenonCompilerMessage() { Level = XenonCompilerMessageType.Error, ErrorMessage = $"Requested prefab image not loaded. While rendering slide {slide.Number}", ErrorName = "Prefab not found", Token = tmp });
                throw ex;
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
