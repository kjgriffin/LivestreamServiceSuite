using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SlideCreater
{
    public static class GraphicsHelper
    {


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


        public static StringFormat TopLeftAlign => new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
        public static StringFormat CenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        public static StringFormat LeftVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
        public static StringFormat RightVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far };

    }
}
