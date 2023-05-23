using System.Collections.Generic;
using System.Drawing;

using Xenon.Compiler;
using Xenon.Helpers;
using Xenon.SlideAssembly;

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

            res.Bitmap = bmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();
            res.KeyBitmap = kbmp.ToImageSharpImage<SixLabors.ImageSharp.PixelFormats.Bgra32>();

            return res;
        }

        private (Bitmap b, Bitmap kb) RenderFilter(ImageFilter filtername, ImageFilterParams ifparams, Bitmap inb, Bitmap inkb)
        {
            // select appropriate image filter
            switch (filtername)
            {
                case ImageFilter.SolidColorCanvas:
                    return ImageFilters.SolidColorCanvas(inb, inkb, ifparams as SolidColorCanvasFilterParams);
                case ImageFilter.CenterAssetFill:
                    return ImageFilters.CenterFillAsset(inb, inkb, ifparams as CenterAssetFillFilterParams);
                case ImageFilter.Crop:
                    return ImageFilters.Crop(inb, inkb, ifparams as CropFilterParams);
                case ImageFilter.UniformStretch:
                    return ImageFilters.UniformStretch(inb, inkb, ifparams as UniformStretchFilterParams);
                case ImageFilter.CenterOnBackground:
                    return ImageFilters.CenterOnBackground(inb, inkb, ifparams as CenterOnBackgroundFilterParams);
                case ImageFilter.ColorEditRGB:
                    return ImageFilters.ColorEditRGB(inb, inkb, ifparams as ColorEditFilterParams);
                case ImageFilter.ColorEditHSV:
                    return ImageFilters.ColorEditHSV(inb, inkb, ifparams as ColorEditFilterParams);
                case ImageFilter.ColorShiftHSV:
                    return ImageFilters.ColorShiftHSV(inb, inkb, ifparams as ColorShiftFilterParams);
                case ImageFilter.ColorTint:
                    return ImageFilters.ColorTint(inb, inkb, ifparams as ColorTintFilterParams);
                case ImageFilter.ColorUnTint:
                    return ImageFilters.ColorUnTint(inb, inkb, ifparams as ColorUnTintFilterParams);
            }
            return (inb, inkb);
        }

    }
}
