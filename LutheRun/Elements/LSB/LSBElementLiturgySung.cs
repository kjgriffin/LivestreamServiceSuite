using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static LutheRun.Elements.LSB.LSBElementHymn;

namespace LutheRun.Elements.LSB
{
    class LSBElementLiturgySung : ILSBElement, IDownloadWebResource
    {
        public string PostsetCmd { get; set; }
        private HymnImageLine Image;

        public IEnumerable<HymnImageLine> Images => Image.ItemAsEnumerable();

        public IElement SourceHTML { get; private set; }

        public BlockType BlockType()
        {
            return Pilot.BlockType.LITURGY_CORPERATE;
        }

        public static LSBElementLiturgySung Parse(IElement element)
        {
            LSBElementLiturgySung res = new LSBElementLiturgySung();
            res.SourceHTML = element;
            HymnImageLine imageline = new HymnImageLine();
            var pictures = element?.Children.Where(c => c.LocalName == "picture");
            int localid = 0;
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
                    // guid for uniqueness was a bad idea since we don't seem to be able to recreate them...
                    // for now use the parser's unique id generation (though not ideal code style)
                    imageline.InferedName = $"LiturgySung_{LSBParser.elementID}-{localid++}";
                    res.Image = imageline;
                }
            }
            return res;
        }

        public string DebugString()
        {
            return $"/// XENON DEBUG::Parsed as LSB_ELEMENT_SUNG_LITUGY.";
        }

        public async Task GetResourcesFromLocalOrWeb(string localpath = "")
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

                using (Stream rstream = await Xenon.Helpers.WebHelpers.httpClient.GetStreamAsync(Image.ScreenURL))
                {
                    Image.Bitmap = new Bitmap(rstream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed trying to download: {Image.ScreenURL}\r\n{ex}");
            }
        }

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine("/// <XENON_AUTO_GEN>");
            sb.AppendLine($"#litimage({Image.InferedName}){PostsetCmd}".Indent(indentDepth, indentSpaces));
            //sb.AppendLine("/// </XENON_AUTO_GEN>");

            return sb.ToString();
        }
    }
}
