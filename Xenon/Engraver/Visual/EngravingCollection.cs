using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Collections.Generic;

using Xenon.Engraver.Visual;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class EngravingCollection : IEngravingRenderable
    {
        internal float XOffset { get; set; } = 0f;
        internal float YOffset { get; set; } = 0f;
        internal List<IEngravingRenderable> Objects { get; set; } = new List<IEngravingRenderable>();

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, bool debug = false)
        {
            foreach (var obj in Objects)
            {
                obj.Render(X + XOffset, Y + YOffset, ibmp, ikbmp, layout, debug);
            }
        }
    }

}
