using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xenon.Helpers;

namespace Xenon.Renderer.ImageFilters
{
    static class ImageFilters
    {

        public static (Bitmap b, Bitmap k) SolidColorCanvas(Bitmap inb, Bitmap inkb, SolidColorCanvasFilterParams fparams)
        {
            Bitmap _b = new Bitmap(fparams.Width, fparams.Height);
            Bitmap _k = new Bitmap(fparams.Width, fparams.Height);

            Graphics gfx = Graphics.FromImage(_b);
            Graphics kgfx = Graphics.FromImage(_k);

            gfx.Clear(fparams.Background);
            kgfx.Clear(fparams.KBackground);

            return (_b, _k);
        }


    }
}
