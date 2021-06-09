using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
                    // is liturgy responsorial if it contains lsb-content elements
                    foreach (var child in element.Children.Where(c => c.LocalName == "lsb-content"))
                    {
                        ParseLSBServiceElement(child);
                    }
                }
                else if (element.ClassList.Contains("hymn"))
                {
                    serviceElements.Add(LSBElementHymn.Parse(element));
                }
                else if (element.ClassList.Contains("option-group") || element.ClassList.Contains("group"))
                {
                    // select all sub lsb-elements what are group selected
                    var selectedgroup = element.Children.Where(c => c.LocalName == "lsb-service-element" && c.ClassList.Contains("group") && c.ClassList.Contains("selected"));
                    foreach (var selectedelement in selectedgroup)
                    {
                        foreach (var lsbserviceelement in selectedelement.Children.Where(c => c.LocalName == "lsb-service-element"))
                        {
                            ParseLSBServiceElement(lsbserviceelement);
                        }
                    }
                }
                else
                {
                    serviceElements.Add(LSBElementUnknown.Parse(element));
                }
            }
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

        public async Task LoadWebAssets(Action<Bitmap, string, string> addImageAsAsset)
        {
            foreach (var se in serviceElements.Select(s => s as IDownloadWebResource))
            {
                if (se != null)
                {
                    await se.GetResourcesFromWeb();
                    foreach (var image in se.Images)
                    {
                        addImageAsAsset(image.Bitmap, image.RetinaScreenURL, image.InferedName);
                    }
                }
            }
        }

    }
}
