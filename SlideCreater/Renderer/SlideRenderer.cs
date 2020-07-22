using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SlideCreater.Renderer
{
    public class SlideRenderer
    {

        public Project project { get; set; }
        
        public List<Bitmap> Render()
        {
            // iterate through all slides and generate images
            List<Bitmap> slides = new List<Bitmap>();

            foreach (var slide in project.Slides)
            {

                // generate slides
                if (slide.Format == "LITURGY")
                {
                    // draw it

                    // for now just draw the layout
                    Bitmap bmp = new Bitmap(project.Layouts.LiturgyLayout.Size.Width, project.Layouts.LiturgyLayout.Size.Height);

                    Graphics gfx = Graphics.FromImage(bmp);


                    gfx.Clear(Color.Black);

                    gfx.FillRectangle(Brushes.White, project.Layouts.LiturgyLayout.Key);

                    Font f = new Font("Arial", 36, System.Drawing.FontStyle.Bold);

                    StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

                    Rectangle text = new Rectangle(project.Layouts.LiturgyLayout.Text.Location.Add(project.Layouts.LiturgyLayout.Key.Location), project.Layouts.LiturgyLayout.Text.Size);

                    Rectangle speaker = new Rectangle(project.Layouts.LiturgyLayout.Speaker.Location.Add(project.Layouts.LiturgyLayout.Key.Location), project.Layouts.LiturgyLayout.Speaker.Size);
                    // just draw all text for now
                    foreach (var line in slide.Lines)
                    {
                        gfx.DrawString(line.Content[0], f, Brushes.Red, speaker, sf);
                        gfx.DrawString(line.Content[1], f, Brushes.Black, text, sf);
                    }

                    slides.Add(bmp);

                }

                
            }

            return slides;

        }


    }
}
