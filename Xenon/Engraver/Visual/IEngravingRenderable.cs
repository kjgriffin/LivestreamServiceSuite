using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal interface IEngravingRenderable
    {
        RectangleF ComputeMaxLayoutBounds();
        void PerformLayout(float X, float Y, EngravingLayoutInfo layout);
        void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout);
    }


}
