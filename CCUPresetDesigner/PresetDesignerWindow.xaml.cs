using CCUPresetDesigner.DataModel;
using CCUPresetDesigner.ViewModel;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Xenon.Helpers;

namespace CCUPresetDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CCUPresetDesignerWindow : Window
    {

        static List<CamPresetMk2> _defaultSource = new List<CamPresetMk2>
        {
            new CamPresetMk2
            {
                Id = "Test A",
                DefaultName = "Choir",
                Camera = "PULPIT",
                Description = "Something explainng when to use this (piano included)",
                ImgUrl = "",
                Pan = 102930124,
                Tilt = 65849,
                ZoomDir = ZoomDir.TELE,
                ZoomMs = 2330,
                Tags =new List<string>{ "pulpit", "choir", "piano", "wide", "start", "steps" },
            },
            new CamPresetMk2
            {
                Id = "Test B",
                DefaultName = "Sermon",
                Camera = "PULPIT",
                Description = "Close up for Astley sermon pulpit side",
                ImgUrl = "",
                Pan = 10293024,
                Tilt = 65848,
                ZoomDir = ZoomDir.WIDE,
                ZoomMs = 400,
                Tags =new List<string>{ "pulpit", "sermon", "close", "astley" },
            },
        };



        public CCUPresetDesignerWindow()
        {
            InitializeComponent();

            // load non-defaults
            _ = LoadResourcesTest();
        }

        private async Task LoadResourcesTest()
        {
            var json = await WebHelpers.DownloadText("https://raw.githubusercontent.com/kjgriffin/LivestreamServiceSuite/ccu-blob-data/blob-data/presets.json");
            var loadedPresets = JsonSerializer.Deserialize<List<CamPresetMk2>>(json);

            Dispatcher.Invoke(() =>
            {
                foreach (var pst in loadedPresets)
                {
                    var ctrl = new PresetDesign();
                    switch (pst.Camera.ToLower())
                    {
                        case "pulpit":
                            lbPulpit.Items.Add(ctrl);
                            break;
                        case "center":
                            lbCenter.Items.Add(ctrl);
                            break;
                        case "lectern":
                            lbLectern.Items.Add(ctrl);
                            break;
                    }
                    ctrl.Initialize(pst);
                }
            });
        }

    }
}