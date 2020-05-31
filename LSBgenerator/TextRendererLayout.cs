using System;
using System.Drawing;

namespace LSBgenerator
{
    [Serializable]
    public class TextRendererLayout
    {
        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }


        public int TextboxWidth { get; set; }
        public int TextboxHeight { get; set; }

        //public int PaddingH { get; set; }
        public int PaddingLeft { get; set; }
        public int PaddingRight { get; set; }
        //public int PaddingV { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingBottom { get; set; }

        public int PaddingCol { get; set; }

        public Font Font { get; set; }

    }
}
