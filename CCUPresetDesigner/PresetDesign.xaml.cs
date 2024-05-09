using CCUPresetDesigner.DataModel;
using CCUPresetDesigner.ViewModel;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CCUPresetDesigner
{
    /// <summary>
    /// Interaction logic for PresetDesign.xaml
    /// </summary>
    public partial class PresetDesign : UserControl
    {
        private CamPresetMk2 Original;
        public PresetDesign()
        {
            //original = src;
            this.DataContext = this;
            InitializeComponent();
        }

        public void Initialize(CamPresetMk2 original)
        {
            Original = original;
            // use original? to init??
            tbCamera.Text = Original?.Camera ?? string.Empty;

            tbPan.Text = Original?.Pan.ToString() ?? string.Empty;
            tbTilt.Text = Original?.Tilt.ToString() ?? string.Empty;

            tbZoomDir.Text = Original?.ZoomDir.ToString() ?? string.Empty;
            tbZoomMs.Text = Original?.ZoomMs.ToString() ?? string.Empty;

            tbName.Text = Original?.DefaultName ?? string.Empty;

            if (Uri.TryCreate(Original?.ImgUrl, UriKind.Absolute, out var uri))
            {
                imgThumbnail.Source = new BitmapImage(uri);
            }

            tbId.Text = Original?.Id?.ToString() ?? string.Empty;
            tbInfo.Text = Original?.Description?.ToString() ?? string.Empty;
            tbTags.Text = string.Join(", ", Original?.Tags ?? new List<string>());
        }

    }
}
