using AngleSharp.Dom;
using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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
        Action<List<(string, string)>, string> _createProj;

        public WebViewUI(string document, Action<List<(string, string)>, string> CreateProject)
        {
            if (!Cef.IsInitialized)
            {
                Cef.EnableHighDPISupport();
                Cef.Initialize(new CefSettings(), performDependencyCheck: true, browserProcessHandler: null);
            }

            InitializeComponent();

            _createProj = CreateProject;

            documentpath = document;

            browser.IsBrowserInitializedChanged += (s, e) =>
            {
                Dispatcher.Invoke(new Action<object, DependencyPropertyChangedEventArgs>((sender, eargs) =>
                {
                    if (Cef.IsInitialized && browser.IsInitialized && (bool)eargs.NewValue)
                    {
                        try
                        {
                            browser.Load(document);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }), s, e);
            };

            try
            {
                Task.Run(() =>
                {
                    ParseService();
                });
            }
            catch (Exception)
            {
            }
        }


        private async void ParseService()
        {
            LutheRun.LSBParser parser = new LutheRun.LSBParser();
            await parser.ParseHTML(documentpath);

            parser.Serviceify();

            foreach (var serviceelement in parser.ServiceElements)
            {
                Dispatcher.Invoke(() =>
                {
                    ElementPreview preview = new ElementPreview();
                    preview.Setup(parser, serviceelement, documentpath);
                    Viewbox view = new Viewbox();
                    view.Stretch = Stretch.Uniform;
                    view.Child = preview;
                    elementlist.Items.Add(view);
                });
            }

        }

        private void ClickImportElements(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            List<Xenon.AssetManagment.ProjectAsset> assets = new List<Xenon.AssetManagment.ProjectAsset>();
            foreach (var ctrl in elementlist.Items)
            {
                var preview = ((Viewbox)ctrl)?.Child as ElementPreview;
                if (preview != null)
                {
                    assets.AddRange(preview.Project.Assets);
                    sb.AppendLine(preview.GetXenonText());
                }
            }
            // remove duplicate assets
            HashSet<string> uniquenames = new HashSet<string>();
            HashSet<string> uniqueassetpaths = new HashSet<string>();
            List<(string, string)> uniqueassets = new List<(string, string)>();
            foreach (var asset in assets)
            {
                if (!uniqueassetpaths.Contains(asset.CurrentPath))
                {
                    if (!uniquenames.Contains(asset.Name))
                    {
                        uniqueassets.Add((asset.CurrentPath, asset.Name));
                        uniquenames.Add(asset.Name);
                    }
                }
                uniqueassetpaths.Add(asset.CurrentPath);
            }

            _createProj(uniqueassets, sb.ToString());
        }

        private void OnClosed(object sender, EventArgs e)
        {
            foreach (var ctrl in elementlist.Items)
            {
                var c = ctrl as IDisposable;
                c?.Dispose();
            }
            elementlist.Items.Clear();
            //browser.Dispose();
        }
    }
}
