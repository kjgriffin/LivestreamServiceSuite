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
        void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout, HashSet<string> debug = null);
    }

    internal static class DebugEngravingRenderableExtensions
    {
        internal static Dictionary<DFlags, string> Flags = new Dictionary<DFlags, string>
        {
            [DFlags.Origin] = "origin",
            [DFlags.Points] = "points",
            [DFlags.Bounds] = "bounds",
        };

        internal enum DFlags
        {
            Origin,
            Points,
            Bounds,
        }

        internal static bool HasFlag(this HashSet<string> set, DFlags flag)
        {
            return set?.Contains(Flags[flag]) == true;
        }

    }


}
