using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using GDI = System.Drawing;

namespace Xenon.Helpers
{
    public static class ImageSharpHelpers
    {
        // https://codeblog.vurdalakov.net/2019/06/imagesharp-convert-image-to-system-drawing-bitmap-and-back.html 
        public static System.Drawing.Bitmap ToBitmap<TPixel>(this SixLabors.ImageSharp.Image<TPixel> image) where TPixel : unmanaged, SixLabors.ImageSharp.PixelFormats.IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                //var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
                var imageEncoder = image.GetConfiguration().ImageFormatsManager.GetEncoder(PngFormat.Instance);
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


        /// <summary>
        /// Returns a new Bitmap the same size as the src, but with each pixels' rbg values premultipled against the alphakey.
        /// </summary>
        /// <param name="src">Source Bitmap to premultiply</param>
        /// <param name="alphakey">The alpha channel to premultipily against. Is assumed to be the same size and grayscale.</param>
        /// <returns>The premultipled Bitmap image with an alpha channel.</returns>
        public static Image<Bgra32> PreMultiplyWithAlphaFast(this Image<Bgra32> src, Image<Bgra32> alphakey)
        {
            if (src.Width != alphakey.Width || src.Height != alphakey.Height)
            {
                return src;
            }

            // from my understanding of imagesharp API this is a prime candidate of work to parallelize
            // I think it's perfectly safe to work on seperate pixels within the same image at the same time without any need for synchronization (and that does make sense)

            // TODO: figure out how to sensibly split this workload (there's probably a limit/tradeoff on using tooooo many threads, but more than 1 is probably faster?)

            // since this is truly a brute force approach, there's no point in using more threads than the system has
            // so that's a reasonable upper bound
            // FFMPEG seems to think so

            // (of course, having already split the rendering in parallel by slide, this will effectively force a bottleneck)
            // but nothing to do about that I guess....

            var MAXTHREADS = Environment.ProcessorCount;

            // I'm guessing that the pixel buffers are aligned on the x axis, so probably best to split vertically and make use of array line caching

            int VLINES = src.Height / MAXTHREADS;
            int LASTVBLOCK = (src.Height - (VLINES * MAXTHREADS)) + VLINES; // in case it don't line up nicely, add the remainder to the last block so we don't miss any

            // split up the workload
            List<Task> blocks = new List<Task>();
            for (int p = 0; p < MAXTHREADS; p++)
            {
                int bsize = VLINES;
                if (p == MAXTHREADS - 1)
                {
                    bsize = LASTVBLOCK;
                }
                int ystart = p * VLINES;
                if (ystart + bsize < src.Height + 1)
                {
                    blocks.Add(Task.Run(() => PreMultiplyAlphaBlock(ystart, bsize, src, alphakey)));
                }
                else
                {
#if DEBUG
                    Debugger.Break();
#endif
                }
            }

            Task.WaitAll(blocks.ToArray());


            /*
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    var spix = src[x, y];

                    var apix = alphakey[x, y];

                    double alpha = (double)apix.B / 255.0d;

                    src[x, y] = new Bgra32((byte)(alpha * spix.R),
                                           (byte)(alpha * spix.G),
                                           (byte)(alpha * spix.B),
                                           apix.B);
                }
            }
            */

            return src;
        }

        static void PreMultiplyAlphaBlock(int yStart, int yBlockSize, Image<Bgra32> src, Image<Bgra32> alphakey)
        {
            for (int y = yStart; y < yStart + yBlockSize && y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    var spix = src[x, y];

                    var apix = alphakey[x, y];

                    double alpha = (double)apix.B / 255.0d;

                    src[x, y] = new Bgra32((byte)(alpha * spix.R),
                                           (byte)(alpha * spix.G),
                                           (byte)(alpha * spix.B),
                                           apix.B);
                }
            }
        }

        public static System.Windows.Media.Imaging.BitmapImage ToBitmapImage(this Image<Bgra32> bmp)
        {
            System.Windows.Media.Imaging.BitmapImage res = new System.Windows.Media.Imaging.BitmapImage();
            MemoryStream ms = new MemoryStream();
            bmp.SaveAsPng(ms);
            res.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            res.StreamSource = ms;
            res.EndInit();
            // I think this is ok, since we never want to modify this. We'd re-render through Xenon in that case
            // this should allow a UI thread in WPF to use it even though it wasn't generated on that thread
            res.Freeze();
            return res;
        }

        public static MemoryStream ToPNGStream(this Image<Bgra32> bmp)
        {
            System.Windows.Media.Imaging.BitmapImage res = new System.Windows.Media.Imaging.BitmapImage();
            MemoryStream ms = new MemoryStream();
            bmp.SaveAsPng(ms);
            return ms;
        }


    }
}
