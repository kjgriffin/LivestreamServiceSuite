using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        public static System.Windows.Media.Imaging.BitmapImage ToBitmapImage(this string base64EncodedPNG)
        {
            System.Windows.Media.Imaging.BitmapImage res = new System.Windows.Media.Imaging.BitmapImage();
            res.BeginInit();
            res.StreamSource = new MemoryStream(Convert.FromBase64String(base64EncodedPNG));
            res.EndInit();
            // I think this is ok, since we never want to modify this. We'd re-render through Xenon in that case
            // this should allow a UI thread in WPF to use it even though it wasn't generated on that thread
            res.Freeze();
            return res;
        }

        /// <summary>
        /// This only works for an image loaded using a stream source. NOT a URI source
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        //public static string ToBase64PngString(this BitmapImage image)
        //{
        //    byte[] buffer = new byte[image.StreamSource.Length];
        //    image.StreamSource.Seek(0, SeekOrigin.Begin);
        //    image.StreamSource.Read(buffer, 0, buffer.Length);

        //    return Convert.ToBase64String(buffer);
        //}

        public static string ToBase64PngString(this BitmapImage image)
        {
            string str64 = "";
            var frame = BitmapFrame.Create(image);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                str64 = ms.ToBase64PngString();
            }

            return str64;
        }


        public static string ToBase64PngString(this Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return Convert.ToBase64String(buffer);
        }



    }
}
