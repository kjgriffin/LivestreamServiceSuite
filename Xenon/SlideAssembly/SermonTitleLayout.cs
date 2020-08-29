using Xenon.LayoutEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

namespace Xenon.SlideAssembly
{
    class SermonTitleLayout
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
