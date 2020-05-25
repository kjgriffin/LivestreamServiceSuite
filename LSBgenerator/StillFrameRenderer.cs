using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{
    public class StillFrameRenderer
    {

        public ImageFormat Format { get; set; } = ImageFormat.Png;

        public void ExportStillFrames(string filepath, List<Bitmap> stills)
        {
            int slidenum = 0;
            foreach (var item in stills)
            {
                FileStream s = new FileStream(Path.Combine(filepath, $"Slide_{slidenum++}.{Format}"), FileMode.OpenOrCreate, FileAccess.Write);
                item.Save(s, Format);
                s.Close();
            }
        }





    }
}
