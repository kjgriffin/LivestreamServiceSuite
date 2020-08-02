using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls.Ribbon;

namespace SlideCreater
{
    public static class MathHelper
    {

        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Rectangle Move(this Rectangle r, Point offset)
        {
            return new Rectangle(r.X + offset.X, r.Y + offset.Y, r.Width, r.Height);
        }

        public static Rectangle Move(this Rectangle r, int offsetx, int offsety)
        {
            return new Rectangle(r.X + offsetx, r.Y + offsety, r.Width, r.Height);
        }
    }
}
