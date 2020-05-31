using System.Drawing;

namespace LSBgenerator
{
    public class RenderFullImage : IRenderable
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
                slide.gfx.DrawImage(Image, new Rectangle(0, 0, r.DisplayWidth, r.DisplayHeight), new Rectangle(new Point(0, 0), Image.Size), GraphicsUnit.Pixel);
            }
            else if (RenderLayoutMode == LayoutMode.PreserveScale)
            {
                double scale = 1;
                Point p;
                Size s;
                // scale by either width or height
                if (Image.Width < Image.Height)
                {
                    // scale by width
                    scale = (double)r.DisplayWidth / (double)Image.Width;
                    s = new Size((int)(Image.Width * scale), (int)(Image.Height * scale));
                    p = new Point(0, (r.DisplayHeight - s.Height) / 2);
                }
                else
                {
                    // scale by height
                    scale = (double)r.DisplayHeight / (double)Image.Height;
                    s = new Size((int)(Image.Width * scale), (int)(Image.Height * scale));
                    p = new Point((r.DisplayWidth - s.Width) / 2, 0);
                }


                // clear background
                slide.gfx.Clear(Color.White);

                // draw image scaled fullscrean
                slide.gfx.DrawImage(Image, new Rectangle(p, s), new Rectangle(new Point(0, 0), Image.Size), GraphicsUnit.Pixel);
            }
        }
    }
}
