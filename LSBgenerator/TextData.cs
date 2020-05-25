using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LSBgenerator
{


    enum Speaker
    {
        Pastor,
        Congregation,
        Leader,
        Assistant,
        Left,
        Right,
        Men,
        Women,
        Respond,
        Versicle,
        None,
    }

    class LiturgySpeaker
    {
        public Speaker Speaker { get; set; }

        public void Render() { }
        
    }

    class LiturgyLine
    {
        public Speaker Speaker { get; set; }
        public string Text { get; set; }

        public int SubSplit { get; set; } = 0;

        public bool IsSubsplit { get; set; } = false;


        /// <summary>
        /// Computes the display length of the string
        /// </summary>
        /// <returns></returns>
        public int ComputeWidth(Font f)
        {
            return 0;
        }

    }

    class TextData
    {

        public List<LiturgyLine> LineData = new List<LiturgyLine>();

        public void ParseText(string text)
        {
            var lines = text.Split(new[] { @"\\" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                LiturgyLine l = new LiturgyLine();
                string tl = line.Trim().Replace(Environment.NewLine, " ");
                l.Text = tl.Replace(" T ", " + ").Substring(1).Trim();

                if (tl.StartsWith("P"))
                {
                    l.Speaker = Speaker.Pastor;
                }
                else if (tl.StartsWith("C"))
                {
                    l.Speaker = Speaker.Congregation;
                }
                l.IsSubsplit = false;
                LineData.Add(l);
            }


        }



        StringFormat format = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.Word };

        public void Render_PreviewText(Graphics gfx, TextRenderer r, Font f, bool clipLines = true)
        {


            int linestartX = 10 +  (r.DisplayWidth - r.TextboxWidth)/2;
            int linestartY = r.DisplayHeight - r.TextboxHeight;

            int speakerblocksize = (int)(f.Size * gfx.DpiX / 72);

            gfx.Clear(Color.Black);


            gfx.FillRectangle(Brushes.White, (r.DisplayWidth - r.TextboxWidth) / 2, r.DisplayHeight - r.TextboxHeight, r.TextboxWidth, r.TextboxHeight);



            // render in centered mode
            foreach (var ll in LineData)
            {

                RectangleF displayaera = new RectangleF(3 + linestartX + speakerblocksize, linestartY + 3, r.TextboxWidth - speakerblocksize, r.TextboxHeight - 3);
                Font df;
                if (ll.Speaker == Speaker.Congregation)
                {
                    df = new Font(f, FontStyle.Bold);
                }
                else
                {
                    df = new Font(f, FontStyle.Regular);
                }

                if (clipLines)
                {
                    if (linestartY + (int)gfx.MeasureString(ll.Text, df, displayaera.Size).Height >= r.DisplayHeight)
                    {
                        return;
                    }
                }

                // draw speaker

                // compute lineheight of single character
                int lineheight = (int)Math.Ceiling(gfx.MeasureString("C", df).Height);
                int speakeroffsety = (int)Math.Floor((lineheight - speakerblocksize) / 2d) - 1;

                //gfx.DrawRectangle(Pens.Black, new Rectangle(linestartX, linestartY + speakeroffsety, speakerblocksize, speakerblocksize));
                Rectangle speakerblock = new Rectangle(linestartX, linestartY + speakeroffsety, speakerblocksize, speakerblocksize);


                if (ll.Speaker == Speaker.Congregation)
                {
                    Font cf = new Font(f, FontStyle.Bold);
                    gfx.FillPath(Brushes.Black, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawString("C", cf, Brushes.White, speakerblock, format );
                }

                if (ll.Speaker == Speaker.Pastor)
                {
                    Font cf = new Font(f, FontStyle.Bold);
                    gfx.FillPath(Brushes.White, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawPath(Pens.Black, DrawRoundedRect(speakerblock, 2));
                    gfx.DrawString("P", cf, Brushes.Black, speakerblock, format);
                }

                // account for offset of speaker graphic

                gfx.DrawString(ll.Text, df, Brushes.Black, displayaera, format);
                int yoffset = (int)gfx.MeasureString(ll.Text, df, new Size(r.TextboxWidth, r.TextboxHeight)).Height;
                linestartY += yoffset;

            }


        }

        private GraphicsPath DrawRoundedRect(Rectangle bounds, int borderradius)
        {

            int diameter = borderradius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (borderradius == 0)
            {
                path.AddRectangle(arc);
                return path;
            }

            path.AddArc(arc, 180, 90);

            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;

        }







    }
}
