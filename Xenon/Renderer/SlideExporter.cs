using Xenon.SlideAssembly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xenon.Compiler;
using System.Linq;
using System.Text.Json;
using SixLabors.ImageSharp;

namespace Xenon.Renderer
{
    public class SlideExporter
    {
        public static List<string> WillCreate(RenderedSlide rs)
        {
            List<string> created = new List<string>();
            if (rs.RenderedAs == "Resource")
            {
                created.Add($"Resource_{rs.Name}{rs.CopyExtension}");
            }

            if (rs.MediaType == MediaType.Image)
            {
                string filename = $"{rs.Number}_{rs.RenderedAs}.png";
                string kfilename = $"Key_{rs.Number}.png";
                if (rs.OverridingBehaviour?.ForceOverrideExport == true)
                {
                    filename = $"{rs.OverridingBehaviour.OverrideExportName}.png";
                    kfilename = $"{rs.OverridingBehaviour.OverrideExportKeyName}.png";
                }
                created.Add(filename);
                created.Add(kfilename);
            }
            else if (rs.MediaType == MediaType.Video)
            {
                string filename = $"{rs.Number}_{rs.RenderedAs}.mp4";
                string kfilename = $"Key_{rs.Number}.png";
                created.Add(filename);
                created.Add(kfilename);
            }
            else if (rs.MediaType == MediaType.Video_KeyedVideo)
            {
                string filename = $"{rs.Number}_{rs.RenderedAs}.mp4";
                string kfilename = $"Key_{rs.Number}.mp4";
                created.Add(filename);
                created.Add(kfilename);
            }
            else if (rs.MediaType == MediaType.Text)
            {
                // output text file
                string filename = $"{rs.Number}_{rs.RenderedAs}.txt";
                created.Add(filename);
            }

            // generate optional Postset
            if (rs.IsPostset)
            {
                string filename = $"Postset_{rs.Number}.txt";
            }

            return created;
        }

        public static void ExportSlides(string directory, Project proj, List<XenonCompilerMessage> messages)
        {
            SlideRenderer slideRenderer = new SlideRenderer(proj);

            foreach (var slide in proj.Slides)
            {
                // render the slide
                RenderedSlide rs = slideRenderer.RenderSlide(slide, messages);

                if (rs.RenderedAs == "Resource")
                {
                    // for now only allow audio files to be rendered as resource
                    // ^^^^^ Not any more!
                    // we'll let anything be a resource
                    // for now it would be done for Image type slides, with an overriden renderedas
                    string filename = Path.Join(directory, $"Resource_{rs.Name}{rs.CopyExtension}");
                    File.Copy(rs.AssetPath, filename, true);
                    continue;
                }

                if (rs.MediaType == MediaType.Image)
                {
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.png");
                    string kfilename = Path.Join(directory, $"Key_{slide.Number}.png");

                    if (rs.OverridingBehaviour?.ForceOverrideExport == true)
                    {
                        filename = Path.Join(directory, $"{slide.OverridingBehaviour.OverrideExportName}.png");
                        kfilename = Path.Join(directory, $"{slide.OverridingBehaviour.OverrideExportKeyName}.png");
                    }

                    using (var stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        rs.Bitmap.SaveAsPng(stream);
                    }
                    using (var stream = new FileStream(kfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        rs.KeyBitmap.SaveAsPng(stream);
                    }
                }
                else if (rs.MediaType == MediaType.Video)
                {
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.mp4");
                    string kfilename = Path.Join(directory, $"Key_{slide.Number}.png");
                    File.Copy(rs.AssetPath, filename);
                    using (var stream = new FileStream(kfilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        rs.KeyBitmap.SaveAsPng(stream);
                    }
                }
                else if (rs.MediaType == MediaType.Video_KeyedVideo)
                {
                    string filename = Path.Join(directory, $"{slide.Number}_{rs.RenderedAs}.mp4");
                    string kfilename = Path.Join(directory, $"Key_{slide.Number}.mp4");
                    File.Copy(rs.AssetPath, filename);
                    File.Copy(rs.KeyAssetPath, kfilename);
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
            proj.ProjectVariables.TryGetValue("global.rendermode.alpha", out List<string> rendermode);
            // we'd always want this now unless legacy is supplied
            if (rendermode == null || !rendermode.Any(s => s == "legacy"))
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
