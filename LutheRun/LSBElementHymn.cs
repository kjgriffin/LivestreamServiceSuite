using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun
{
    class LSBElementHymn : ILSBElement, IDownloadWebResource
    {

        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";
        public string Copyright { get; private set; } = "";

        private bool HasText { get; set; } = false;
        private int imageIndex = 0;

        public IEnumerable<HymnImageLine> Images => ImageUrls;

        private List<HymnTextVerse> TextVerses = new List<HymnTextVerse>();
        private List<HymnImageLine> ImageUrls = new List<HymnImageLine>();


        public int Lines
        {
            get
            {
                return Images.Count();
            }
        }

        public int LineWidthVariance(string localpath = "")
        {
            int variance = 0;
            int width = 0;
            bool first = true;
            Task.Run(() => GetResourcesFromWeb(localpath)).Wait();
            foreach (var image in ImageUrls)
            {
                // need to perform some image manipulation. crop based on black&white to find true size
                if (image.Loaded)
                {
                    //int iwidth = Xenon.Helpers.GraphicsHelper.TrimBitmap(image.Bitmap, Color.FromArgb(230, 230, 230)).Width;
                    var nb = Xenon.Helpers.GraphicsHelper.Rescale(image.Bitmap, 2);
                    var tb = Xenon.Helpers.GraphicsHelper.TrimBitmap(nb, Color.FromArgb(0,0,0,0), false);
                    // resize image here?
                    image.Bitmap = tb;
                    var iwidth = tb.Width;
                    if (first)
                    {
                        width = iwidth;
                        first = false;
                    }
                    else
                    {
                        variance += Math.Abs(iwidth - width);
                    }
                }
            }
            return variance;
        }

        public static LSBElementHymn Parse(IElement element)
        {
            // all hymns have caption and subcaption
            var res = new LSBElementHymn();
            res.Caption = element.QuerySelectorAll(".caption-text").FirstOrDefault()?.TextContent ?? "";
            res.SubCaption = element.QuerySelectorAll(".subcaption-text").FirstOrDefault()?.TextContent ?? "";

            // then parse the lsb-content (could be either text or image)
            var content = element.Children.Where(c => c.LocalName == "lsb-content").FirstOrDefault();

            if (content == null)
            {
                return res;
            }

            foreach (var child in content?.Children)
            {
                if (child.ClassList.Contains("numbered-stanza"))
                {
                    res.HasText = true;
                    HymnTextVerse verse = new HymnTextVerse();
                    verse.Number = child.QuerySelectorAll(".stanza-number").FirstOrDefault()?.TextContent ?? "";
                    //var s = Regex.Replace(child.TextContent, @"^\d+", "");
                    //verse.Lines = s.Split(new string[] { "\r\n", "\r", "\n", Environment.NewLine, ".", ",", ":", ";", "!", "?", "    " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    //res.TextVerses.Add(verse);
                    verse.Lines = child.ParagraphText();
                    res.TextVerses.Add(verse);
                }
                else if (child.ClassList.Contains("image"))
                {
                    HymnImageLine imageline = new HymnImageLine();
                    var picture = child.Children.First();
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
                    }
                    var img = picture.Children.FirstOrDefault(c => c.LocalName == "img");
                    imageline.LocalPath = img?.Attributes["src"].Value;
                    imageline.InferedName = $"Hymn_{res.Caption.Replace(",", "")}_{res.imageIndex++}";
                    res.ImageUrls.Add(imageline);
                }
                else if (child.ClassList.Contains("copyright"))
                {
                    res.Copyright = res.Copyright + child.StrippedText();
                }
            }

            return res;
        }

        public string DebugString()
        {
            if (HasText)
            {
                return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_HYMN_WITH_TEXT. Caption:'{Caption}' SubCaption:'{SubCaption}''";
            }
            return "";
        }

        public string XenonAutoGen()
        {
            // Assumes that any text verses will always be at the end.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/// <XENON_AUTO_GEN>");
            sb.AppendLine(XenonAutoGenImageHymn());
            sb.AppendLine(XenonAutoGenTextHymn());
            sb.AppendLine("/// </XENON_AUTO_GEN>");
            return sb.ToString();
        }


        private string XenonAutoGenTextHymn()
        {
            StringBuilder sb = new StringBuilder();
            if (HasText)
            {
                var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");
                string title = "Hymn";
                string name = match.Groups["name"]?.Value.Trim() ?? "";
                string number = "LSB " + match.Groups["number"]?.Value.Trim() ?? "";
                string tune = "";
                string copyright = Copyright;
                sb.Append($"#texthymn(\"{title}\", \"{name}\", \"{tune}\", \"{number}\", \"{copyright}\") {{\r\n");

                foreach (var verse in TextVerses)
                {
                    string verseinsert = verse.Number != string.Empty ? $"(Verse {verse.Number})" : "";
                    sb.AppendLine($"#verse{verseinsert} {{");
                    foreach (var line in verse.Lines)
                    {
                        sb.AppendLine(line.Trim());
                    }
                    sb.AppendLine("}");
                }
                sb.Append("}");
            }
            return sb.ToString();
        }

        private string XenonAutoGenImageHymn()
        {
            StringBuilder sb = new StringBuilder();
            if (ImageUrls.Count() > 0)
            {
                var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");
                string title = "Hymn";
                string name = match.Groups["name"]?.Value.Trim() ?? "";
                string number = "LSB " + match.Groups["number"]?.Value.Trim() ?? "";
                string tune = "";
                string copyright = Copyright;

                //sb.AppendLine($"/// \"{title}\", \"{name}\", \"{tune}\", \"{number}\", \"{copyright}\"");
                sb.AppendLine($"#stitchedimage(\"{title}\", \"{name}\", \"{number}\", \"{copyright}\") {{");
                //sb.AppendLine("/// URLS::");
                foreach (var imageline in ImageUrls)
                {
                    //sb.AppendLine($"/// img={imageline.RetinaScreenURL}");
                    sb.AppendLine($"{imageline.InferedName};");
                }
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        public Task GetResourcesFromWeb(string localpath = "")
        {
            var tasks = ImageUrls.Select(async imageurls =>
            {
                if (!imageurls.Loaded)
                {
                    await imageurls.GetResourcesFromWeb(localpath);
                }
            });
            return Task.WhenAll(tasks);
        }

        class HymnTextVerse
        {
            public string Number { get; set; } = "";
            public List<string> Lines { get; set; } = new List<string>();
        }

        internal class HymnImageLine : IDownloadWebResource
        {
            public string LocalPath { get; set; } = "";
            public string PrintURL { get; set; } = "";
            public string ScreenURL { get; set; } = "";
            public string RetinaScreenURL { get; set; } = "";
            public string InferedName { get; set; } = "";
            public Bitmap Bitmap { get; set; }
            public bool Loaded { get; private set; } = false;

            public IEnumerable<HymnImageLine> Images => throw new NotImplementedException();

            public static HymnImageLine Parse(IElement element, string name)
            {
                HymnImageLine imageline = new HymnImageLine();
                var picture = element.Children.First();
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
                }
                imageline.InferedName = name;
                return imageline;
            }

            public Task GetResourcesFromWeb(string localpath = "")
            {

                return Task.Run(async () =>
                           {
                               // try loading locally
                               string path = Path.Combine(localpath, LocalPath);
                               string file = Path.GetFullPath(path);
                               if (File.Exists(file))
                               {
                                   try
                                   {
                                       System.Diagnostics.Debug.WriteLine($"Loading image from local file {path}.");
                                       Bitmap = new Bitmap(file);
                                       Loaded = true;
                                       return;
                                   }
                                   catch (Exception ex)
                                   {
                                       System.Diagnostics.Debug.WriteLine($"Failed to load local file {path} because {ex}. Falling back to web.");
                                   }
                               }
                               // use screenurls
                               try
                               {
                                   System.Diagnostics.Debug.WriteLine($"Fetching image {RetinaScreenURL} from web.");
                                   System.Net.WebRequest request = System.Net.WebRequest.Create(RetinaScreenURL);
                                   System.Net.WebResponse response = await request.GetResponseAsync();
                                   System.IO.Stream responsestream = response.GetResponseStream();
                                   Bitmap = new Bitmap(responsestream);
                                   Loaded = true;
                               }
                               catch (Exception ex)
                               {
                                   Debug.WriteLine($"Failed trying to download: {RetinaScreenURL}\r\n{ex}");
                               }
                           });

            }
        }


    }

}
