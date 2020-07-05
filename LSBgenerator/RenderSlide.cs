using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{
    [Serializable]
    public class RenderSlide
    {

        public List<IRenderable> RenderLines { get; set; } = new List<IRenderable>();

        public int Order { get; set; } = 0;

        public int YOffset { get; set; } = 0;

        public bool Blank { get => RenderLines.Count == 0; }

        public int Lines { get; set; } = 0;

        public Bitmap rendering { get; set; } = new Bitmap(1920, 1080);
        public Graphics gfx { get => Graphics.FromImage(rendering); }

        public bool IsMediaReference { get; set; } = false;

        public string MediaReference { get; set; } = string.Empty;
    }
}
