using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDI = System.Drawing;

namespace Xenon.Helpers
{
    internal static class ImageSharpHelpers
    {
        // https://codeblog.vurdalakov.net/2019/06/imagesharp-convert-image-to-system-drawing-bitmap-and-back.html 
        public static System.Drawing.Bitmap ToBitmap<TPixel>(this SixLabors.ImageSharp.Image<TPixel> image) where TPixel : unmanaged, SixLabors.ImageSharp.PixelFormats.IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
                image.Save(memoryStream, imageEncoder);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new GDI.Bitmap(memoryStream);
            }
        }

        // https://codeblog.vurdalakov.net/2019/06/imagesharp-convert-image-to-system-drawing-bitmap-and-back.html 
        public static SixLabors.ImageSharp.Image<TPixel> ToImageSharpImage<TPixel>(this GDI.Bitmap bmp) where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                bmp.Save(memoryStream, GDI.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return SixLabors.ImageSharp.Image.Load<TPixel>(memoryStream);
            }
        }



        public static Image<Bgra32> Crop(this Image<Bgra32> src, byte R, byte G, byte B, byte A, bool ignoreA = true)
        {

            // inspect the image for each bound
            // we'll take the 'largest' subset of the image that contains something not the provided color
            int top = src.Height;
            int bottom = 0;
            int left = src.Width;
            int right = 0;

            // think we've got it all done in one pass
            for (int x = 0; x < src.Width; x++)
            {
                for (int y = 0; y < src.Height; y++)
                {
                    Bgra32 pix = src[x, y];
                    if (pix.R == R && pix.G == G && pix.B == B && (pix.A == A).OptionalTrue(ignoreA))
                    {
                        if (y < top)
                        {
                            top = y;
                        }
                        if (x < left)
                        {
                            left = x;
                        }
                        if (y > bottom)
                        {
                            bottom = y;
                        }
                        if (x > right)
                        {
                            right = x;
                        }
                    }
                }
            }

            // crop to bounds
            src.Mutate(ctx => ctx.Crop(new Rectangle(left, top, Math.Max(right - left, 0), Math.Max(bottom - top, 0))));
            return src;
        }




        public static void SetupGDIGraphics(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, out GDI.Graphics gfx, out GDI.Graphics kgfx, out GDI.Bitmap bmp, out GDI.Bitmap kbmp)
        {
            bmp = ibmp.ToBitmap();
            kbmp = ikbmp.ToBitmap();
            gfx = GDI.Graphics.FromImage(bmp);
            kgfx = GDI.Graphics.FromImage(kbmp);
        }

    }
}
