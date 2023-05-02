using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Integrated_Presenter.BMDSwitcher.Mock
{
    /// <summary>
    /// Interaction logic for MockMV_SimpleOutput_PIP.xaml
    /// </summary>
    public partial class MockMV_SimpleOutput_PIP : UserControl
    {
        public MockMV_SimpleOutput_PIP()
        {
            InitializeComponent();
        }

        public void SetPIPName(string text)
        {
            tbPIPName.Text = text;
        }

        public void UpdateFromBrush(Brush brush)
        {
            imgStaticSource.Visibility = Visibility.Hidden;

            rectDisplay.Fill = brush;
            rectDisplay.Visibility = Visibility.Visible;
        }

        public void UpdateFromImage(BitmapImage img)
        {
            imgStaticSource.Source = img;
            imgStaticSource.Visibility = Visibility.Visible;
            rectDisplay.Fill = null;
            rectDisplay.Visibility = Visibility.Hidden;
        }
    }
}
