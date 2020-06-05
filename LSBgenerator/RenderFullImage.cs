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

                double xscale = 1;
                double yscale = 1;

                xscale = (double)r.DisplayWidth / (double)Image.Width;
                yscale = (double)r.DisplayHeight / (double)Image.Height;

                scale = xscale < yscale ? xscale : yscale;

                s = new Size((int)(Image.Width * scale), (int)(Image.Height * scale));
                p = new Point((r.DisplayWidth - s.Width) / 2, (r.DisplayHeight - s.Height) /2);


                // clear background
                slide.gfx.Clear(Color.White);

                // draw image scaled fullscrean
                slide.gfx.DrawImage(Image, new Rectangle(p, s), new Rectangle(new Point(0, 0), Image.Size), GraphicsUnit.Pixel);
            }
        }
    }
}
