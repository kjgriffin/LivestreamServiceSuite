using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xenon.Renderer
{
    public class RenderedSlide
    {
        public MediaType MediaType { get; set; }
        public string AssetPath { get; set; }
        public string RenderedAs { get; set; }
        public string Name { get; set; }
        public string CopyExtension { get; set; }
        public string Text { get; set; }
        public Bitmap Bitmap {get; set;}
        public Bitmap KeyBitmap {get; set;}
        public int Number { get; set; }

        public static RenderedSlide Default()
        {
            return new RenderedSlide() { MediaType = MediaType.Empty, AssetPath = "", CopyExtension = "", Text = "", Name = "", RenderedAs = "Default", Number = 0 };
        }
    }
}
