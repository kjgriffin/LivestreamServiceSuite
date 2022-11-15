using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            imgStaticSource.Source= img;
            imgStaticSource.Visibility = Visibility.Visible;
            rectDisplay.Fill = null;
            rectDisplay.Visibility = Visibility.Hidden;
        }
    }
}
