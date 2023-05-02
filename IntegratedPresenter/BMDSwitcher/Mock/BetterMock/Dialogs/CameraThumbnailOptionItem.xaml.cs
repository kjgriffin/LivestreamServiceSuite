using CommonGraphics;

using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Integrated_Presenter.BMDSwitcher.Mock.BetterMock.Dialogs
{
    /// <summary>
    /// Interaction logic for CameraThumbnailOptionItem.xaml
    /// </summary>
    public partial class CameraThumbnailOptionItem : UserControl
    {

        public event EventHandler<int> OnRequestSelection;
        int id = 0;

        BitmapImage _img;

        public CameraThumbnailOptionItem(BitmapImage img, int id)
        {
            InitializeComponent();

            _img = img;

            this.id = id;
            this.img.Source = img;
        }

        public void UpdateSelected(bool selected)
        {
            bdr.BorderBrush = selected ? Brushes.Red : Brushes.Transparent;
        }

        private void Click(object sender, MouseButtonEventArgs e)
        {
            OnRequestSelection?.Invoke(this, id);
        }

        public string GetImageAsBase64String()
        {
            string str64 = "";
            var frame = BitmapFrame.Create(_img);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                str64 = ms.ToBase64PngString();
            }

            return str64;
        }
    }
}
