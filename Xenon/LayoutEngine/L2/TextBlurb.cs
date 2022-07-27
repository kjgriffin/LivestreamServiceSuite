using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using Xenon.Renderer.Helpers.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using GDI = System.Drawing;
using GDIT = System.Drawing.Text;
using System.Linq;

using Xenon.FontManagement;
using Xenon.Helpers;
using Xenon.LayoutInfo;
using Xenon.LayoutInfo.BaseTypes;

namespace Xenon.LayoutEngine.L2
{

    class SizedTextBlurb : TextBlurb
    {
        public GDI.SizeF Size { get; private set; }

        [Obsolete("TODO: directly call into an imagesharp based method")]
        public static SizedTextBlurb CreateMeasured(TextBlurb blurb, GDI.Graphics gfx, string defaultFont, float defaultFontSize, GDI.FontStyle defaultStyle, GDI.RectangleF rect)
        {
            SizedTextBlurb sblurb = new SizedTextBlurb(blurb);
            //sblurb.Size = sblurb.Measure(gfx, defaultFont, defaultFontSize, defaultStyle, rect);
            var s = sblurb.Measure(defaultFont, defaultFontSize, (SixLabors.Fonts.FontStyle)defaultStyle);
            sblurb.Size = new GDI.SizeF(s.Width, s.Height);
            return sblurb;
        }

        public static SizedTextBlurb CreateMeasured(TextBlurb blurb, string defaultFont, float defaultFontSize, GDI.FontStyle defaultStyle, GDI.RectangleF rect)
        {
            SizedTextBlurb sblurb = new SizedTextBlurb(blurb);
            //sblurb.Size = sblurb.Measure(gfx, defaultFont, defaultFontSize, defaultStyle, rect);
            var s = sblurb.Measure(defaultFont, defaultFontSize, (SixLabors.Fonts.FontStyle)defaultStyle);
            sblurb.Size = new GDI.SizeF(s.Width, s.Height);
            return sblurb;
        }


        public SizedTextBlurb(TextBlurb blurb) : base(blurb.Pos, blurb.Text, blurb.AltFont, blurb.FontStyle, blurb.FontSize, blurb.Space, blurb.IsWhitespace, blurb.HexFColor, blurb.ScriptType, blurb.Rules)
        {
            Size = GDI.SizeF.Empty;
        }

        public override SizedTextBlurb Clone()
        {
            return new SizedTextBlurb(base.Clone()) { Size = this.Size };
        }

    }

    internal class TextBlurb
    {

        public static float SCRIPT_FSIZE_MODIFIER = 0.6f;
        public static float SCRIPT_OFFSET_MODIFIER = 1.4f;

        public GDI.Point Pos { get; private set; }
        public string Text { get; private set; }
        public string AltFont { get; private set; }
        public int FontStyle { get; private set; }
        /// <summary>
        /// 0: regular, 1: superscript, -1: subscript
        /// </summary>
        public int ScriptType { get; private set; } = 0;
        public float FontSize { get; private set; }

        public bool IsWhitespace { get => Space || NewLine; }
        public bool Space { get; private set; }
        public bool NewLine { get; private set; }

        public string HexFColor { get; private set; }

        /// <summary>
        ///  Provides addition context for layout rules that may be used by layout engines when performing measurement/layout
        /// </summary>
        public string Rules { get; private set; } = "";

        public virtual TextBlurb Clone()
        {
            return new TextBlurb(Pos, Text, AltFont, FontStyle, FontSize, Space, NewLine, HexFColor, ScriptType, Rules);
        }


        public TextBlurb(GDI.Point pos, string text, string altfont = "", int style = -1, float size = -1, bool space = false, bool newline = false, string hexColor = null, int scripttype = 0, string rules = "")
        {
            Pos = pos;
            Text = text;
            AltFont = altfont;
            FontStyle = style;
            FontSize = size;
            Space = space;
            NewLine = newline;
            HexFColor = hexColor;
            ScriptType = scripttype;
            Rules = rules;
        }

        [Obsolete("Use imagesharp instead.")]
        internal GDI.SizeF Measure(GDI.Graphics gfx, string defaultFont, float defaultFontSize, GDI.FontStyle defaultStyle, GDI.RectangleF rect)
        {
            GDI.FontStyle style = FontStyle != -1 ? (GDI.FontStyle)FontStyle : defaultStyle;
            float fsize = FontSize > 0 ? FontSize : defaultFontSize;
            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                var installedFonts = new GDIT.InstalledFontCollection();
                if (installedFonts.Families.Any(x => x.Name == AltFont))
                {
                    fname = AltFont;
                }
            }

            GDI.Font f = new GDI.Font(fname, fsize, style);
            GDI.SizeF size = gfx.MeasureStringCharacters(Text, ref f, rect);
            f.Dispose();
            return size;
        }

        internal SizeF Measure(string defaultFont, float defaultFontSize, FontStyle defaultStyle)
        {
            FontStyle style = FontStyle != -1 ? (FontStyle)FontStyle : defaultStyle;
            float fsize = FontSize > 0 ? FontSize : defaultFontSize;

            if (ScriptType != 0)
            {
                fsize = fsize * SCRIPT_FSIZE_MODIFIER;
            }

            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                if (FontManager.HasFont(AltFont))
                {
                    fname = AltFont;
                }
            }

            TextOptions tops = new TextOptions(FontManager.GetFont(new LWJFont(fname, fsize, (int)style)))
            {
                // todo: do we need to set any more options?
                Dpi = 96,
            };

            var frect = TextMeasurer.MeasureBounds(Text, tops);
            var size = new SizeF(frect.Width, frect.Height);

            if (ScriptType != 0)
            {
                // make it a bit wider
                // NOTE: since it will be bigger this will work for any right-to-left read language since extra space to the right is where we want the margin

                size.Width = size.Width * SCRIPT_OFFSET_MODIFIER;
            }

            return size;
        }

        public void Place(GDI.Point p)
        {
            Pos = p;
        }

        [Obsolete("Use an imagesharp based render")]
        public void Render(GDI.Graphics gfx, GDI.Graphics ikbmp, GDI.Color defaultfcolor, GDI.Color defaultkcolor, string defaultFont, float defaultFontSize, GDI.FontStyle defaultStyle)
        {
            GDI.FontStyle style = FontStyle != -1 ? (GDI.FontStyle)FontStyle : defaultStyle;
            float fsize = FontSize > 0 ? FontSize : defaultFontSize;
            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                var installedFonts = new GDIT.InstalledFontCollection();
                if (installedFonts.Families.Any(x => x.Name == AltFont))
                {
                    fname = AltFont;
                }
            }

            GDI.Color fcolor = defaultfcolor;
            GDI.Color kcolor = defaultkcolor;

            if (!string.IsNullOrEmpty(HexFColor))
            {
                var fcol = new LWJColor() { Hex = HexFColor };
                fcolor = GDI.Color.FromArgb(255, fcol.Red, fcol.Green, fcol.Blue);
                kcolor = GDI.Color.FromArgb(255, fcol.Alpha, fcol.Alpha, fcol.Alpha);
            }


            using (GDI.Font f = new GDI.Font(fname, fsize, style))
            using (var gbrush = new GDI.SolidBrush(fcolor))
            using (var kbrush = new GDI.SolidBrush(kcolor))
            {
                gfx.DrawString(Text, f, gbrush, Pos);
                ikbmp.DrawString(Text, f, kbrush, Pos);
            }

        }

        public void Render(Image<Bgra32> ibmp,
                           Image<Bgra32> ikbmp,
                           Color defaultfcolor,
                           Color defaultkcolor,
                           string defaultFont,
                           float defaultFontSize,
                           FontStyle defaultStyle)
        {
            FontStyle style = FontStyle != -1 ? (FontStyle)FontStyle : defaultStyle;
            float fsize = FontSize > 0 ? FontSize : defaultFontSize;

            if (ScriptType != 0)
            {
                fsize = fsize * SCRIPT_FSIZE_MODIFIER;
            }

            string fname = defaultFont;
            if (!string.IsNullOrEmpty(AltFont))
            {
                if (FontManager.HasFont(AltFont))
                {
                    fname = AltFont;
                }
            }

            var fcolor = defaultfcolor;
            var kcolor = defaultkcolor;

            if (!string.IsNullOrEmpty(HexFColor))
            {
                var fcol = new LWJColor() { Hex = HexFColor };
                fcolor = Color.FromRgb((byte)fcol.Red, (byte)fcol.Green, (byte)fcol.Blue);
                kcolor = Color.FromRgb((byte)fcol.Alpha, (byte)fcol.Alpha, (byte)fcol.Alpha);
            }

            TextOptions topts = new TextOptions(FontManager.GetFont(new LWJFont(fname, fsize, (int)style)))
            {
                Dpi = 96,
                Origin = new PointF(Pos.X, Pos.Y),
            };

            ibmp.Mutate(ctx => ctx.DrawText(topts, Text, fcolor));
            ikbmp.Mutate(ctx => ctx.DrawText(topts, Text, kcolor));

        }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, TextboxLayout textbox)
        {
            Render(ibmp,
                   ikbmp,
                   textbox.FColor.ToColor(),
                   Color.FromRgb((byte)textbox.FColor.Alpha, (byte)textbox.FColor.Alpha, (byte)textbox.FColor.Alpha),
                   textbox.Font.Name,
                   textbox.Font.Size,
                   (FontStyle)textbox.Font.Style);
        }


    }

}
