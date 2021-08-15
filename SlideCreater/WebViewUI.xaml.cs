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
using System.Windows.Shapes;

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for WebViewUI.xaml
    /// </summary>
    public partial class WebViewUI : Window
    {

        private string documentpath = "";

        public WebViewUI(string document)
        {
            InitializeComponent();

            documentpath = document;

            browser.Navigate(new Uri(@"file://" + document));

            ParseService();
        }


        private async void ParseService()
        {
            LutheRun.LSBParser parser = new LutheRun.LSBParser();
            await parser.ParseHTML(documentpath);

            parser.Serviceify();

            foreach (var serviceelement in parser.ServiceElements)
            {
                ElementPreview preview = new ElementPreview();
                preview.Setup(parser, serviceelement, documentpath);
                Viewbox view = new Viewbox();
                view.Stretch = Stretch.Uniform;
                view.Child = preview;
                Dispatcher.Invoke(() =>
                {
                    elementlist.Items.Add(view);
                });
            }

        }

    }
}
