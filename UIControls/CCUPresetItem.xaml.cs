using CCU.Config;

using CommonGraphics;

using Microsoft.Win32;

using SixLabors.ImageSharp.Formats.Tga;

using System;
using System.Collections.Generic;
using System.IO;
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

namespace UIControls
{
    /// <summary>
    /// Interaction logic for CCUPresetItem.xaml
    /// </summary>
    public partial class CCUPresetItem : UserControl
    {

        CombinedPresetInfo m_cfgOrig;

        string _thumbnail = "";

        public event EventHandler<CCUPresetItem> OnDeleteRequest;
        public event EventHandler<EventArgs> OnEditsChanged;

        bool _isInit = false;

        public CCUPresetItem(CombinedPresetInfo cfg)
        {
            InitializeComponent();

            m_cfgOrig = cfg;

            tbCamName.Text = cfg.CamName;
            tbPstName.Text = cfg.PresetPosName;
            tbPan.Text = cfg.Pan.ToString();
            tbTilt.Text = cfg.Tilt.ToString();
            tbValid.Text = cfg.Valid ? "true" : "false";
            tbZoomPst.Text = cfg.ZoomPresetName;
            tbZoomMode.Text = cfg.ZoomMode;
            tbZoomMS.Text = cfg.ZoomMS.ToString();
            tbMoveMS.Text = cfg.MoveMS.ToString();

            _thumbnail = cfg.Thumbnail;

            if (!string.IsNullOrEmpty(_thumbnail))
            {
                imgThumbnail.Source = _thumbnail.ToBitmapImage();
            }
            /*
            try
            {
                imgThumbnail.Source = new BitmapImage(new Uri(cfg.Thumbnail));
            }
            catch (Exception)
            {
            }
            */

            _isInit = true;
        }

        internal CombinedPresetInfo GetInfo()
        {
            int.TryParse(tbZoomMS.Text, out int zms);
            int.TryParse(tbMoveMS.Text, out int rms);

            return new CombinedPresetInfo
            {
                CamName = tbCamName.Text,
                PresetPosName = tbPstName.Text,
                Pan = m_cfgOrig.Pan,
                Tilt = m_cfgOrig.Tilt,
                Valid = m_cfgOrig.Valid,
                ZoomPresetName = tbZoomPst.Text,
                ZoomMode = tbZoomMode.Text,
                ZoomMS = zms,
                MoveMS = rms,
                Thumbnail = _thumbnail
            };
        }

        private void ClickChangeThumbnail(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose Thumbnail Image";
            ofd.Filter = "PNG (*.png)|*.png";
            if (ofd.ShowDialog() == true)
            {
                var img = new BitmapImage(new Uri(ofd.FileName));
                var frame = BitmapFrame.Create(img);

                imgThumbnail.Source = img;

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);

                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    var str64 = ms.ToBase64PngString();
                    _thumbnail = str64;
                }
            }

            OnEditsChanged?.Invoke(this, new EventArgs());
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInit)
            {
                OnEditsChanged?.Invoke(this, new EventArgs());
            }
        }

        private void ClickDeletePreset(object sender, RoutedEventArgs e)
        {
            OnDeleteRequest?.Invoke(this, this);
        }
    }
}
