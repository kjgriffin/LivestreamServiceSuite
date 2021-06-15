using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LutheRun
{
    public class LSBParser
    {

        private StringBuilder stringBuilder = new StringBuilder();

        private List<ILSBElement> serviceElements = new List<ILSBElement>();

        public string XenonText { get => stringBuilder.ToString(); }

        private string ServiceFileName;



        public string XenonDebug()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var se in serviceElements)
            {
                sb.AppendLine(se.DebugString());
            }
            return sb.ToString();
        }

        public async Task ParseHTML(string path)
        {
            ServiceFileName = path;
            string html = "";
            try
            {
                using (TextReader reader = new StreamReader(path))
                {
                    html = await reader.ReadToEndAsync();
                    ParseHTMLIntoDOM(html);
                }
            }
            catch (Exception)
            {
            }
        }

        private async void ParseHTMLIntoDOM(string html)
        {
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(html);

            var bulletin = document.All.Where(e => e.Id == "bulletin_surface").FirstOrDefault();
            if (bulletin != null)
            {
                var service = bulletin.Children.First().Children.First().Children.First().Children.Where(e => e.LocalName == "lsb-service-element").ToList();
                ParseDOMElements(service);
            }

        }

        private void ParseDOMElements(List<AngleSharp.Dom.IElement> dom)
        {
            List<ILSBElement> elements = new List<ILSBElement>();
            serviceElements.Clear();
            foreach (var e in dom)
            {
                ParseLSBServiceElement(e);
            }
        }

        private void ParseLSBServiceElement(AngleSharp.Dom.IElement element)
        {

            if (element.LocalName == "lsb-content")
            {
                if (element.Children.Any(c => c.ClassList.Contains("lsb-responsorial")))
                {
                    serviceElements.Add(LSBElementLiturgy.Parse(element));
                }
                foreach (var imageline in element.Children.Where(c => c.ClassList.Contains("image")))
                {
                    serviceElements.Add(LSBElementLiturgySung.Parse(imageline));
                }
            }
            else
            {

                if (element.ClassList.Contains("heading"))
                {
                    serviceElements.Add(LSBElementHeading.Parse(element));
                }
                else if (element.ClassList.Contains("caption"))
                {
                    // TODO: do something different for anthem-titles, sermon-titles, postlude/preludes. Ignore others???
                    serviceElements.Add(LSBElementCaption.Parse(element));
                }
                else if (element.ClassList.Contains("static"))
                {
                    if (ParseAsPrefab(element))
                    {
                        return;
                    }

                    if (ParsePropperAsFullMusic(element))
                    {
                        return;
                    }

                    // is liturgy responsorial if it contains lsb-content elements
                    foreach (var child in element.Children.Where(c => c.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(child);
                    }
                }
                else if (element.ClassList.Contains("reading"))
                {
                    serviceElements.Add(LSBElementReading.Parse(element));
                }
                else if (element.ClassList.Contains("hymn"))
                {
                    serviceElements.Add(LSBElementHymn.Parse(element));
                }
                else if (element.ClassList.Contains("option-group") || element.ClassList.Contains("group"))
                {
                    var singlechild = element.Children.Where(c => c.LocalName == "lsb-service-element");
                    if (singlechild.Count() == 1)
                    {
                        if (singlechild.First().ClassList.Contains("static"))
                        {
                            ParseLSBServiceElement(singlechild.First());
                            return;
                        }
                    }

                    // select all sub lsb-elements what are group selected
                    var selectedgroup = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("selected"));
                    foreach (var selectedelement in selectedgroup)
                    {
                        if (selectedelement.ClassList.Contains("group"))
                        {
                            foreach (var lsbserviceelement in selectedelement.Children.Where(c => c.LocalName == "lsb-service-element"))
                            {
                                ParseLSBServiceElement(lsbserviceelement);
                            }
                        }
                        else if (selectedelement.ClassList.Contains("static"))
                        {
                            ParseLSBServiceElement(selectedelement);
                        }
                        else if (selectedelement.ClassList.Contains("prayer"))
                        {
                            foreach (var c in selectedelement.Children.Where(x => x.LocalName == "lsb-content"))
                            {
                                ParseLSBServiceElement(c);
                            }
                        }
                    }
                }
                else if (element.ClassList.Contains("proper"))
                {
                    if (ParseAsPrefab(element))
                    {
                        return;
                    }
                    if (ParsePropperAsFullMusic(element) == true)
                    {
                        return;
                    }
                }
                else if (element.ClassList.Contains("prayer"))
                {
                    if (ParseAsPrefab(element))
                    {
                        return;
                    }
                    foreach (var c in element.Children.Where(x => x.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(c);
                    }
                }
                else
                {
                    serviceElements.Add(LSBElementUnknown.Parse(element));
                }
            }
        }

        private bool ParsePropperAsFullMusic(IElement element)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption != null && caption?.Caption != string.Empty)
            {
                // reasonably sure this might have something interesting
                // we're expecting multiple images in the content
                foreach (var content in element.Children.Where(c => c.LocalName == "lsb-content"))
                {
                    var images = content.Children.Where(c => c.ClassList.Contains("image"));
                    if (images.Count() > 1)
                    {
                        LSBElementHymn hymn = LSBElementHymn.Parse(element);
                        // check that we really want this as a hymn
                        if (hymn.Lines <= 2)
                        {
                            // lets do it as litimage instead
                            return false;
                        }
                        int variance = hymn.LineWidthVariance(ServiceFileName);
                        if (variance > 10)
                        {
                            // pretty sure it's not one thing, but lots of sung liturgy
                            return false;
                        }

                        serviceElements.Add(hymn);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ParseAsPrefab(IElement element)
        {
            var caption = LSBElementCaption.Parse(element) as LSBElementCaption;
            if (caption != null && caption?.Caption != string.Empty)
            {
                string ctext = Regex.Replace(caption.Caption, @"[^\w ]", "");
                // first check if is known element (Creed/Prayer)
                Dictionary<string, string> prefabs = new Dictionary<string, string>()
                {
                    ["Apostles Creed"] = "apostlescreed",
                    ["Nicene Creed"] = "nicenecreed",
                    ["Lords Prayer"] = "lordsprayer",
                };
                if (prefabs.Keys.Contains(ctext))
                {
                    // use a prefab instead
                    serviceElements.Add(new LSBElementIsPrefab(prefabs[ctext], element.StrippedText()));
                    return true;
                }
            }
            return false;
        }

        public void CompileToXenon()
        {
            stringBuilder.Clear();
            stringBuilder.Append($"\r\n////////////////////////////////////\r\n// XENON AUTO GEN: From Service File '{System.IO.Path.GetFileName(ServiceFileName)}'\r\n////////////////////////////////////\r\n\r\n");
            foreach (var se in serviceElements)
            {
                stringBuilder.AppendLine(se.XenonAutoGen());
            }
        }

        public Task LoadWebAssets(Action<Bitmap, string, string> addImageAsAsset)
        {
            IEnumerable<IDownloadWebResource> resources = serviceElements.Select(s => s as IDownloadWebResource).Where(s => s != null);
            IEnumerable<Task> tasks = resources.Select(async s =>
            {
                await s.GetResourcesFromWeb(Path.GetDirectoryName(ServiceFileName));
                foreach (var image in s.Images)
                {
                    addImageAsAsset(image.Bitmap, image.RetinaScreenURL, image.InferedName);
                }
            });
            return Task.WhenAll(tasks);
        }

    }
}
