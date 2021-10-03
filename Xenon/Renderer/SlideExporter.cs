using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Xenon.Compiler;
using System.Linq;
using System.Text.Json;

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
                    string kfilename = Path.Join(directory, $"Key_{slide.Number}.png");
                    rs.Bitmap.Save(filename, ImageFormat.Png);
                    rs.KeyBitmap.Save(kfilename, ImageFormat.Png);
                }
                else if (rs.MediaType == MediaType.Video)
                {
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.mp4");
                    string kfilename = Path.Join(directory, $"Key_{slide.Number}.png");
                    File.Copy(rs.AssetPath, filename);
                    rs.KeyBitmap.Save(kfilename, ImageFormat.Png);
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

                // generate optional Postset
                if (rs.IsPostset)
                {
                    string filename = Path.Join(directory, $"Postset_{slide.Number}.txt");
                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.Write(rs.Postset);
                    }
                }

            }

            // generate non-standard config file to load BMD switcher for DSK1 to use pre-multipled key if rendered for premultipled alpha
            if (proj.ProjectVariables.TryGetValue("global.rendermode.alpha", out List<string> rendermode))
            {
                if (rendermode.Any(s => s == "premultiplied"))
                {
                    var config = Configurations.SwitcherConfig.DefaultConfig.GetDefaultConfig();
                    config.DownstreamKey1Config.IsPremultipled = 1;
                    string json = JsonSerializer.Serialize(config);
                    string filename = Path.Join(directory, $"BMDSwitcherConfig.json");
                    using (StreamWriter sw = new StreamWriter(filename, false))
                    {
                        sw.Write(json);
                    }
                }
            }


        }
    }
}
