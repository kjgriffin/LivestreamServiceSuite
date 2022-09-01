using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for PIPPlacePreview.xaml
    /// </summary>
    public partial class PIPPlacePreview : UserControl, INotifyPropertyChanged
    {
        public PIPPlacePreview()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        PIPPlaceSettings _pipPlace;
        public PIPPlaceSettings PIPPlace
        {
            get
            {
                return _pipPlace;
            }
            set
            {
                _pipPlace = value;
                pipscaletransform.ScaleX = value.ScaleX;
                pipscaletransform.ScaleY = value.ScaleY;
                piptranslatetransform.X = value.PosX;
                piptranslatetransform.Y = -value.PosY;

                // still have to do some math for the masks

                double width = 32;
                double height = 18;

                width = (width - value.MaskRight) + value.MaskLeft;
                height = (height - value.MaskBottom) + value.MaskTop;

                pipmaskclip.Rect = new Rect(value.MaskLeft, value.MaskTop, width, height);

                OnPropertyChanged();
            }
        }

        string _placeName = "UNKNOWN";
        public string PlaceName
        {
            get => _placeName;
            set
            {
                _placeName = value;
                OnPropertyChanged();
            }
        }

        bool _showActive = false;
        public bool ShowActive
        {
            get => _showActive;
            set
            {
                _showActive = value;

                if (value)
                {
                    pipbdr.Background = new SolidColorBrush(Color.FromArgb(65, 255, 0, 0));
                    rectpip.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }
                else
                {
                    pipbdr.Background = new SolidColorBrush(Color.FromArgb(65, 237, 175, 59));
                    rectpip.Fill = new SolidColorBrush(Color.FromArgb(255, 237, 175, 59));
                }

                OnPropertyChanged();
            }
        }

    }
}
