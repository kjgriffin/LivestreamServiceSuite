using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{
    class RenderSlide
    {

        public List<RenderLine> RenderLines { get; set; } = new List<RenderLine>();

        public int Order { get; set; } = 0;

        public int YOffset { get; set; } = 0;

        public bool Blank { get => RenderLines.Count == 0; }

        public int Lines { get; set; } = 0;

        public Bitmap rendering { get; set; } = new Bitmap(1920, 1080);
        public Graphics gfx { get => Graphics.FromImage(rendering); }
    }
}
