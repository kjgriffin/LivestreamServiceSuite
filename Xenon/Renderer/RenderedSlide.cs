using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.IO;

using Xenon.SlideAssembly;

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
        public string Pilot { get; set; }
        public bool HasPilot { get => !string.IsNullOrWhiteSpace(Pilot); }
        public Image<Bgra32> Bitmap { get; set; }
        public Image<Bgra32> KeyBitmap { get; set; }

        public MemoryStream BitmapPNGMS { get; set; }
        public MemoryStream KeyPNGMS { get; set; }

        public int Number { get; set; }

        public bool IsPostset { get; set; }
        public int Postset { get; set; }

        public int SourceLineRef { get; set; } = 0;
        public string SourceFileRef { get; set; } = "";

        public SlideOverridingBehaviour OverridingBehaviour { get; set; }

        public static RenderedSlide Default()
        {
            return new RenderedSlide() { MediaType = MediaType.Empty, AssetPath = "", KeyAssetPath = "", CopyExtension = "", Text = "", Name = "", RenderedAs = "Default", Number = 0, IsPostset = false, Postset = -1, OverridingBehaviour = new SlideOverridingBehaviour(), Pilot = "" };
        }

        public RenderedSlide Clone()
        {
            return new RenderedSlide
            {
                MediaType = this.MediaType,
                AssetPath = this.AssetPath,
                KeyAssetPath = this.KeyAssetPath,
                CopyExtension = this.CopyExtension,
                Text = this.Text,
                Pilot = this.Pilot,
                Name = this.Name,
                RenderedAs = this.RenderedAs,
                Postset = this.Postset,
                IsPostset = this.IsPostset,
                Number = this.Number,
                OverridingBehaviour = this.OverridingBehaviour,
                Bitmap = this.Bitmap,
                BitmapPNGMS = this.BitmapPNGMS,
                KeyBitmap = this.KeyBitmap,
                KeyPNGMS = this.KeyPNGMS,
                SourceLineRef = this.SourceLineRef,
                SourceFileRef = this.SourceFileRef,
            };
        }
    }
}
