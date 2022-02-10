using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xenon.Layouts;

namespace Xenon.Renderer.Helpers
{
    internal static class TransformHelper
    {

        public static PointF[] ApplyTransforms(this List<LWJPoint> points, LWJTransformSet transforms)
        {
            // pull into points
            var npoints = points.Select(x => x.GetPointF()).ToArray();

            if (transforms == null)
            {
                return npoints;
            }

            // apply in order
            npoints = transforms?.Scale?.Apply(npoints) ?? npoints;
            npoints = transforms?.Translate?.Apply(npoints) ?? npoints;

            return npoints;
        }



    }
}
