using AngleSharp;
using AngleSharp.Dom;

using LutheRun;
using LutheRun.Wizard;

using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
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
    /// Interaction logic for LSBImportWizardUI.xaml
    /// </summary>
    public partial class LSBImportWizardUI : Window
    {
        string m_serviceFilename;
        public LSBImportWizardUI(string filename)
        {
            InitializeComponent();
            m_serviceFilename = filename;

            Task.Run(LoadAndBuild);
        }

        List<IElement> elems = new List<IElement>();
        private async Task LoadAndBuild()
        {
            LSBParser parser = new LSBParser();
            parser.LSBImportOptions = new LSBImportOptions();
            await parser.ParseHTML(m_serviceFilename);
            parser.Serviceify(parser.LSBImportOptions);

            var rc = new LSBReCompiler();
            string tmpFile = System.IO.Path.GetTempFileName() + "lsbrecompile.html" + Guid.NewGuid().ToString() + ".html";

            rc.CompileToXenonMappedToSource(m_serviceFilename, parser.LSBImportOptions, parser.ServiceElements);

            await File.WriteAllTextAsync(tmpFile, rc.GenerateHTMLReport(parser.ServiceElements));

            Dispatcher.Invoke(() =>
            {

                browser.Source = new Uri(tmpFile);
            });
        }

    }
}
