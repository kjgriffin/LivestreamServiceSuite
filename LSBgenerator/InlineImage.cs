using System;

namespace LSBgenerator
{
    [Serializable]
    public class InlineImage : ITypesettable
    {
        public ProjectAsset ImageAsset { get; set; }
        public bool AutoScale { get; set; } = false;

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
            RenderInlineImage ril = new RenderInlineImage() { Image = ImageAsset.Image, Height = ImageAsset.Image.Height, Width = ImageAsset.Image.Width, RenderLayoutMode = AutoScale ? LayoutMode.Auto : LayoutMode.Fixed, RenderX = 0, RenderY = 0 };
            rslide.RenderLines.Add(ril);

            r.Slides.Add(r.FinalizeSlide(rslide));
            return new RenderSlide() { Order = rslide.Order + 1 };

        }
    }
}
