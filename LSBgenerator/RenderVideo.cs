using LSBgenerator.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{
    public class RenderVideo : ITypesettable
    {

        public ProjectAsset Asset { get; set; }

        public RenderSlide TypesetSlide(RenderSlide slide, TextRenderer r)
        {

            // force new slide
            RenderSlide rslide = slide;
            if (!slide.Blank)
            {
                r.Slides.Add(r.FinalizeSlide(slide));
                rslide = new RenderSlide() { Order = slide.Order + 1 };
            }

            // on new slide create it with only content for video
            rslide.IsMediaReference = true;
            rslide.MediaReference = Asset.ResourcePath;

            // add a render line to show it's a video

            RenderFullImage ril = new RenderFullImage() { Image = Asset.Image, Height = Asset.Image.Height, Width = Asset.Image.Width, RenderLayoutMode = LayoutMode.Auto};
            rslide.RenderLines.Add(ril);


            r.Slides.Add(r.FinalizeSlide(slide));

            // return new slide
            return new RenderSlide() { Order = rslide.Order + 1 };


        }


    }
}
