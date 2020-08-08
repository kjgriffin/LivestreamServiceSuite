using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

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



    }
}
