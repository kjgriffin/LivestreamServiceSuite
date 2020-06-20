using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LSBgenerator
{

    public enum SlideType
    {
        Video,
        Still,
    }
    public class StillVideoSlideRenderer
    {

        public ImageFormat Format { get; set; } = ImageFormat.Png;

        public void ExportStillFramesAndVideos(string filepath, List<(SlideType type, object slide)> slides)
        {
            int contentnum = 0;
            foreach (var item in slides)
            {
                if (item.type == SlideType.Still)
                {
                    FileStream s = new FileStream(Path.Combine(filepath, $"{contentnum++}_Slide.{Format}"), FileMode.OpenOrCreate, FileAccess.Write);
                    (item.slide as Bitmap).Save(s, Format);
                    s.Close();
                }
                else
                {
                    // copy video
                    File.Copy(item.slide as string, Path.Combine(filepath, $"{contentnum++}_Video.mp4"), true);
                }
            }
        }

    }
}
