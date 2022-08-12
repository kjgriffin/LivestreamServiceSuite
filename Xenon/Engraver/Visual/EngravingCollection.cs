using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Generic;
using System.Linq;

using Xenon.Engraver.Visual;
using Xenon.LayoutInfo;

namespace Xenon.Engraver.Visual
{
    internal class EngravingCollection : IEngravingRenderable
    {
        float XOffset = 0f;
        float YOffset = 0f;
        
        internal List<IEngravingRenderable> ChildObjs { get; set; } = new List<IEngravingRenderable>();

        public RectangleF ComputeMaxLayoutBounds()
        {
            // for now we don't need to optomize
            // instead well gaurantee to be as correct as possible

            List<RectangleF> cosize = new List<RectangleF>();
            foreach (var item in ChildObjs)
            {
                cosize.Add(item.ComputeMaxLayoutBounds());
            }

            float xmin = cosize.Min(r => r.X);
            float xmax = cosize.Max(r => r.X);
            float ymin = cosize.Min(r => r.Y);
            float ymax = cosize.Max(r => r.Y);

            return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public void PerformLayout(float X, float Y, EngravingLayoutInfo layout)
        {
            XOffset = X;
            YOffset = Y;
            foreach (var obj in ChildObjs)
            {
                obj.PerformLayout(X + XOffset, Y + YOffset, layout);
            }
        }

        public void Render(float X, float Y, Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            foreach (var obj in ChildObjs)
            {
                obj.Render(ibmp, ikbmp, layout);
            }
        }

        public void Render(Image<Bgra32> ibmp, Image<Bgra32> ikbmp, EngravingLayoutInfo layout)
        {
            throw new NotImplementedException();
        }
    }



}
