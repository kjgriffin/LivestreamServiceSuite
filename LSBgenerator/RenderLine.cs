using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{

    public interface IRenderable
    {
        int RenderX { get; set; }
        int RenderY { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        LayoutMode RenderLayoutMode { get; set; }
        void Render(RenderSlide slide, TextRenderer r);
    }

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
                slide.gfx.DrawImage(Image, r.TextboxRect, new Rectangle(new Point(0, 0), Image.Size), GraphicsUnit.Pixel);
            }
            else
            {
                // scale image to fit height, may not fit center
                int oldw = Image.Width;
                int oldh = Image.Height;

                float scale = (float)r.ETextRect.Height / (float)oldh;
                RectangleF imgr = new RectangleF(0, 0, oldw * scale, oldh * scale);

                // center
                imgr.Location = new PointF((r.ETextRect.Width - imgr.Width) / 2, r.ETextRect.Y);

                slide.gfx.DrawImage(Image, imgr);
            }
        }
    }


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
            // center image in textrect
            slide.gfx.DrawImage(Image, new Rectangle(0, 0, r.DisplayWidth, r.DisplayHeight), new Rectangle(new Point(0, 0), Image.Size), GraphicsUnit.Pixel);
        }
    }
    public class RenderLine : IRenderable
    {
        public int RenderX { get; set; }
        public int RenderY { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string Text { get; set; }

        public Speaker Speaker { get; set; }

        public bool ShowSpeaker { get; set; }

        public LayoutMode RenderLayoutMode { get; set; }

        public int LineNum { get; set; }

        public Font Font { get; set; }

        public Brush TextBrush { get; set; } = Brushes.Black;


        public void Render(RenderSlide slide, TextRenderer r)
        {
            RenderLine target = this;
            // draw text
            slide.gfx.DrawString(target.Text, target.Font, target.TextBrush, new Rectangle(r.ETextRect.X + target.RenderX, r.ETextRect.Y + target.RenderY, r.ETextRect.Size.Width, r.ETextRect.Size.Height), r.format);
            // draw speaker
            if (target.ShowSpeaker)
            {
                Font speakerfont = new Font(target.Font, FontStyle.Bold);
                int speakeroffsety = (int)Math.Floor(Math.Ceiling((slide.gfx.MeasureString(r.SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont).Height - r.blocksize) / 2) - 1);
                Rectangle speakerblock = new Rectangle(r.TextboxRect.X + r.PaddingCol, r.ETextRect.Y + target.RenderY + speakeroffsety, r.blocksize, r.blocksize);

                if (target.Speaker == Speaker.None)
                {
                    return;
                }

                if (r.SpeakerFills.TryGetVal(target.Speaker, false))
                {
                    slide.gfx.FillPath(Brushes.Red, r.RoundedRect(speakerblock, 2));
                    slide.gfx.DrawString(r.SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont, Brushes.White, speakerblock, r.format);
                }
                else
                {
                    slide.gfx.FillPath(Brushes.White, r.RoundedRect(speakerblock, 2));
                    slide.gfx.DrawPath(Pens.Red, r.RoundedRect(speakerblock, 2));
                    slide.gfx.DrawString(r.SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont, Brushes.Red, speakerblock, r.format);
                }

            }

        }

    }

    public enum LayoutMode
    {
        Auto,
        Fixed,
    }
}
