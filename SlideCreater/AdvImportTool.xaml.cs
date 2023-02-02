using AngleSharp.Dom;

using LutheRun.Parsers;

using System;
using System.IO;
using IO = System.IO;
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

using Xenon.SlideAssembly;
using LutheRun.Wizard;

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for AdvImportTool.xaml
    /// </summary>
    public partial class AdvImportTool : Window
    {
        public AdvImportTool(string filename, LSBImportOptions defaultOpts)
        {
            InitializeComponent();
            InitializeComponent();
            m_serviceFilename = filename;

            Task.Run(LoadAndBuild);
            this.options = defaultOpts;
        }

        LSBImportOptions options;
        string m_serviceFilename;

        List<IElement> elems = new List<IElement>();

        private async Task LoadAndBuild()
        {
            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = options;
            await parser.ParseHTML(m_serviceFilename);
            parser.Serviceify(parser.LSBImportOptions);

            string tmpFile = IO.Path.GetTempFileName() + "lsbrecompile.html" + Guid.NewGuid().ToString() + ".html";


            parser.LSBImportOptions.Macros = IProjectLayoutLibraryManager.GetDefaultBundledLibraries().FirstOrDefault(x => x.LibName == "Xenon.CommonColored")?.Macros ?? new Dictionary<string, string>();
            parser.CompileToXenon();

            List<string> cssFiles = new List<string>();
            try
            {
                var path = IO.Path.GetDirectoryName(m_serviceFilename);
                var chromeDownload = IO.Path.Combine(path, IO.Path.GetFileNameWithoutExtension(m_serviceFilename) + "_files");

                var files = IO.Directory.GetFiles(chromeDownload);
                // find the app-guid.css file
                //var file = files.First(f => IO.Path.GetFileName(f).StartsWith("app-") && f.EndsWith(".css"));
                var cssfiles = files.Where(f => IO.Path.GetExtension(f) == ".css");

                foreach (var file in cssfiles)
                {
                    var tmpCss = IO.Path.GetTempFileName() + "lsbrecompile.css" + Guid.NewGuid().ToString() + ".css";
                    File.Copy(file, tmpCss);
                    cssFiles.Add(tmpCss);
                }

            }
            catch (Exception ex)
            {
                throw;
            }

            var rc = new LSBReCompiler();
            await File.WriteAllTextAsync(tmpFile, rc.GenerateHTMLReport(parser.ServiceElements, cssFiles.ToArray()));


            Dispatcher.Invoke(() =>
            {
                browser.Source = new Uri(tmpFile);
            });
        }


    }
}
