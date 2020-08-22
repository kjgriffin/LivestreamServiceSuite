using SlideCreater.LayoutEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;

namespace SlideCreater.SlideAssembly
{
    public class ReadingLayout
    {
        public Size Size { get; set; }
        public Rectangle Key { get; set; }
        public Rectangle TextAera { get; set; }
        [JsonIgnore]
        public Font Font { get; set; }

        public ReadingLayout()
        {
            Font = new Font("Arial", 36, FontStyle.Regular);
        }

        public ReadingLayoutRenderInfo GetRenderInfo()
        {
            return new ReadingLayoutRenderInfo();
        }

    }
}
