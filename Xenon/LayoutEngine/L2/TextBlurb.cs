using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Helpers;

namespace Xenon.LayoutEngine.L2
{
    internal class TextBlurb
    {
        public Point Pos { get; set; }
        public string Text { get; set; }
        public string AltFont { get; set; }
        public FontStyle FontStyle { get; set; }
        public int FontSize { get; set; }

        public TextBlurb(Point pos, string text, string altfont = "", FontStyle style = FontStyle.Regular, int size = -1)
        {
            Pos = pos;
            Text = text;
            AltFont = altfont;
            FontStyle = style;
            FontSize = size;
        }

        public SizeF Measure(Graphics gfx, string defaultFont, int defaultFontSize, FontStyle defaultStyle, RectangleF rect)
        {
            FontStyle style = defaultStyle | FontStyle;
            int fsize = FontSize > 0 ? FontSize : defaultFontSize;
            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                var installedFonts = new InstalledFontCollection();
                if (installedFonts.Families.Any(x => x.Name == AltFont))
                {
                    fname = AltFont;
                }
            }

            Font f = new Font(fname, fsize, style);
            SizeF size = gfx.MeasureStringCharacters(Text, ref f, rect);
            f.Dispose();
            return size;
        }

        public void Render(Graphics gfx, Graphics kgfx, Color fcolor, Color kcolor, string defaultFont, int defaultFontSize, FontStyle defaultStyle)
        {
            FontStyle style = defaultStyle | FontStyle;
            int fsize = FontSize > 0 ? FontSize : defaultFontSize;
            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                var installedFonts = new InstalledFontCollection();
                if (installedFonts.Families.Any(x => x.Name == AltFont))
                {
                    fname = AltFont;
                }
            }

            using (Font f = new Font(fname, fsize, style))
            using (var gbrush = new SolidBrush(kcolor))
            using (var kbrush = new SolidBrush(kcolor))
            {
                gfx.DrawString(Text, f, gbrush, Pos);
                kgfx.DrawString(Text, f, kbrush, Pos);
            }

        }

    }

}
