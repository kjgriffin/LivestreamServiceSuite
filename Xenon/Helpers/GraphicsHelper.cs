using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
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
        public static Bitmap TrimBitmap(this Bitmap source, System.Drawing.Color color)
        {
            int t = source.SearchBitmapForColor(color, 1).y;
            int b = source.SearchBitmapForColor(color, 2).y;
            int l = source.SearchBitmapForColor(color, 3).x;
            int r = source.SearchBitmapForColor(color, 4).x;

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
        public static (int x, int y) SearchBitmapForColor(this Bitmap source, System.Drawing.Color col, int type)
        {
            System.Drawing.Color test = System.Drawing.Color.FromArgb(255, col.R, col.G, col.B);
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

        public static SizeF MeasureStringCharacters(this Graphics gfx, string text, Font font, RectangleF textbox)
        {
            //StringFormat format = StringFormat.GenericTypographic;
            //format.Trimming = StringTrimming.None;
            //format.FormatFlags = StringFormatFlags.NoWrap;
            //format.LineAlignment = StringAlignment.Near;

            string measuretext = text;

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

                }


                format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, ".".Length) });

                var rectofextra = RectangleF.Empty;
                try
                {
                    rectofextra = gfx.MeasureCharacterRanges(".", font, textbox, format)[0].GetBounds(gfx);
                }
                catch (Exception ex)
                {

                }

                return new SizeF(rectwithextra.Size.Width - rectofextra.Size.Width, font.Height);
            }
            else
            {
                StringFormat format = DefaultStringFormat();
                gfx = Graphics.FromHwnd((IntPtr)0);
                format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, measuretext.Length) });
                try
                {
                    return gfx.MeasureCharacterRanges(measuretext, font, textbox, format)[0].GetBounds(gfx).Size;
                }
                catch (Exception ex)
                {
                    return SizeF.Empty;
                }
            }

        }

        //public static Font LoadLSBSymbolFont()
        //{

        //}


        internal static StringFormat TopLeftAlign => new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
        internal static StringFormat CenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        internal static StringFormat LeftVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
        internal static StringFormat RightVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far };

    }
}
