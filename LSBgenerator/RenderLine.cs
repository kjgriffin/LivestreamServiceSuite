using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSBgenerator
{
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
            slide.gfx.DrawString(target.Text, target.Font, target.TextBrush, new Rectangle(r.TextboxRect.X + target.RenderX, r.TextboxRect.Y + target.RenderY, r.TextboxRect.Width - target.RenderX, r.TextboxRect.Height - target.RenderY), r.format);
            //slide.gfx.FillRectangle(Brushes.Red, new Rectangle(r.TextboxRect.X + target.RenderX, r.TextboxRect.Y + target.RenderY, r.TextboxRect.Width - target.RenderX, r.TextboxRect.Height - target.RenderY));
            // draw speaker
            if (target.ShowSpeaker)
            {
                Font speakerfont = new Font(target.Font, FontStyle.Bold);
                int speakeroffsety = (int)Math.Floor(Math.Ceiling((slide.gfx.MeasureString(r.SpeakerText.TryGetVal(target.Speaker, "?"), speakerfont).Height - r.blocksize) / 2) - 1);
                Rectangle speakerblock = new Rectangle(r.LayoutRect.X, r.TextboxRect.Y + target.RenderY + speakeroffsety, r.blocksize, r.blocksize);

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
}
