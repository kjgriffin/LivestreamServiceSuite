using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Compiler;
using Xenon.SlideAssembly;
using Xenon.Helpers;

namespace Xenon.Renderer.ImageFilters
{
    class ImageFilterRenderer
    {
        public RenderedSlide RenderImageSlide(Slide slide, List<XenonCompilerMessage> messages)
        {
            RenderedSlide res = new RenderedSlide();
            res.MediaType = MediaType.Image;
            res.AssetPath = "";
            // TODO: add filter to change rendertype
            res.RenderedAs = "Full";

            Bitmap bmp = new Bitmap(1920, 1080);
            Bitmap kbmp = new Bitmap(1920, 1080);
            Graphics gfx = Graphics.FromImage(bmp);
            Graphics kgfx = Graphics.FromImage(kbmp);

            gfx.Clear(Color.White);
            kgfx.Clear(Color.White);

            // extract filters from slide
            List<(ImageFilter fname, ImageFilterParams fparams)> fchain = (List<(ImageFilter, ImageFilterParams)>)slide.Data["filter-chain"];

            // run each filter in sequence
            Bitmap lastb = new Bitmap(bmp);
            Bitmap lastkb = new Bitmap(kbmp);
            foreach (var filter in fchain)
            {
                (Bitmap b, Bitmap kb) result = RenderFilter(filter.fname, filter.fparams, lastb, lastkb);
                lastb = new Bitmap(result.b);
                lastkb = new Bitmap(result.kb);
            }

            bmp = lastb;
            kbmp = lastkb;

            res.Bitmap = bmp;
            res.KeyBitmap = kbmp;

            return res;
        }

        private (Bitmap b, Bitmap kb) RenderFilter(ImageFilter filtername, ImageFilterParams ifparams, Bitmap inb, Bitmap inkb)
        {
            // select appropriate image filter
            switch (filtername)
            {
                case ImageFilter.SolidColorCanvas:
                    return ImageFilters.SolidColorCanvas(inb, inkb, ifparams as SolidColorCanvasFilterParams);
                case ImageFilter.Crop:
                    break;
                case ImageFilter.Resize:
                    break;
            }
            return (inb, inkb);
        }

    }
}
