using AngleSharp.Dom;

using LutheRun.Elements.Interface;
using LutheRun.Parsers;
using LutheRun.Parsers.DataModel;
using LutheRun.Pilot;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xenon.Helpers;

namespace LutheRun.Elements.LSB
{
    class LSBElementHymn : ILSBElement, ICaptionElement, IDownloadWebResource
    {

        public string PostsetCmd { get; set; } = "";
        public string Caption { get; private set; } = "";
        public string SubCaption { get; private set; } = "";
        public string Copyright { get; private set; } = "";

        private bool HasText { get; set; } = false;
        private int imageIndex = 0;

        public bool IsCommunionHymn { get; set; } = false;

        public IEnumerable<HymnImageLine> Images => ImageUrls;

        private List<HymnTextVerse> TextVerses = new List<HymnTextVerse>();
        private List<HymnImageLine> ImageUrls = new List<HymnImageLine>();

        public IElement SourceHTML { get; private set; }

        public BlockType BlockType(LSBImportOptions importOptions)
        {
            return Pilot.BlockType.HYMN;
        }


        public int Lines
        {
            get
            {
                return Images.Count();
            }
        }

        public bool HasSeperateTextLines(string localpath = "")
        {
            int textlines = 0;

            Task.Run(() => GetResourcesFromLocalOrWeb(localpath)).Wait();
            foreach (var image in ImageUrls)
            {
                if (image.Loaded)
                {
                    //var tb = Xenon.Helpers.GraphicsHelper.TrimBitmap(image.Bitmap, Color.FromArgb(0, 0, 0, 0), false);
                    if (image.Bitmap.Height < 45)
                    {
                        textlines += 1;
                    }
                }
            }

            return textlines > 0;

        }

        public int LineWidthVariance(string localpath = "")
        {
            int variance = 0;
            int width = 0;
            bool first = true;
            Task.Run(() => GetResourcesFromLocalOrWeb(localpath)).Wait();
            foreach (var image in ImageUrls)
            {
                // need to perform some image manipulation. crop based on black&white to find true size
                if (image.Loaded)
                {
                    //int iwidth = Xenon.Helpers.GraphicsHelper.TrimBitmap(image.Bitmap, Color.FromArgb(230, 230, 230)).Width;
                    //var nb = Xenon.Helpers.GraphicsHelper.Rescale(image.Bitmap, 2);
                    var iwidth = image.Bitmap.TrimBitmap(Color.FromArgb(0, 0, 0, 0), false).Width;
                    // resize image here?
                    //image.Bitmap = tb;
                    //var iwidth = image.Bitmap.Width;
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
            res.SourceHTML = element;
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
                    verse.Lines = child.ParseNumberedStanzaText();
                    res.TextVerses.Add(verse);
                }
                if (child.ClassList.Contains("doxological-numbered-stanza"))
                {
                    res.HasText = true;
                    HymnTextVerse verse = new HymnTextVerse();
                    verse.Number = child.QuerySelectorAll(".stanza-number").FirstOrDefault()?.TextContent ?? "";
                    verse.Lines = child.ParseDoxologicalNumberedStanzaText();
                    verse.Doxological = true;
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
                    imageline.InferedName = $"Hymn_{res.Caption.Replace(",", "")}_{res.imageIndex++}_{LSBParser.elementID}";
                    res.ImageUrls.Add(imageline);
                }
                else if (child.ClassList.Contains("copyright"))
                {
                    res.Copyright = res.Copyright + child.StrippedText();
                }
                else if (child.ClassList.Contains("body"))
                {
                    // this often is a bit 'over eager' to find anything that looks like text...
                    // so lets make it check that there's actually something that looks vaugley like words beforew we start going after it
                    var unnumberedlines = child.ParagraphText2();
                    if (unnumberedlines.All(x => string.IsNullOrWhiteSpace(x)) || unnumberedlines.FirstOrDefault()?.StartsWith('<') == true) // yeah... probably not anything we'd need to worry about
                    {
                        continue;
                    }


                    res.HasText = true;


                    List<HymnTextVerse> verses = new List<HymnTextVerse>();

                    HymnTextVerse verse = new HymnTextVerse();
                    int vnum = 1;
                    verse.Number = vnum.ToString();
                    verse.Lines = new List<string>();

                    foreach (var line in unnumberedlines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            verses.Add(verse);
                            vnum += 1;
                            verse = new HymnTextVerse();
                            verse.Lines = new List<string>();
                            verse.Number = vnum.ToString();
                            continue;
                        }
                        else
                        {
                            verse.Lines.Add(line);
                        }
                    }
                    if (verse.Lines.Any())
                    {
                        verses.Add(verse);
                    }
                    res.TextVerses.AddRange(verses);

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

        public string XenonAutoGen(LSBImportOptions lSBImportOptions, ref int indentDepth, int indentSpaces, ParsedLSBElement fullInfo, Dictionary<string, string> ExtraFiles)
        {
            if (lSBImportOptions.UsePIPHymns)
            {
                return XenonAutoGen_PIP(ref indentDepth, indentSpaces, lSBImportOptions);
            }
            return XenonAutoGen_Simple(ref indentDepth, indentSpaces);
        }

        public string XenonAutoGen_Simple(ref int indentDepth, int indentSpaces)
        {
            // Assumes that any text verses will always be at the end.
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("/// <XENON_AUTO_GEN>");

            // may want to get a bit more advanced for split image/text hymns to apply first only on image and last only on text
            // for now we'll just put it on both

            var imagehymn = XenonAutoGenImageHymn(ref indentDepth, indentSpaces);
            var texthymn = XenonAutoGenTextHymn(ref indentDepth, indentSpaces);

            if (!string.IsNullOrEmpty(imagehymn))
            {
                sb.AppendLine(imagehymn + PostsetCmd);
            }
            if (!string.IsNullOrEmpty(texthymn))
            {
                sb.AppendLine(texthymn + PostsetCmd);
            }
            //sb.AppendLine("/// </XENON_AUTO_GEN>");
            return sb.ToString();
        }

        private string XenonAutoGen_PIP(ref int indentDepth, int indentSpaces, LSBImportOptions lSBImportOptions)
        {
            // Assumes that any text verses will always be at the end.
            StringBuilder sb = new StringBuilder();
            //sb.AppendLine("/// <XENON_AUTO_GEN>");

            // may want to get a bit more advanced for split image/text hymns to apply postset first only on image and postset last only on text
            // for now we'll just put it on both

            string wrappername = "PrePIPScriptBlock_Hymn-std";

            if (lSBImportOptions.RunPIPHymnsLikeAProWithoutStutters)
            {
                if (IsCommunionHymn && !lSBImportOptions.ImSoProICanRunPIPHymsWithoutStuttersEvenDuringCommunion)
                {
                    // intentionally do nothing here, because I'm too lazy to d'morgan this block of logic
                }
                else
                {
                    wrappername = "PrePIPScriptBlock_Hymn-fast";
                }
            }



            if (!lSBImportOptions.WrapConsecuitivePackages)
            {
                var name = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                                          .GetManifestResourceNames()
                                          .FirstOrDefault(x => x.Contains(wrappername));

                var stream = System.Reflection.Assembly.GetAssembly(typeof(ExternalPrefabGenerator))
                    .GetManifestResourceStream(name);

                // wrap it into a scripted block
                using (StreamReader sr = new StreamReader(stream))
                {
                    string pcmd = sr.ReadToEnd();
                    pcmd = Regex.Replace(pcmd, Regex.Escape("$>"), "".PadLeft(indentSpaces));

                    pcmd = Regex.Replace(pcmd, Regex.Escape("$PIPFILL"), IsCommunionHymn ? "ORGAN" : "BACK");
                    pcmd = Regex.Replace(pcmd, Regex.Escape("$PIPCAM"), IsCommunionHymn ? "ORGAN" : "BACK");
                    pcmd = Regex.Replace(pcmd, Regex.Escape("$POSTCAM"), IsCommunionHymn ? "ORGAN" : "CENTER");

                    sb.AppendLine(pcmd.IndentBlock(indentDepth, indentSpaces));
                }
                indentDepth++;

                sb.AppendLine();
            }

            var imagehymn = XenonAutoGenImageHymn(ref indentDepth, indentSpaces);
            if (!string.IsNullOrEmpty(imagehymn))
            {
                sb.AppendLine(imagehymn + PostsetCmd);
            }
            var texthymn = XenonAutoGenTextHymn(ref indentDepth, indentSpaces);
            if (!string.IsNullOrEmpty(texthymn))
            {
                sb.AppendLine(texthymn + PostsetCmd);
            }

            if (!lSBImportOptions.WrapConsecuitivePackages)
            {
                indentDepth--;
                sb.AppendLine("}".Indent(indentDepth, indentSpaces));
            }

            return sb.ToString();
        }


        private string XenonAutoGenTextHymn(ref int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();
            if (HasText)
            {
                var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");
                string title = "Hymn";
                string name = match.Groups["name"]?.Value.Trim() ?? "";
                string number = match.Groups["number"]?.Value.Trim().Length > 0 ? "LSB " + match.Groups["number"]?.Value.Trim() : "";
                string tune = "";
                string copyright = Copyright;
                sb.AppendLine($"#texthymn(\"{title}\", \"{name}\", \"{tune}\", \"{number}\", \"{copyright}\")".Indent(indentDepth, indentSpaces));
                sb.AppendLine("{".Indent(indentDepth, indentSpaces));

                indentDepth++;
                foreach (var verse in TextVerses)
                {
                    //string verseinsert = verse.Number != string.Empty ? $"(Verse {verse.Number})" : "";
                    string verseinsert = string.Empty;
                    if (verse.Number != string.Empty)
                    {
                        string vformat = verse.Number.Trim();
                        if (Regex.Match(vformat, @"\d+").Success)
                        {
                            vformat = $"Verse {vformat}";
                        }
                        verseinsert = $"({vformat})";
                    }
                    if (verse.Doxological)
                    {
                        verseinsert += "[doxological]";
                    }
                    sb.AppendLine($"#verse{verseinsert}".Indent(indentDepth, indentSpaces));
                    sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                    indentDepth++;
                    foreach (var line in verse.Lines)
                    {
                        sb.AppendLine(line.Trim().Indent(indentDepth, indentSpaces));
                    }
                    indentDepth--;
                    sb.AppendLine("}".Indent(indentDepth, indentSpaces));
                }
                indentDepth--;
                sb.Append("}".Indent(indentDepth, indentSpaces));
            }
            return sb.ToString();
        }

        private string XenonAutoGenImageHymn(ref int indentDepth, int indentSpaces)
        {
            StringBuilder sb = new StringBuilder();
            if (ImageUrls.Count() > 0)
            {
                var match = Regex.Match(Caption, @"(?<number>\d+)?(?<name>.*)");

                HymnCaptionExtractor.ExtractAsHymnInfo(this, out var title, out var name, out var number);
                string copyright = Copyright;

                sb.AppendLine($"#stitchedimage(\"{title}\", \"{name}\", \"{number}\", \"{copyright}\")".Indent(indentDepth, indentSpaces));
                sb.AppendLine("{".Indent(indentDepth, indentSpaces));
                indentDepth++;
                foreach (var imageline in ImageUrls)
                {
                    sb.AppendLine($"{imageline.InferedName};".Indent(indentDepth, indentSpaces));
                }
                indentDepth--;
                sb.Append("}".Indent(indentDepth, indentSpaces));
            }
            return sb.ToString();
        }

        public Task GetResourcesFromLocalOrWeb(string localpath = "")
        {
            var tasks = ImageUrls.Select(async imageurls =>
            {
                if (!imageurls.Loaded)
                {
                    await imageurls.GetResourcesFromLocalOrWeb(localpath);
                }
            });
            return Task.WhenAll(tasks);
        }

        class HymnTextVerse
        {
            public bool Doxological { get; set; } = false;
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

            public Task GetResourcesFromLocalOrWeb(string localpath = "")
            {

                return Task.Run(async () =>
                           {
                               Bitmap b = null;
                               // try loading locally
                               string path = Path.Combine(localpath, LocalPath);
                               string file = Path.GetFullPath(path);
                               bool foundlocal = false;
                               if (File.Exists(file))
                               {
                                   try
                                   {
                                       Debug.WriteLine($"Loading image from local file {path}.");
                                       b = new Bitmap(file);
                                       foundlocal = true;
                                   }
                                   catch (Exception ex)
                                   {
                                       Debug.WriteLine($"Failed to load local file {path} because {ex}. Falling back to web.");
                                   }
                               }
                               if (!foundlocal)
                               {
                                   Debug.WriteLine($"Failed to load local file {path} since it does not exist. Falling back to web.");
                                   // use screenurls
                                   try
                                   {
                                       Debug.WriteLine($"Fetching image {ScreenURL} from web.");

                                       using (Stream rstream = await WebHelpers.httpClient.GetStreamAsync(ScreenURL))
                                       {
                                           b = new Bitmap(rstream);
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       Debug.WriteLine($"Failed trying to download: {ScreenURL}\r\n{ex}");
                                   }
                               }
                               try
                               {
                                   if (b != null)
                                   {
                                       Bitmap = b;
                                       Loaded = true;
                                   }
                               }
                               catch (Exception ex)
                               {
                                   Debug.WriteLine($"Failed setting image for: {file}\r\n{ex}");
                               }
                           });

            }
        }


    }

    public static class HymnCaptionExtractor
    {
        public static void ExtractAsHymnInfo(ICaptionElement element, out string title, out string name, out string number)
        {
            var match = Regex.Match(element.Caption, @"(?<number>\d+)?(?<name>.*)");
            title = "Hymn";
            name = match.Groups["name"]?.Value.Trim() ?? "";
            number = match.Groups["number"]?.Value.Trim().Length > 0 ? "LSB " + match.Groups["number"]?.Value.Trim() : "";
        }
    }

}
