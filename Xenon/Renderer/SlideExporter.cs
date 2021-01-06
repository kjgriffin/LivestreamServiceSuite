using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Xenon.Compiler;

namespace Xenon.Renderer
{
    public class SlideExporter
    {
        public static void ExportSlides(string directory, Project proj, List<XenonCompilerMessage> messages)
        {
            foreach (var slide in proj.Slides)
            {
                SlideRenderer slideRenderer = new SlideRenderer(proj);
                // render the slide
                RenderedSlide rs = slideRenderer.RenderSlide(slide, messages);

                if (rs.RenderedAs == "Resource")
                {
                    // for now only allow audio files to be rendered as resource
                    string filename = Path.Join(directory, $"Resource_{rs.Name}{rs.CopyExtension}");
                    File.Copy(rs.AssetPath, filename, true);
                    continue;
                }

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
                else if (rs.MediaType == MediaType.Text)
                {
                    // output text file
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.txt");
                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.Write(rs.Text);
                    }
                }
                
            } 
        }
    }
}
