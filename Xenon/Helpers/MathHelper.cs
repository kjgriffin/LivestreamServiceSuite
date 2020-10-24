using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xenon.Helpers
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
        
        public static RectangleF Move(this RectangleF r, PointF offset)
        {
            return new RectangleF(r.X + offset.X, r.Y + offset.Y, r.Width, r.Height);
        }


        public static Rectangle Move(this Rectangle r, int offsetx, int offsety)
        {
            return new Rectangle(r.X + offsetx, r.Y + offsety, r.Width, r.Height);
        }
        
        public static RectangleF Move(this RectangleF r, float offsetx, float offsety)
        {
            return new RectangleF(r.X + offsetx, r.Y + offsety, r.Width, r.Height);
        }

        public static Rectangle Center(this Rectangle r)
        {
            return new Rectangle(r.CenterPoint(), r.Size);
        }

        public static System.Drawing.Point CenterPoint(this Rectangle r)
        {
            return new System.Drawing.Point(r.X + r.Width / 2, r.Y + r.Height / 2);
        }
    }
}
