using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Xenon.Helpers
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

        //public static Font LoadLSBSymbolFont()
        //{

        //}


        internal static StringFormat TopLeftAlign => new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Near };
        internal static StringFormat CenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        internal static StringFormat LeftVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };
        internal static StringFormat RightVerticalCenterAlign => new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Far };

    }
}
