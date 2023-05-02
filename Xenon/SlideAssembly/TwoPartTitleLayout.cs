using System.Drawing;
using System.Text.Json.Serialization;

namespace Xenon.SlideAssembly
{
    public class TwoPartTitleLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle MainLine { get; set; }
        public Rectangle Line1 { get; set; }
        public Rectangle Line2 { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public TwoPartTitleLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }
    }
}
