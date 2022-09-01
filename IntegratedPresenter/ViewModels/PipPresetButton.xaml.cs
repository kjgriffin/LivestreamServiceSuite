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

    public delegate void ClickEventArgs();

    /// <summary>
    /// Interaction logic for PipPresetButton.xaml
    /// </summary>
    public partial class PipPresetButton : UserControl, INotifyPropertyChanged
    {
        public PipPresetButton()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event ClickEventArgs OnClick;

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
                var b = ((PIPPlacePreview)btn.Template.FindName("PIPPreview", btn));
                if (b != null)
                {
                    b.PIPPlace = value;
                }
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
                var b = ((PIPPlacePreview)btn.Template.FindName("PIPPreview", btn));
                if (b != null)
                {
                    b.PlaceName = value;
                }
                OnPropertyChanged();
            }
        }

        string _kscText = "KEY";
        public string KSCText
        {
            get => _kscText;
            set
            {
                _kscText = value;
                OnPropertyChanged();
            }
        }

        bool _kscVisible = false;
        public bool KSCVIsible
        {
            get => _kscVisible;
            set
            {
                _kscVisible = value;
                ksc_key.Visibility = value ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }



        bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                var b = ((PIPPlacePreview)btn.Template.FindName("PIPPreview", btn));
                if (b != null)
                {
                    b.ShowActive = value;
                }

                var bdr = ((Border)btn.Template.FindName("Border", btn));
                if (bdr != null)
                {
                    bdr.BorderBrush = value ? Brushes.Red : Brushes.White;
                }


                OnPropertyChanged();
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke();
        }

    }
}
