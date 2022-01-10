using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Xenon.Helpers
{
    public static class GraphicsHelper
    {

        /// <summary>
        /// Returns a new bitmap that is trimmed in size to exclude borders of only the color
        /// </summary>
        /// <param name="source">The source bitmap</param>
        /// <param name="color">Color of border to ignore</param>
        /// <returns>Trimmed bitmap</returns>
        public static Bitmap TrimBitmap(this Bitmap source, System.Drawing.Color color, bool omitalpha = true)
        {
            int t = source.SearchBitmapForColor(color, 1, omitalpha).y;
            int b = source.SearchBitmapForColor(color, 2, omitalpha).y;
            int l = source.SearchBitmapForColor(color, 3, omitalpha).x;
            int r = source.SearchBitmapForColor(color, 4, omitalpha).x;

            Rectangle rect = new Rectangle(l, t, r - l, b - t);

            Bitmap bmp = new Bitmap(rect.Size.Width, rect.Size.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);

            return bmp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="col"></param>
        /// <param name="type"> 1: top, 2: bottom, 3: left, 4: right</param>
        /// <returns></returns>
        public static (int x, int y) SearchBitmapForColor(this Bitmap source, System.Drawing.Color col, int type, bool omitalpha = true)
        {
            System.Drawing.Color test = col;
            if (omitalpha)
            {
                test = System.Drawing.Color.FromArgb(255, col.R, col.G, col.B);
            }
            if (type == 1)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        System.Drawing.Color pix = source.GetPixel(x, y);
                        if (pix != test)
                        {
                            return (x, y);
                        }
                    }
                }
            }
            else if (type == 2)
            {
                for (int y = source.Height - 1; y >= 0; y--)
                {
                    for (int x = source.Width - 1; x >= 0; x--)
                    {
                        System.Drawing.Color pix = source.GetPixel(x, y);
                        if (pix != test)
                        {
                            return (x, y);
                        }
                    }
                }
            }
            else if (type == 3)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    for (int y = 0; y < source.Height; y++)
                    {
                        System.Drawing.Color pix = source.GetPixel(x, y);
                        if (pix != test)
                        {
                            return (x, y);
                        }
                    }
                }
            }
            else if (type == 4)
            {
                for (int x = source.Width - 1; x >= 0; x--)
                {
                    for (int y = source.Height - 1; y >= 0; y--)
                    {
                        System.Drawing.Color pix = source.GetPixel(x, y);
                        if (pix != test)
                        {
                            return (x, y);
                        }
                    }
                }
            }

            return (0, 0);
        }


        public static System.Drawing.Color ColorFromRGB(string col)
        {
            var colmatch = Regex.Match(col, @"(?<Red>\d{1,3}),(?<Green>\d{1,3}),(?<Blue>\d{1,3})").Groups;
            int alpha = 255;
            int red = Convert.ToInt32(colmatch["Red"].Value);
            int green = Convert.ToInt32(colmatch["Green"].Value);
            int blue = Convert.ToInt32(colmatch["Blue"].Value);
            return System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }



        public static BitmapImage ConvertToBitmapImage(this Bitmap bmp)
        {
            BitmapImage res = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            res.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            res.StreamSource = ms;
            res.EndInit();
            return res;
        }

        public static Bitmap ConvertToBitmap(this BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }


        public static StringFormat DefaultStringFormat()
        {
            StringFormat format = StringFormat.GenericTypographic;
            format.Trimming = StringTrimming.None;
            format.FormatFlags = StringFormatFlags.NoWrap;
            format.LineAlignment = StringAlignment.Near;
            return format;
        }

        public static SizeF MeasureStringCharacters(this Graphics gfx, string text, ref Font _font, RectangleF textbox)
        {
            //StringFormat format = StringFormat.GenericTypographic;
            //format.Trimming = StringTrimming.None;
            //format.FormatFlags = StringFormatFlags.NoWrap;
            //format.LineAlignment = StringAlignment.Near;
            using (Font font = new Font(_font.FontFamily, _font.Size, _font.Style))
            {

                string measuretext = text;

                SizeF res;

                if (Regex.Match(text, "\\s").Success)
                {
                    StringFormat format = DefaultStringFormat();
                    measuretext = $"{text}.";
                    gfx = Graphics.FromHwnd((IntPtr)0);
                    format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, measuretext.Length) });
                    var rectwithextra = RectangleF.Empty;
                    try
                    {
                        rectwithextra = gfx.MeasureCharacterRanges(measuretext, font, textbox, format)[0].GetBounds(gfx);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error Measuring String '{text}'");
                        Debug.WriteLine(ex);
                        Debug.WriteLine(Environment.StackTrace);
                    }


                    format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, ".".Length) });

                    var rectofextra = RectangleF.Empty;
                    try
                    {
                        rectofextra = gfx.MeasureCharacterRanges(".", font, textbox, format)[0].GetBounds(gfx);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error Measuring String '{text}'");
                        Debug.WriteLine(ex);
                        Debug.WriteLine(Environment.StackTrace);
                    }

                    res = new SizeF(rectwithextra.Size.Width - rectofextra.Size.Width, font.Height);
                }
                else
                {
                    StringFormat format = DefaultStringFormat();
                    gfx = Graphics.FromHwnd((IntPtr)0);
                    format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, measuretext.Length) });
                    try
                    {
                        res = gfx.MeasureCharacterRanges(measuretext, font, textbox, format)[0].GetBounds(gfx).Size;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error Measuring String '{text}'");
                        Debug.WriteLine(ex);
                        Debug.WriteLine(Environment.StackTrace);
                        res = SizeF.Empty;
                    }
                }
                Debug.WriteLine($"Measured '{text}', {res}");
                return res;
            }
        }


        public static Bitmap InvertImageWithMatrix(this Bitmap source)
        {
            Bitmap dest = new Bitmap(source.Width, source.Height);

            ColorMatrix m = new ColorMatrix(new float[][]
            {
                new float[] {-1, 0, 0, 0, 0 },
                new float[] {0, -1, 0, 0, 0 },
                new float[] {0, 0, -1, 0, 0 },
                new float[] {0, 0, 0, 1, 0 },
                new float[] {1, 1, 1, 0, 1 },
            });

            using (ImageAttributes a = new ImageAttributes())
            {
                a.SetColorMatrix(m);
                using (Graphics g = Graphics.FromImage(dest))
                {
                    g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, a);
                }
            }
            return dest;
        }

        public static Point ToPoint(this PointF point)
        {
            return new Point((int)point.X, (int)point.Y);
        }

        public enum AlphaMode
        {
            Preserve,
            Invert,
            Remove,
        }

        public static Bitmap InvertImage(this Bitmap source, AlphaMode alphaMode = AlphaMode.Remove)
        {
            /*
                https://stackoverflow.com/questions/33024881/invert-image-faster-in-c-sharp
             */
            Bitmap res = new Bitmap(source);

            SpeedyBitmapManipulator resManipulator = new SpeedyBitmapManipulator();
            resManipulator.Initialize(res);

            for (int y = 0; y < res.Height; y++)
            {
                for (int x = 0; x < res.Width; x++)
                {
                    //Color inv = res.GetPixel(x, y);
                    Color inv = resManipulator.GetPixel(x, y);
                    int alpha = alphaMode == AlphaMode.Remove ? 255 : alphaMode == AlphaMode.Invert ? 255 - inv.A : inv.A;
                    inv = Color.FromArgb(alpha, 255 - inv.R, 255 - inv.G, 255 - inv.B);
                    //res.SetPixel(x, y, inv);
                    resManipulator.SetPixel(x, y, inv);
                }
            }
            res = resManipulator.Finialize();
            return res;
        }

        public static Bitmap ConvertTransparencyToGreyscale(this Bitmap source)
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
                    if (inv.R != 0 && inv.G != 0 && inv.B != 0 && inv.A != 255)
                    {
                        res.SetPixel(x, y, Color.FromArgb(255, 255 - inv.A, 255 - inv.A, 255 - inv.A));
                    }
                }
            }
            return res;
        }

        public static Bitmap CorrectImage(this Bitmap source, int cfactor, Color ccol)
        {
            Bitmap res = new Bitmap(source);

            for (int y = 0; y < res.Height; y++)
            {
                for (int x = 0; x < res.Width; x++)
                {
                    Color inv = res.GetPixel(x, y);
                    if (Math.Abs(ccol.R - inv.R) < cfactor || Math.Abs(ccol.G - inv.G) < cfactor || Math.Abs(ccol.B - inv.B) < cfactor)
                    {
                        res.SetPixel(x, y, ccol);
                    }
                }
            }
            return res;

        }

        public static Bitmap ForceMonoChromeImage(this Bitmap source, Color except, Color force)
        {
            Bitmap res = new Bitmap(source);

            for (int y = 0; y < res.Height; y++)
            {
                for (int x = 0; x < res.Width; x++)
                {
                    Color inv = res.GetPixel(x, y);
                    if (inv.R != except.R || inv.G != except.G || inv.B != except.B)
                    {
                        res.SetPixel(x, y, force);
                    }
                }
            }
            return res;

        }


        /// <summary>
        /// Process an image and resolve it to contain only two colors.
        /// </summary>
        /// <param name="source">Source image</param>
        /// <param name="match">Color in original image to keep.</param>
        /// <param name="tolerance">RGB tolerance to match color to keep.</param>
        /// <param name="force">Color to replace pixels that don't match.</param>
        /// <returns></returns>
        public static Bitmap DichotimizeImage(this Bitmap source, Color match, int tolerance, Color force)
        {
            Bitmap res = new Bitmap(source);

            for (int y = 0; y < res.Height; y++)
            {
                for (int x = 0; x < res.Width; x++)
                {
                    Color inv = res.GetPixel(x, y);
                    if (Math.Abs(match.R - inv.R) < tolerance || Math.Abs(match.G - inv.G) < tolerance || Math.Abs(match.B - inv.B) < tolerance)
                    {
                        res.SetPixel(x, y, match);
                    }
                    else
                    {
                        res.SetPixel(x, y, force);
                    }
                }
            }
            return res;

        }


        public static Bitmap SwapImageColors(this Bitmap source, Color scol, Color dcol)
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
                    if (inv.R == scol.R && inv.G == scol.G && inv.B == scol.B)
                    {
                        res.SetPixel(x, y, dcol);
                    }
                }
            }
            return res;
        }


        //public static Font LoadLSBSymbolFont()
        //{

        //}

        public static Bitmap Rescale(this Bitmap source, double scale)
        {
            int width = (int)(source.Width * scale);
            int height = (int)(source.Height * scale);
            Bitmap newimage = new Bitmap(width, height, source.PixelFormat);
            using (Graphics g = Graphics.FromImage(newimage))
            {
                g.DrawImage(source, new Rectangle(0, 0, width, height), new RectangleF(0, 0, source.Width, source.Height), GraphicsUnit.Pixel);
                return newimage;
            }
        }


        internal static StringFormat TopLeftAlign => new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
        internal static StringFormat CenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        internal static StringFormat LeftVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
        internal static StringFormat RightVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far };

    }
}
