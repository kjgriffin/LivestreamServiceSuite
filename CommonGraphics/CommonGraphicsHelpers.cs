using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonGraphics
{
    public static class CommonGraphicsHelpers
    {
        public static System.Windows.Media.Imaging.BitmapImage ToBitmapImage(this MemoryStream pngEnvodedMS)
        {
            System.Windows.Media.Imaging.BitmapImage res = new System.Windows.Media.Imaging.BitmapImage();
            res.BeginInit();
            pngEnvodedMS.Seek(0, SeekOrigin.Begin);
            res.StreamSource = pngEnvodedMS;
            res.EndInit();
            // I think this is ok, since we never want to modify this. We'd re-render through Xenon in that case
            // this should allow a UI thread in WPF to use it even though it wasn't generated on that thread
            res.Freeze();
            return res;

        }

    }
}
