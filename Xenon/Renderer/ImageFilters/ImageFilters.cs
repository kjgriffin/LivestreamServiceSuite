using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;

namespace Xenon.Renderer.ImageFilters
{
    public static class ImageFilters
    {

        /// <summary>
        /// Searches the image in the provided direction for the first pixel that matches within tolerance.
        /// </summary>
        /// <param name="source">Source Image.</param>
        /// <param name="direction">Direction to search from. 1 = Top, 2 = Bottom, 3 = Left, 4 = Right</param>
        /// <param name="match">Color to match</param>
        /// <param name="rtolerance">Red tolerance</param>
        /// <param name="gtolerance">Green tolerance</param>
        /// <param name="btolerance">Blue tolerance</param>
        /// <param name="isexclude">If True will find first pixel that does not match within tolerances.</param>
        /// <returns>The X and Y coordinates of the matched pixel</returns>
        private static (int x, int y) SearchBitmapForColorWithTolerance(this Bitmap source, int direction, Color match, int rtolerance, int gtolerance, int btolerance, bool isexclude)
        {
            if (direction == 1)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        Color pix = source.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            return (x, y);
                        }
                    }
                }
            }
            else if (direction == 2)
            {
                for (int y = source.Height - 1; y >= 0; y--)
                {
                    for (int x = source.Width - 1; x >= 0; x--)
                    {
                        Color pix = source.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            return (x, y);
                        }

                    }
                }
            }
            else if (direction == 3)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        Color pix = source.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            return (x, y);
                        }

                    }
                }
            }
            else if (direction == 4)
            {
                for (int x = source.Width - 1; x >= 0; x--)
                {
                    for (int y = source.Height - 1; y >= 0; y--)
                    {
                        Color pix = source.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            return (x, y);
                        }

                    }
                }
            }
            return (0, 0);
        }

        public static (Bitmap b, Bitmap k) Crop(Bitmap inb, Bitmap inkb, CropFilterParams fparams)
        {
            // get bounds to crop
            (int x, int y) bounds = inb.SearchBitmapForColorWithTolerance((int)fparams.Bound + 1, fparams.Identifier, fparams.RTolerance, fparams.GTolerance, fparams.BTolerance, fparams.IsExcludeMatch);

            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;
            int width = 0;
            int height = 0;

            if (fparams.Bound == CropFilterParams.CropBound.Top)
            {
                top = bounds.y;
                left = 0;
                right = inb.Width;
                bottom = inb.Height;
                width = inb.Width;
                height = inb.Height - top;
            }
            else if (fparams.Bound == CropFilterParams.CropBound.Bottom)
            {
                top = 0;
                left = 0;
                right = inb.Width;
                bottom = bounds.y;
                width = inb.Width;
                height = bottom;
            }
            else if (fparams.Bound == CropFilterParams.CropBound.Left)
            {
                top = 0;
                left = bounds.x;
                right = inb.Width;
                bottom = inb.Height;
                width = inb.Width - left;
                height = inb.Height;
            }
            else if (fparams.Bound == CropFilterParams.CropBound.Right)
            {
                top = 0;
                left = 0;
                right = bounds.x;
                bottom = inb.Height;
                width = right;
                height = inb.Height;
            }

            Bitmap _b = new Bitmap(width, height);
            Bitmap _k = new Bitmap(width, height);
            Graphics gfx = Graphics.FromImage(_b);
            Graphics kgfx = Graphics.FromImage(_k);

            gfx.DrawImage(inb, 0, 0, new Rectangle(left, top, right, bottom), GraphicsUnit.Pixel);
            kgfx.DrawImage(inkb, 0, 0, new Rectangle(left, top, right, bottom), GraphicsUnit.Pixel);

            return (_b, _k);
        }

        public static (Bitmap b, Bitmap k) ColorEdit(Bitmap inb, Bitmap inkb, ColorEditFilterParams fparams)
        {
            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Bitmap source = _b;
            if (fparams.ForKey)
            {
                source = _k;
            }

            for (int y = 0; y < _b.Height; y++)
            {
                for (int x = 0; x < _b.Width; x++)
                {
                    Color pix = source.GetPixel(x, y);
                    if (Math.Abs(pix.R - fparams.Identifier.R) <= fparams.RTolerance && Math.Abs(pix.G - fparams.Identifier.G) <= fparams.GTolerance && Math.Abs(pix.B - fparams.Identifier.B) <= fparams.BTolerance && ((Math.Abs(pix.A - fparams.Identifier.A) <= fparams.ATolerance && fparams.CheckAlpha) || !fparams.CheckAlpha))
                    {
                        if (!fparams.IsExcludeMatch)
                        {
                            source.SetPixel(x, y, fparams.Replace);
                        }
                    }
                    else if (fparams.IsExcludeMatch)
                    {
                        source.SetPixel(x, y, fparams.Replace);
                    }
                }
            }

            return (_b, _k);
        }

        public static (Bitmap b, Bitmap k) CenterOnBackground(Bitmap inb, Bitmap inkb, CenterOnBackgroundFilterParams fparams)
        {
            Bitmap _b = new Bitmap(Math.Max(fparams.Width, inb.Width), Math.Max(fparams.Height, inb.Height));
            Bitmap _k = new Bitmap(Math.Max(fparams.Width, inb.Width), Math.Max(fparams.Height, inb.Height));
            Graphics gfx = Graphics.FromImage(_b);
            Graphics kgfx = Graphics.FromImage(_k);

            int xoff = inb.Width < _b.Width ? (_b.Width - inb.Width) / 2 : 0;
            int yoff = inb.Height < _b.Height ? (_b.Height - inb.Height) / 2 : 0;

            gfx.Clear(fparams.Fill);
            kgfx.Clear(fparams.KFill);

            gfx.DrawImage(inb, xoff, yoff);
            kgfx.DrawImage(inkb, xoff, yoff);

            return (_b, _k);
        }

        public static (Bitmap b, Bitmap k) UniformStretch(Bitmap inb, Bitmap inkb, UniformStretchFilterParams fparams)
        {
            double xscale = ((double)fparams.Width / inb.Width);
            double yscale = ((double)fparams.Height / inb.Height);
            double scale = xscale < yscale ? xscale : yscale;

            Bitmap _b = new Bitmap(fparams.Width, fparams.Height);
            Bitmap _k = new Bitmap(fparams.Width, fparams.Height);
            Graphics gfx = Graphics.FromImage(_b);
            Graphics kgfx = Graphics.FromImage(_k);

            gfx.Clear(fparams.Fill);
            kgfx.Clear(fparams.KFill);

            Point p = new Point((int)((fparams.Width - (inb.Width * scale)) / 2), (int)((fparams.Height - (inb.Height * scale)) / 2));

            gfx.DrawImage(inb, new Rectangle(p, new Size((int)(inb.Width * scale), (int)(inb.Height * scale))), new Rectangle(0, 0, inb.Width, inb.Height), GraphicsUnit.Pixel);
            kgfx.DrawImage(inkb, new Rectangle(p, new Size((int)(inb.Width * scale), (int)(inb.Height * scale))), new Rectangle(0, 0, inb.Width, inb.Height), GraphicsUnit.Pixel);

            return (_b, _k);
        }

        public static (Bitmap b, Bitmap k) CenterFillAsset(Bitmap inb, Bitmap inkb, CenterAssetFillFilterParams fparams)
        {
            Bitmap sourceimage;
            try
            {
                sourceimage = new Bitmap(fparams.AssetPath);
            }
            catch (Exception)
            {
                throw new Exception($"Unable to load image <${fparams.AssetPath}> in filter");
            }

            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Graphics gfx = Graphics.FromImage(_b);

            if (sourceimage.Height > _b.Height || sourceimage.Width > _b.Width)
            {
                // overwrite with image
                // apply default white key

                _b = new Bitmap(sourceimage);
                _k = new Bitmap(sourceimage.Width, sourceimage.Height);

                Graphics kgfx = Graphics.FromImage(_k);
                kgfx.Clear(Color.White);
            }
            else
            {
                int xoff = (_b.Width - sourceimage.Width) / 2;
                int yoff = (_b.Height - sourceimage.Height) / 2;
                gfx.DrawImageUnscaled(sourceimage, xoff, yoff);
            }

            return (_b, _k);
        }

        public static (Bitmap b, Bitmap k) SolidColorCanvas(Bitmap inb, Bitmap inkb, SolidColorCanvasFilterParams fparams)
        {
            Bitmap _b = new Bitmap(fparams.Width, fparams.Height);
            Bitmap _k = new Bitmap(fparams.Width, fparams.Height);

            Graphics gfx = Graphics.FromImage(_b);
            Graphics kgfx = Graphics.FromImage(_k);

            gfx.Clear(fparams.Background);
            kgfx.Clear(fparams.KBackground);

            return (_b, _k);
        }


    }
}
