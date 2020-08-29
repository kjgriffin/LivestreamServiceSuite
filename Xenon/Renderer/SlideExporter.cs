using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Xenon.Renderer
{
    class SlideExporter
    {
        public static void ExportSlides(string directory, Project proj)
        {
            foreach (var slide in proj.Slides)
            {
                SlideRenderer slideRenderer = new SlideRenderer(proj);
                // render the slide
                RenderedSlide rs = slideRenderer.RenderSlide(slide.Number);

                if (rs.MediaType == MediaType.Image)
                {
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.png");
                    rs.Bitmap.Save(filename, ImageFormat.Png);
                }
                else if (rs.MediaType == MediaType.Video)
                {
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.mp4");
                    File.Copy(rs.AssetPath, filename);
                }
                
            } 
        }
    }
}
