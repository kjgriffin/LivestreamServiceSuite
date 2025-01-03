﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
            SpeedyBitmapManipulator sourceManipulator = new SpeedyBitmapManipulator();
            sourceManipulator.Initialize(source);

            if (direction == 1)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        //Color pix = source.GetPixel(x, y);
                        Color pix = sourceManipulator.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                sourceManipulator.Finialize();
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            sourceManipulator.Finialize();
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
                        //Color pix = source.GetPixel(x, y);
                        Color pix = sourceManipulator.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                sourceManipulator.Finialize();
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            sourceManipulator.Finialize();
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
                        //Color pix = source.GetPixel(x, y);
                        Color pix = sourceManipulator.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                sourceManipulator.Finialize();
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            sourceManipulator.Finialize();
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
                        //Color pix = source.GetPixel(x, y);
                        Color pix = sourceManipulator.GetPixel(x, y);
                        if (Math.Abs(pix.R - match.R) <= rtolerance && Math.Abs(pix.G - match.G) <= gtolerance && Math.Abs(pix.B - match.B) <= btolerance)
                        {
                            if (!isexclude)
                            {
                                sourceManipulator.Finialize();
                                return (x, y);
                            }
                        }
                        else if (isexclude)
                        {
                            sourceManipulator.Finialize();
                            return (x, y);
                        }

                    }
                }
            }
            sourceManipulator.Finialize();
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

        public static (Bitmap b, Bitmap k) ColorEditRGB(Bitmap inb, Bitmap inkb, ColorEditFilterParams fparams)
        {
            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Bitmap source = _b;
            if (fparams.ForKey)
            {
                source = _k;
            }

            SpeedyBitmapManipulator sourceManipulator = new SpeedyBitmapManipulator();
            sourceManipulator.Initialize(source);

            for (int y = 0; y < _b.Height; y++)
            {
                for (int x = 0; x < _b.Width; x++)
                {
                    //Color pix = source.GetPixel(x, y);
                    Color pix = sourceManipulator.GetPixel(x, y);
                    if (Math.Abs(pix.R - fparams.Identifier.R) <= fparams.RTolerance && Math.Abs(pix.G - fparams.Identifier.G) <= fparams.GTolerance && Math.Abs(pix.B - fparams.Identifier.B) <= fparams.BTolerance && ((Math.Abs(pix.A - fparams.Identifier.A) <= fparams.ATolerance && fparams.CheckAlpha) || !fparams.CheckAlpha))
                    {
                        if (!fparams.IsExcludeMatch)
                        {
                            //source.SetPixel(x, y, fparams.Replace);
                            sourceManipulator.SetPixel(x, y, fparams.Replace);
                        }
                    }
                    else if (fparams.IsExcludeMatch)
                    {
                        //source.SetPixel(x, y, fparams.Replace);
                        sourceManipulator.SetPixel(x, y, fparams.Replace);
                    }
                    else sourceManipulator.SetPixel(x, y, pix);
                }
            }

            var r = sourceManipulator.Finialize();

            return (fparams.ForKey ? _b : r, fparams.ForKey ? r : _k);
        }

        public static (Bitmap b, Bitmap k) ColorEditHSV(Bitmap inb, Bitmap inkb, ColorEditFilterParams fparams)
        {
            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Bitmap source = _b;
            if (fparams.ForKey)
            {
                source = _k;
            }

            SpeedyBitmapManipulator sourceManipulator = new SpeedyBitmapManipulator();
            sourceManipulator.Initialize(source);

            for (int y = 0; y < _b.Height; y++)
            {
                for (int x = 0; x < _b.Width; x++)
                {
                    //Color pix = source.GetPixel(x, y);
                    Color pix = sourceManipulator.GetPixel(x, y);
                    var h = Math.Round(pix.GetHue() / (360.0d / 240));
                    var s = Math.Round(pix.GetSaturation() * 240);
                    var b = Math.Round(pix.GetBrightness() * 240);
                    if (Math.Abs(h - fparams.Identifier.R) <= fparams.RTolerance && Math.Abs(s - fparams.Identifier.G) <= fparams.GTolerance && Math.Abs(b - fparams.Identifier.B) <= fparams.BTolerance && ((Math.Abs(pix.A - fparams.Identifier.A) <= fparams.ATolerance && fparams.CheckAlpha) || !fparams.CheckAlpha))
                    {
                        if (!fparams.IsExcludeMatch)
                        {
                            //source.SetPixel(x, y, fparams.Replace);
                            sourceManipulator.SetPixel(x, y, fparams.Replace);
                        }
                    }
                    else if (fparams.IsExcludeMatch)
                    {
                        //source.SetPixel(x, y, fparams.Replace);
                        sourceManipulator.SetPixel(x, y, fparams.Replace);
                    }
                    else sourceManipulator.SetPixel(x, y, pix);
                }
            }

            var r = sourceManipulator.Finialize();

            return (fparams.ForKey ? _b : r, fparams.ForKey ? r : _k);
        }

        public static (Bitmap b, Bitmap k) ColorTint(Bitmap inb, Bitmap inkb, ColorTintFilterParams fparams)
        {
            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Bitmap source = _b;

            SpeedyBitmapManipulator sourceManipulator = new SpeedyBitmapManipulator();
            sourceManipulator.Initialize(source);

            for (int y = 0; y < _b.Height; y++)
            {
                for (int x = 0; x < _b.Width; x++)
                {
                    Color pix = sourceManipulator.GetPixel(x, y);
                    int rr = Tint(pix.R, fparams.Red, fparams.Mix, fparams.Gammma);
                    int g = Tint(pix.G, fparams.Green, fparams.Mix, fparams.Gammma);
                    int b = Tint(pix.B, fparams.Blue, fparams.Mix, fparams.Gammma);

                    sourceManipulator.SetPixel(x, y, Color.FromArgb(255, rr, g, b));
                }
            }

            var r = sourceManipulator.Finialize();

            return (r, _k);
        }

        public static (Bitmap b, Bitmap k) ColorUnTint(Bitmap inb, Bitmap inkb, ColorUnTintFilterParams fparams)
        {
            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Bitmap source = _b;

            SpeedyBitmapManipulator sourceManipulator = new SpeedyBitmapManipulator();
            sourceManipulator.Initialize(source);

            for (int y = 0; y < _b.Height; y++)
            {
                for (int x = 0; x < _b.Width; x++)
                {
                    Color pix = sourceManipulator.GetPixel(x, y);
                    int h = (int)Math.Round(pix.GetHue() / (360.0d / 240));
                    int s = (int)Math.Round(pix.GetSaturation() * 240);
                    int bb = (int)Math.Round(pix.GetBrightness() * 240);
                    if (h >= fparams.HueMin && h <= fparams.HueMax && bb >= fparams.LumMin && bb <= fparams.LumMax)
                    {
                        int rr = UnTint(pix.R, fparams.Red, fparams.Mix, fparams.Gammma);
                        int g = UnTint(pix.G, fparams.Green, fparams.Mix, fparams.Gammma);
                        int b = UnTint(pix.B, fparams.Blue, fparams.Mix, fparams.Gammma);
                        sourceManipulator.SetPixel(x, y, Color.FromArgb(255, rr, g, b));
                    }
                    else
                    {
                        sourceManipulator.SetPixel(x, y, pix);
                    }
                }
            }

            var r = sourceManipulator.Finialize();

            return (r, _k);
        }




        public static int Tint(int s, int x, double mix, double gamma)
        {
            return (int)Math.Pow((Math.Pow(s, gamma) * (1 - mix) + Math.Pow(x, gamma) * mix), (1d / gamma));
        }

        public static int UnTint(int tval, int X, double mix, double gamma)
        {
            return Math.Clamp((int)Math.Exp(((gamma * Math.Log(tval)) - (Math.Log((1 - mix) * Math.Pow(X, gamma))) - (Math.Log(mix))) / mix), 0, 255);
            //var y = (int)Math.Exp(((gamma * Math.Log(tval)) - (Math.Log((1 - mix) * Math.Pow(X, gamma))) - (Math.Log(mix)))) % 240;
            //return y < 0 ? -1 * y : y;
        }

        public static (Bitmap b, Bitmap k) ColorShiftHSV(Bitmap inb, Bitmap inkb, ColorShiftFilterParams fparams)
        {
            Bitmap _b = new Bitmap(inb);
            Bitmap _k = new Bitmap(inkb);

            Bitmap source = _b;

            SpeedyBitmapManipulator sourceManipulator = new SpeedyBitmapManipulator();
            sourceManipulator.Initialize(source);

            for (int y = 0; y < _b.Height; y++)
            {
                for (int x = 0; x < _b.Width; x++)
                {
                    //Color pix = source.GetPixel(x, y);
                    Color pix = sourceManipulator.GetPixel(x, y);
                    int h = (int)Math.Round(pix.GetHue() / (360.0d / 240));
                    int s = (int)Math.Round(pix.GetSaturation() * 240);
                    int b = (int)Math.Round(pix.GetBrightness() * 240);

                    sourceManipulator.SetPixel(x, y, ColorFromHSV(h - fparams.Hue, s - fparams.Saturation, b - fparams.Luminance));


                }
            }

            var r = sourceManipulator.Finialize();

            return (r, _k);
        }


        public static Color ColorFromHSV(int H, int S, int V)
        {
            // scale hue from 0-360 -> 0-240
            // scale saturation 0-240 -> 0-1
            // scale value 0-240 -> 0-1
            double hue = Math.Clamp(H * 360d / 240, 0, 360);
            double saturation = Math.Clamp(S / 240d, 0, 1);
            double value = Math.Clamp(V / 240d, 0, 1);


            int hi = Convert.ToInt32(Math.Floor(hue / 60d)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60d);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
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

        /// <summary>
        /// Returns a new Bitmap the same size as the src, but with each pixels' rbg values premultipled against the alphakey.
        /// </summary>
        /// <param name="src">Source Bitmap to premultiply</param>
        /// <param name="alphakey">The alpha channel to premultipily against. Is assumed to be the same size and grayscale.</param>
        /// <returns>The premultipled Bitmap image with an alpha channel.</returns>
        public static Bitmap PreMultipleWithAlpha(this Bitmap src, Bitmap alphakey)
        {
            if (src.Size != alphakey.Size)
            {
                return src;
            }

            Bitmap b = new Bitmap(src.Width, src.Height);
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color color = src.GetPixel(x, y);
                    Color acolor = alphakey.GetPixel(x, y);
                    // assume alpha r == g == b (grayscale)
                    int alpha = acolor.R;
                    b.SetPixel(x, y, Color.FromArgb(alpha, (int)(alpha / 255.0d * (double)color.R), (int)(alpha / 255.0d * (double)color.G), (int)(alpha / 255.0d * (double)color.B)));
                }
            }

            return b;
        }

        /// <summary>
        /// Returns a new Bitmap the same size as the src, but with each pixels' rbg values premultipled against the alphakey.
        /// </summary>
        /// <param name="src">Source Bitmap to premultiply</param>
        /// <param name="alphakey">The alpha channel to premultipily against. Is assumed to be the same size and grayscale.</param>
        /// <returns>The premultipled Bitmap image with an alpha channel.</returns>
        public static Bitmap PreMultiplyWithAlphaFast(this Bitmap src, Bitmap alphakey)
        {
            if (src.Size != alphakey.Size)
            {
                return src;
            }

            Bitmap b = new Bitmap(src.Width, src.Height);

            BitmapData srcdata = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, src.PixelFormat);
            BitmapData alphadata = alphakey.LockBits(new Rectangle(0, 0, alphakey.Width, alphakey.Height), ImageLockMode.ReadOnly, alphakey.PixelFormat);
            BitmapData newdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, b.PixelFormat);

            int srcbytesperpixel = Bitmap.GetPixelFormatSize(src.PixelFormat) / 8;
            int alphadatabytesperpixel = Bitmap.GetPixelFormatSize(alphakey.PixelFormat) / 8;
            int newdatabytesperpixel = Bitmap.GetPixelFormatSize(b.PixelFormat) / 8;

            byte[] srcpixels = new byte[srcdata.Stride * src.Height];
            byte[] alphapixels = new byte[alphadata.Stride * alphakey.Height];
            byte[] newpixels = new byte[newdata.Stride * b.Height];

            IntPtr ptrFirstSrcPixel = srcdata.Scan0;
            IntPtr ptrFirstAlphaPixel = alphadata.Scan0;
            IntPtr ptrFirstNewPixel = newdata.Scan0;

            Marshal.Copy(ptrFirstSrcPixel, srcpixels, 0, srcpixels.Length);
            Marshal.Copy(ptrFirstAlphaPixel, alphapixels, 0, alphapixels.Length);

            int srcHeightInPixels = srcdata.Height;
            int srcWidthInPixels = srcdata.Width * srcbytesperpixel;
            int alphaHeightInPixels = alphadata.Height;
            int alphaWidthInPixels = alphadata.Width * alphadatabytesperpixel;
            int newHeightInPixels = newdata.Height;
            int newWidthInPixels = newdata.Width * newdatabytesperpixel;

            for (int y = 0; y < srcHeightInPixels; y++)
            {
                int currentLine = y * srcdata.Stride;
                for (int x = 0; x < srcWidthInPixels; x = x + srcbytesperpixel)
                {
                    int srcBlue = srcpixels[currentLine + x];
                    int srcGreen = srcpixels[currentLine + x + 1];
                    int srcRed = srcpixels[currentLine + x + 2];

                    int alphaBlue = alphapixels[currentLine + x];
                    int alphaGreen = alphapixels[currentLine + x + 1];
                    int alphaRed = alphapixels[currentLine + x + 2];

                    double alpha = (double)alphaBlue / 255.0d;
                    newpixels[currentLine + x] = (byte)(alpha * srcBlue);
                    newpixels[currentLine + x + 1] = (byte)(alpha * srcGreen);
                    newpixels[currentLine + x + 2] = (byte)(alpha * srcRed);
                    newpixels[currentLine + x + 3] = (byte)(alphaBlue);
                }
            }

            Marshal.Copy(newpixels, 0, ptrFirstNewPixel, newpixels.Length);

            src.UnlockBits(srcdata);
            alphakey.UnlockBits(alphadata);
            b.UnlockBits(newdata);

            return b;
        }


    }
}
