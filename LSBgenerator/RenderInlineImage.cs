using System;
using System.Drawing;

namespace LSBgenerator
{
    [Serializable]
    public class RenderInlineImage : IRenderable
    {
        public int RenderX { get; set; }
        public int RenderY { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public LayoutMode RenderLayoutMode { get; set; }

        public Bitmap Image { get; set; }
        public void Render(RenderSlide slide, TextRenderer r)
        {
            if (RenderLayoutMode == LayoutMode.Auto)
            {
                // center image in textrect
                slide.gfx.DrawImage(Image, r.KeyRect, new Rectangle(new Point(0, 0), Image.Size), GraphicsUnit.Pixel);
            }
            else
            {
                // scale image to fit height, may not fit center
                int oldw = Image.Width;
                int oldh = Image.Height;

                float scale = (float)r.TextboxRect.Height / (float)oldh;
                RectangleF imgr = new RectangleF(0, 0, oldw * scale, oldh * scale);

                // center
                imgr.Location = new PointF((r.TextboxRect.Width - imgr.Width) / 2, r.TextboxRect.Y);

                slide.gfx.DrawImage(Image, imgr);
            }
        }
    }
}
