using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LutheRun.LSBElementHymn;

namespace LutheRun
{
    class LSBElementLiturgySung : ILSBElement, IDownloadWebResource
    {
        public string PostsetCmd { get; set; }
        private HymnImageLine Image;

        public IEnumerable<HymnImageLine> Images => Image.ItemAsEnumerable();

        public static LSBElementLiturgySung Parse(IElement element)
        {
            LSBElementLiturgySung res = new LSBElementLiturgySung();
            HymnImageLine imageline = new HymnImageLine();
            var pictures = element?.Children.Where(c => c.LocalName == "picture");
            foreach (var picture in pictures)
            {
                var sources = picture?.Children.Where(c => c.LocalName == "source");
                foreach (var source in sources)
                {
                    if (source.Attributes["media"].Value == "print")
                    {
                        imageline.PrintURL = source.Attributes["srcset"].Value;
                    }
                    else if (source.Attributes["media"].Value == "screen")
                    {
                        string src = source.Attributes["srcset"].Value;
                        var urls = src.Split(", ");
                        foreach (var url in urls)
                        {
                            if (url.Contains("retina"))
                            {
                                string s = url.Trim();
                                if (s.EndsWith(" 2x"))
                                {
                                    s = s.Substring(0, s.Length - 3);
                                }
                                imageline.RetinaScreenURL = s.Trim();
                            }
                            else
                            {
                                imageline.ScreenURL = url.Trim();
                            }
                        }
                    }
                    var img = picture.Children.FirstOrDefault(c => c.LocalName == "img");
                    imageline.LocalPath = img?.Attributes["src"].Value;
                    imageline.InferedName = $"LiturgySung_{Guid.NewGuid().ToString()}";
                    res.Image = imageline;
                }
            }
            return res;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_SUNG_LITUGY.";
        }

        public Task GetResourcesFromLocalOrWeb(string localpath = "")
        {
            return Task.Run(async () =>
            {
                // try loading locally
                string path = Path.Combine(localpath, Image.LocalPath);
                string file = Path.GetFullPath(path);
                if (File.Exists(file))
                {
                    try
                    {
                        Debug.WriteLine($"Loading image from local file {path}.");
                        Image.Bitmap = new Bitmap(file);
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to load local file {path} because {ex}. Falling back to web.");
                    }
                }
                // use screenurls
                try
                {
                    Debug.WriteLine($"Fetching image {Image.ScreenURL} from web.");
                    System.Net.WebRequest request = System.Net.WebRequest.Create(Image.ScreenURL);
                    System.Net.WebResponse response = await request.GetResponseAsync();
                    System.IO.Stream responsestream = response.GetResponseStream();
                    Image.Bitmap = new Bitmap(responsestream);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed trying to download: {Image.ScreenURL}\r\n{ex}");
                }
            });
        }

        public string XenonAutoGen()
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine("/// <XENON_AUTO_GEN>");
            sb.AppendLine($"#litimage({Image.InferedName}){PostsetCmd}");
            //sb.AppendLine("/// </XENON_AUTO_GEN>");

            return sb.ToString();
        }
    }
}
