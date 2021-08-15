using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Text;
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

        public void Setup(LutheRun.LSBParser parser, LutheRun.ILSBElement lsbelement, string docpath)
        {
            documentpath = docpath;
            element = lsbelement;
            this.parser = parser;
            // get the xenon command for the element
            string xenon_text = lsbelement.XenonAutoGen();
            // add text to textbox
            textbox.SetText(xenon_text);

            tbtype.Text = lsbelement.GetType().ToString();

            
            // create a dummy project to give rendering previews


            // create the web preview of the original
            string html = $"<!DOCTYPE html><html lang=\"en-us\">{RemoveScripts(parser.HTMLHead)?.OuterHtml}<body><section class=\"surface surface-XamlGeneratedNamespace-4\"><div class=\"bulletin-page lsml ember-view\"><div class=\"insertion-point-wrapper chalked ember-view\">{ReplaceImages(lsbelement.SourceHTML)?.OuterHtml}</div></div></section></body></html>";
            webview.NavigateToString(html);
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

    }
}
