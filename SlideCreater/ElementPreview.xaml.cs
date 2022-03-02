using AngleSharp.Dom;
using CefSharp;
using SlideCreater.ViewControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for ElementPreview.xaml
    /// </summary>
    public partial class ElementPreview : UserControl
    {

        private string documentpath = "";

        LutheRun.LSBParser parser;
        LutheRun.ILSBElement element;

        public ElementPreview()
        {
            InitializeComponent();
            //textbox.SetFontSize(40);
            if (element != null)
            {
                Setup(parser, element, documentpath);
            }
        }

        public async void Setup(LutheRun.LSBParser parser, LutheRun.ILSBElement lsbelement, string docpath)
        {
            documentpath = docpath;
            element = lsbelement;
            this.parser = parser;
            // get the xenon command for the element
            string xenon_text = lsbelement.XenonAutoGen(new LutheRun.LSBImportOptions());
            // add text to textbox
            textbox.SetText(xenon_text);

            tbtype.Text = lsbelement.GetType().ToString();
            await PreviewXenonCode(parser, xenon_text);

            string body = ReplaceImages(lsbelement.SourceHTML)?.OuterHtml;

            // add default message for empty body
            if (string.IsNullOrWhiteSpace(body))
            {
                body = "<h1>No Source Element</h1><p>This element was likely infered from context by the LutheRun Servicifier and has no matching Lutheran Service Builder source element.</p>";
            }

            // create the web preview of the original
            string html = $"<!DOCTYPE html><html lang=\"en-us\">{RemoveScripts(parser.HTMLHead)?.OuterHtml}<body><section class=\"surface surface-XamlGeneratedNamespace-4\"><div class=\"bulletin-page lsml ember-view\"><div class=\"insertion-point-wrapper chalked ember-view\">{body}</div></div></section></body></html>";
            webview.IsBrowserInitializedChanged += (s, e) =>
            {
                if (webview.IsBrowserInitialized)
                {
                    webview.LoadHtml(html);
                }
            };
        }

        public Xenon.SlideAssembly.Project Project { get; private set; }

        private async Task PreviewXenonCode(LutheRun.LSBParser parser, string xenon_text)
        {
            // create a dummy project to give rendering previews
            try
            {

                Xenon.SlideAssembly.Project proj = new Xenon.SlideAssembly.Project(true);
                Project = proj;
                proj.SourceCode = xenon_text;

                var lra = element as LutheRun.ILoadResourceAsync;
                if (lra != null)
                {
                    await parser.LoadAssetsForElement(proj.CreateImageAsset, Xenon.Helpers.EnumerableExtensions.ItemAsEnumerable(element));
                }


                Xenon.Compiler.XenonBuildService builder = new Xenon.Compiler.XenonBuildService();
                var compiled = await builder.CompileProjectAsync(proj);
                if (compiled.success)
                {
                    var slides = await builder.RenderProjectAsync(compiled.project);

                    slidelist.Items.Clear();
                    foreach (var slide in slides.OrderBy(s => s.Number))
                    {
                        MegaSlidePreviewer previewer = new MegaSlidePreviewer();
                        previewer.Width = slidelist.Width;
                        previewer.Slide = slide;
                        slidelist.Items.Add(previewer);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private IElement ReplaceImages(IElement element)
        {
            var images = element?.QuerySelectorAll("img");

            if (element is null)
            {
                return null;
            }

            foreach (var img in images)
            {
                var s = img.GetAttribute("src");
                // replace source
                s = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(documentpath), s);
                img.SetAttribute("src", s);
            }
            return element;
        }

        private IElement RemoveScripts(IElement element)
        {
            var scripts = element?.QuerySelectorAll("script");
            foreach (var script in scripts)
            {
                element.RemoveElement(script);
            }

            return element;
        }

        private async void ClickReRender(object sender, RoutedEventArgs e)
        {
            await PreviewXenonCode(this.parser, textbox.GetAllText());
        }

        public string GetXenonText()
        {
            return textbox.GetAllText();
        }
    }
}
