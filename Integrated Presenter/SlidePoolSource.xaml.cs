using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Integrated_Presenter
{
    /// <summary>
    /// Interaction logic for SlidePoolSource.xaml
    /// </summary>
    public partial class SlidePoolSource : UserControl
    {
        public SlidePoolSource()
        {
            InitializeComponent();
        }

        public SlideType Type = SlideType.Full;
        public Uri Source;
        private bool selected = false;
        public bool Selected
        {
            get => selected; set
            {
                selected = value;
                SelectedChanged();
            }
        }

        private bool loaded = false;

        private void SelectedChanged()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (selected)
                {
                    BtnTake.Background = Application.Current.FindResource("RedLight") as RadialGradientBrush;
                }
                else
                {
                    if (loaded)
                    {
                        BtnTake.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    }
                    else
                    {
                        BtnTake.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
                    }
                }
            });
        }

        private void ClickSlideMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Full;
        }

        private void ClickLiturgyMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Liturgy;
        }

        private void ClickVideoMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Video;
        }

        public void PlayMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.PlayMedia();
        }

        public void PauseMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.PauseMedia();
        }

        public void StopMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.StopMedia();
        }

        public void RestartMedia()
        {
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.ReplayMedia();
        }

        private void ClickLoadMedia(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Media File";
            ofd.Filter = "Images and Video(*.mp4;*.png)|*.mp4;*.png";
            if (ofd.ShowDialog() == true)
            {
                Source = new Uri(ofd.FileName);
                mediapreview.SetMedia(Source, Type);
                BtnTake.Style = (Style)Application.Current.FindResource("SwitcherButton");
                loaded = true;
                string ext = System.IO.Path.GetExtension(ofd.FileName);
                if (ext == ".mp4" || ext == ".MP4")
                {
                    Type = SlideType.Video;
                    rbvideo.IsChecked = true;
                }
            }
        }

        private void ClickTake(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Selected = !Selected;
                ClickTakeEvent?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler ClickTakeEvent;


    }

}
