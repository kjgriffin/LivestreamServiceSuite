using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

namespace Xenon.SlideAssembly
{
    public class TwoPartTitleLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle MainLine { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public TwoPartTitleLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }
    }
}
