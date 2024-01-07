using CCU.Config;

using ICSharpCode.AvalonEdit.Search;

using Microsoft.Win32;

using SlideCreater.Controllers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

using UIControls;

using Xenon.SlideAssembly;

namespace SlideCreater.ViewControls
{
    /// <summary>
    /// Interaction logic for CCUEditorCtrl.xaml
    /// </summary>
    public partial class CCUEditorCtrl : UserControl
    {
        public event EventHandler OnTextEditDirty;
        IFullAccess<Project> _proj;

        internal CCUEditorCtrl(IFullAccess<Project> proj)
        {
            InitializeComponent();
            _proj = proj;

            TbConfigCCU.LoadLanguage_JSON();
            TbConfigCCU.Options.IndentationSize = 4;
            TbConfigCCU.Options.ConvertTabsToSpaces = true;
            TbConfigCCU.TextArea.TextView.LinkTextForegroundBrush = System.Windows.Media.Brushes.LawnGreen;
            TbConfigCCU.Background = new SolidColorBrush(Color.FromRgb(0x27, 0x27, 0x27));
            TbConfigCCU.Foreground = new SolidColorBrush(Color.FromRgb(0xea, 0xea, 0xea));
            TbConfigCCU.FontFamily = new FontFamily("cascadia code");
            TbConfigCCU.FontSize = 14;
            TbConfigCCU.ShowLineNumbers = true;
            TbConfigCCU.Padding = new Thickness(3);
            TbConfigCCU.IsReadOnly = true;

            SearchPanel.Install(TbConfigCCU.TextArea);

            var res = SanatizePNGsFromCfg(proj?.Value?.CCPUConfig);
            TbConfigCCU.Text = res.fake;

        }

        internal void UpdateFromProj()
        {
            var res = SanatizePNGsFromCfg(_proj?.Value?.CCPUConfig);
            TbConfigCCU.Text = res.fake;
        }

        #region CCU CONFIG

        CCUConfigEditor m_ccueditor;
        private void ClickOpenCCUConfigEditor(object sender, RoutedEventArgs e)
        {
            CCPUConfig_Extended cfg = null;
            if (string.IsNullOrEmpty(_proj.Value.SourceCCPUConfigFull))
            {
                cfg = new CCPUConfig_Extended();
            }
            else
            {
                cfg = JsonSerializer.Deserialize<CCPUConfig_Extended>(_proj.Value.SourceCCPUConfigFull);
            }

            if (m_ccueditor == null || m_ccueditor?.WasClosed == true)
            {
                m_ccueditor = new CCUConfigEditor(cfg, SaveCCUConfigChanges);
            }
            m_ccueditor.Show();
        }

        private void ClickLoadCCUFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open CCU Config File";
            ofd.Filter = "json (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(ofd.FileName))
                {
                    string jsonTxt = sr.ReadToEnd();

                    _proj.Value.SourceCCPUConfigFull = jsonTxt;
                }

                var fullCFG = JsonSerializer.Deserialize<CCPUConfig_Extended>(_proj.Value.SourceCCPUConfigFull);
                _proj.Value.CCPUConfig = fullCFG;

                var res = SanatizePNGsFromCfg(fullCFG);
                TbConfigCCU.Text = res.fake;
            }
        }

        private void SaveCCUConfigChanges(CCPUConfig_Extended cfg)
        {
            // build 2 copies of the JSON file...
            // 1 to display that will ignore the images to improve text loading...
            // 1 true copy to stuff into the project for rendering

            OnTextEditDirty?.Invoke(this, EventArgs.Empty);

            var res = SanatizePNGsFromCfg(cfg);

            _proj.Value.CCPUConfig = cfg;
            _proj.Value.SourceCCPUConfigFull = res.full;
            Dispatcher.Invoke(() =>
            {
                TbConfigCCU.Text = res.fake;
            });
        }

        private (string fake, string full) SanatizePNGsFromCfg(CCPUConfig_Extended cfg)
        {
            var trueCFG = JsonSerializer.Serialize(cfg);
            _proj.Value.SourceCCPUConfigFull = trueCFG;

            var fakeCFG = JsonSerializer.Deserialize<CCPUConfig_Extended>(trueCFG);

            foreach (var mock in fakeCFG.MockPresetInfo)
            {
                foreach (var mockpst in mock.Value)
                {
                    mockpst.Value.Thumbnail = "(base64encoded png)";
                }
            }

            return (JsonSerializer.Serialize(fakeCFG, new JsonSerializerOptions { WriteIndented = true }), trueCFG);
        }

        #endregion

        private void SourceTextChanged(object sender, EventArgs e)
        {
            OnTextEditDirty?.Invoke(this, EventArgs.Empty);
        }
    }
}
