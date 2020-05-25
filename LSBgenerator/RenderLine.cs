using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{
    class RenderLine
    {
        public int RenderX { get; set; }
        public int RenderY { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string Text { get; set; }

        public Speaker Speaker { get; set; }

        public bool ShowSpeaker { get; set; }

        public LayoutMode RenderLayoutMode { get; set; }

        public int LineNum { get; set; }
    }

    enum LayoutMode
    {
        Auto,
        Fixed,
    }
}
