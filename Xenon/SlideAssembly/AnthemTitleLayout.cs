using System.Drawing;
using System.Text.Json.Serialization;

namespace Xenon.SlideAssembly
{
    public class AnthemTitleLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle TopLine { get; set; }
        public Rectangle MainLine { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public AnthemTitleLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }
    }
}
