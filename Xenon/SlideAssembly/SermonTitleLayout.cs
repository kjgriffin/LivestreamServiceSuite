using System.Drawing;
using System.Text.Json.Serialization;

using Xenon.LayoutEngine;

namespace Xenon.SlideAssembly
{
    public class SermonTitleLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle TextAera { get; set; }
        public Rectangle TopLine { get; set; }
        public Rectangle MainLine { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public SermonTitleLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }

        public SermonLayoutRenderInfo GetRenderInfo()
        {
            return new SermonLayoutRenderInfo();
        }
    }
}
