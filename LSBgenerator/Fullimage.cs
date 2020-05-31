using System;

namespace LSBgenerator
{
    [Serializable]
    public class Fullimage : ITypesettable
    {
        public ProjectAsset ImageAsset { get; set; }

        public bool Streach { get; set; } = true;

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {
            // will force a new slide
            RenderSlide rslide = slide;
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                rslide = new RenderSlide() { Order = slide.Order + 1 };
            }

            // uses default renderline
            RenderFullImage ril = new RenderFullImage() { Image = ImageAsset.Image, Height = ImageAsset.Image.Height, Width = ImageAsset.Image.Width, RenderLayoutMode = Streach ? LayoutMode.Auto : LayoutMode.PreserveScale };
            rslide.RenderLines.Add(ril);

            r.Slides.Add(r.FinalizeSlide(rslide));
            return new RenderSlide() { Order = rslide.Order + 1 };
        }
    }
}
