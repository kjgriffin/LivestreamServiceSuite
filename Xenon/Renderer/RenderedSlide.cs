using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Xenon.Renderer
{
    public class RenderedSlide
    {
        public MediaType MediaType { get; set; }
        public string AssetPath { get; set; }
        public string KeyAssetPath { get; set; }
        public string RenderedAs { get; set; }
        public string Name { get; set; }
        public string CopyExtension { get; set; }
        public string Text { get; set; }
        public Image<Bgra32> Bitmap {get; set;}
        public Image<Bgra32> KeyBitmap {get; set;}
        public int Number { get; set; }

        public bool IsPostset { get; set; }
        public int Postset { get; set; }

        public SlideOverridingBehaviour OverridingBehaviour { get; set; }

        public static RenderedSlide Default()
        {
            return new RenderedSlide() { MediaType = MediaType.Empty, AssetPath = "", KeyAssetPath = "", CopyExtension = "", Text = "", Name = "", RenderedAs = "Default", Number = 0, IsPostset = false, Postset = -1 , OverridingBehaviour = new SlideOverridingBehaviour()};
        }
    }
}
