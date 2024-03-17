using SixLabors.ImageSharp.Drawing.Processing;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json.Serialization;

using IBrush = SixLabors.ImageSharp.Drawing.Processing.Brush;

namespace Xenon.LayoutInfo
{
    class LWJPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        [JsonIgnore]
        public SixLabors.ImageSharp.PointF PointF { get => new SixLabors.ImageSharp.PointF(X, Y); }
        [JsonIgnore]
        public SixLabors.ImageSharp.Point Point { get => new SixLabors.ImageSharp.Point(X, Y); }

        public LWJPoint() { }

        public LWJPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
        public LWJPoint(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
        public Point GetPoint()
        {
            return new Point(X, Y);
        }

        public PointF GetPointF()
        {
            return new PointF(X, Y);
        }
    }

    class LWJSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
        [JsonIgnore]
        public SixLabors.ImageSharp.SizeF SizeF { get => new SixLabors.ImageSharp.SizeF(Width, Height); }
        [JsonIgnore]
        public SixLabors.ImageSharp.Size Size { get => new SixLabors.ImageSharp.Size(Width, Height); }

        public LWJSize() { }
        public LWJSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }
        public Size GetSize()
        {
            return new Size(Width, Height);
        }
    }

    class LWJRect
    {
        public LWJPoint Origin { get; set; }
        public LWJSize Size { get; set; }
        [JsonIgnore]
        public SixLabors.ImageSharp.RectangleF RectangleF { get => new SixLabors.ImageSharp.RectangleF(Origin.PointF, Size.SizeF); }
        [JsonIgnore]
        public SixLabors.ImageSharp.Rectangle Rectangle { get => new SixLabors.ImageSharp.Rectangle(Origin.Point, Size.Size); }

        public LWJRect() { }
        public LWJRect(Rectangle rect)
        {
            Origin = new LWJPoint(rect.Location);
            Size = new LWJSize(rect.Size);
        }
        public Rectangle GetRectangle()
        {
            return new Rectangle(Origin.GetPoint(), Size.GetSize());
        }

        internal RectangleF GetRectangleF()
        {
            return new RectangleF(Origin.GetPoint(), Size.GetSize());
        }
    }

    class LWJPolygon
    {
        public LWJTransformSet Transforms { get; set; } = new LWJTransformSet();
        public List<LWJPoint> Verticies { get; set; } = new List<LWJPoint>();
        public int BorderWidth { get; set; }
        public LWJColor FillColor { get; set; }
        public LWJColor KeyFillColor { get; set; }

        public LWJFillBrush FillBrush { get; set; }
        public LWJFillBrush KeyFillBrush { get; set; }

        public LWJColor BorderColor { get; set; }
        public LWJColor KeyBorderColor { get; set; }

        public IBrush GetFillBrush()
        {
            if (FillBrush != null)
            {
                return FillBrush.GetBrush();
            }
            var col = FillColor?.ToColor() ?? SixLabors.ImageSharp.Color.Black;
            return new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(col);
        }

        public IBrush GetKeyFillBrush()
        {
            if (KeyFillBrush != null)
            {
                return KeyFillBrush.GetBrush();
            }
            var col = KeyFillColor?.ToColor() ?? SixLabors.ImageSharp.Color.Black;
            return new SixLabors.ImageSharp.Drawing.Processing.SolidBrush(col);
        }


    }

    class LWJFillBrush
    {
        public string Mode { get; set; } = "linear";
        public LWJColor Stop1 { get; set; } = new LWJColor();
        public LWJColor Stop2 { get; set; } = new LWJColor();
        public LWJPoint Point1 { get; set; } = new LWJPoint();
        public LWJPoint Point2 { get; set; } = new LWJPoint();

        public IBrush GetBrush()
        {
            return new SixLabors.ImageSharp.Drawing.Processing.LinearGradientBrush(Point1.PointF, Point2.PointF, GradientRepetitionMode.None, new ColorStop[] { new ColorStop(0, Stop1.ToColor()), new ColorStop(1, Stop2.ToColor()) });
        }
    }


    class LWJTransformSet
    {
        public LWJScaleTransform Scale { get; set; } = new LWJScaleTransform();
        public LWJTranslateTransform Translate { get; set; } = new LWJTranslateTransform();
    }

    abstract class LWJTransform
    {
        public abstract PointF[] Apply(PointF[] points);
        public abstract SixLabors.ImageSharp.PointF[] Apply(SixLabors.ImageSharp.PointF[] points);
    }

    class LWJScaleTransform : LWJTransform
    {
        public double XScale { get; set; } = 1;
        public double YScale { get; set; } = 1;

        public override PointF[] Apply(PointF[] points)
        {
            return points.Select(p => new PointF((float)(p.X * XScale), (float)(p.Y * YScale))).ToArray();
        }

        public override SixLabors.ImageSharp.PointF[] Apply(SixLabors.ImageSharp.PointF[] points)
        {
            return points.Select(p => new SixLabors.ImageSharp.PointF((float)(p.X * XScale), (float)(p.Y * YScale))).ToArray();
        }
    }
    class LWJTranslateTransform : LWJTransform
    {
        public double XShift { get; set; } = 0;
        public double YShift { get; set; } = 0;

        public override PointF[] Apply(PointF[] points)
        {
            return points.Select(p => new PointF((float)(p.X + XShift), (float)(p.Y + YShift))).ToArray();
        }

        public override SixLabors.ImageSharp.PointF[] Apply(SixLabors.ImageSharp.PointF[] points)
        {
            return points.Select(p => new SixLabors.ImageSharp.PointF((float)(p.X + XShift), (float)(p.Y + YShift))).ToArray();
        }
    }


    class LWJFont
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public int Style { get; set; }
        public LWJFont() { }
        public LWJFont(Font f)
        {
            Name = f.Name;
            Size = f.Size;
            Style = (int)f.Style;
        }
        public LWJFont(string name, float size, int style)
        {
            Name = name;
            Size = size;
            Style = style;
        }
        public Font GetFont()
        {
            return new Font(Name, Size, (FontStyle)Style);
        }

        public FontStyle GetStyle()
        {
            return (FontStyle)Style;
        }
    }

    enum LWJHAlign
    {
        Left,
        Center,
        Right,
        Justified,
        Centered
    }
    enum LWJVAlign
    {
        Top,
        Center,
        Bottom,
        Equidistant
    }

    public static class LWJEnumExtenstions
    {
        internal static SixLabors.Fonts.HorizontalAlignment HALIGN(this LWJHAlign align)
        {
            switch (align)
            {
                case LWJHAlign.Left:
                    return SixLabors.Fonts.HorizontalAlignment.Left;
                case LWJHAlign.Center:
                    return SixLabors.Fonts.HorizontalAlignment.Center;
                case LWJHAlign.Right:
                    return SixLabors.Fonts.HorizontalAlignment.Right;
                case LWJHAlign.Justified:
                    return SixLabors.Fonts.HorizontalAlignment.Left; // TODO: this may not be right...
                case LWJHAlign.Centered:
                    return SixLabors.Fonts.HorizontalAlignment.Left; // TODO: this may not be right...
            }
            return SixLabors.Fonts.HorizontalAlignment.Left;
        }

        internal static SixLabors.Fonts.VerticalAlignment VALIGN(this LWJVAlign align)
        {
            switch (align)
            {
                case LWJVAlign.Top:
                    return SixLabors.Fonts.VerticalAlignment.Top;
                case LWJVAlign.Center:
                    return SixLabors.Fonts.VerticalAlignment.Center;
                case LWJVAlign.Bottom:
                    return SixLabors.Fonts.VerticalAlignment.Bottom;
                case LWJVAlign.Equidistant:
                    return SixLabors.Fonts.VerticalAlignment.Top; // TODO: check if this is right...
            }
            return SixLabors.Fonts.VerticalAlignment.Top;
        }

    }


    class LWJColor
    {
        /// <summary>
        /// #AARRGGBB
        /// </summary>
        public string Hex { get; set; }

        [JsonIgnore]
        public int Alpha { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        [JsonIgnore]
        public int Red { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        [JsonIgnore]
        public int Green { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        [JsonIgnore]
        public int Blue { get => Hex?.Length == 9 ? int.Parse(Hex?.Substring(7, 2), System.Globalization.NumberStyles.HexNumber) : 0; }

        public LWJColor() { }
        public LWJColor(Color col)
        {
            Hex = $"#{col.A:X2}{col.R:X2}{col.G:X2}{col.B:X2}";
        }
        public Color GetColor()
        {
            return Color.FromArgb(Alpha, Red, Green, Blue);
        }

        public SixLabors.ImageSharp.Color ToColor()
        {
            return SixLabors.ImageSharp.Color.FromRgba((byte)Red, (byte)Green, (byte)Blue, (byte)Alpha);
        }

        public SixLabors.ImageSharp.Color ToAlphaColor()
        {
            return SixLabors.ImageSharp.Color.FromRgba((byte)Alpha, (byte)Alpha, (byte)Alpha, (byte)Alpha);
        }

        public SixLabors.ImageSharp.Color RGBFromAlpha()
        {
            return SixLabors.ImageSharp.Color.FromRgba((byte)Alpha, (byte)Alpha, (byte)Alpha, 255);
        }

    }

    class LWJTLVerseLayout
    {
        public LWJSize Size { get; set; }
        public LWJRect Key { get; set; }
        public LWJRect Textbox { get; set; }
        public LWJRect TitleLine { get; set; }
        public LWJFont Font { get; set; }
    }



}
