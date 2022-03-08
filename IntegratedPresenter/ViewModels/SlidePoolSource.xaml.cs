using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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

namespace IntegratedPresenter.Main
{
    /// <summary>
    /// Interaction logic for SlidePoolSource.xaml
    /// </summary>
    public partial class SlidePoolSource : UserControl
    {
        public SlidePoolSource()
        {
            InitializeComponent();
            mediapreview.OnMediaLoaded += Mediapreview_OnMediaLoaded;
            mediapreview.OnMediaPlaybackTimeUpdate += Mediapreview_OnMediaPlaybackTimeUpdate;
            mediapreview.videoPlayer.Volume = 0;
            mediapreview.AutoSilentPlayback = true;
            mediapreview.AutoSilentReplay = true;
            UpdateDurationUI();
        }

        private void Mediapreview_OnMediaPlaybackTimeUpdate(object sender, MediaPlaybackTimeEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateDurationUI();
            });
        }

        private void Mediapreview_OnMediaLoaded(object sender, MediaPlaybackTimeEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateDurationUI();
            });
        }

        private SlideType type = SlideType.Full;
        public bool Driven { get; set; } = true;
        public SlideType Type
        {
            get => type;
            set
            {
                type = value;
                if (Slide != null)
                {
                    Slide.Type = value;
                }
                switch (type)
                {
                    case SlideType.Full:
                        btnStill.Background = Brushes.Orange;
                        btnStill.Foreground = Brushes.Orange;
                        btnLiturgy.Background = Brushes.WhiteSmoke;
                        btnLiturgy.Foreground = Brushes.WhiteSmoke;
                        btnVideo.Background = Brushes.WhiteSmoke;
                        btnVideo.Foreground = Brushes.WhiteSmoke;
                        btnChromaStill.Background = Brushes.WhiteSmoke;
                        btnChromaStill.Foreground = Brushes.WhiteSmoke;
                        btnChromaVideo.Background = Brushes.WhiteSmoke;
                        btnChromaVideo.Foreground = Brushes.WhiteSmoke;
                        break;
                    case SlideType.Liturgy:
                        btnStill.Background = Brushes.WhiteSmoke;
                        btnStill.Foreground = Brushes.WhiteSmoke;
                        btnLiturgy.Background = Brushes.Orange;
                        btnLiturgy.Foreground = Brushes.Orange;
                        btnVideo.Background = Brushes.WhiteSmoke;
                        btnVideo.Foreground = Brushes.WhiteSmoke;
                        btnChromaStill.Background = Brushes.WhiteSmoke;
                        btnChromaStill.Foreground = Brushes.WhiteSmoke;
                        btnChromaVideo.Background = Brushes.WhiteSmoke;
                        btnChromaVideo.Foreground = Brushes.WhiteSmoke;
                        break;
                    case SlideType.Video:
                        btnStill.Background = Brushes.WhiteSmoke;
                        btnStill.Foreground = Brushes.WhiteSmoke;
                        btnLiturgy.Background = Brushes.WhiteSmoke;
                        btnLiturgy.Foreground = Brushes.WhiteSmoke;
                        btnVideo.Background = Brushes.Orange;
                        btnVideo.Foreground = Brushes.Orange;
                        btnChromaStill.Background = Brushes.WhiteSmoke;
                        btnChromaStill.Foreground = Brushes.WhiteSmoke;
                        btnChromaVideo.Background = Brushes.WhiteSmoke;
                        btnChromaVideo.Foreground = Brushes.WhiteSmoke;
                        break;
                    case SlideType.ChromaKeyStill:
                        btnStill.Background = Brushes.WhiteSmoke;
                        btnStill.Foreground = Brushes.WhiteSmoke;
                        btnLiturgy.Background = Brushes.WhiteSmoke;
                        btnLiturgy.Foreground = Brushes.WhiteSmoke;
                        btnVideo.Background = Brushes.WhiteSmoke;
                        btnVideo.Foreground = Brushes.WhiteSmoke;
                        btnChromaStill.Background = Brushes.Orange;
                        btnChromaStill.Foreground = Brushes.Orange;
                        btnChromaVideo.Background = Brushes.WhiteSmoke;
                        btnChromaVideo.Foreground = Brushes.WhiteSmoke;
                        break;
                    case SlideType.ChromaKeyVideo:
                        btnStill.Background = Brushes.WhiteSmoke;
                        btnStill.Foreground = Brushes.WhiteSmoke;
                        btnLiturgy.Background = Brushes.WhiteSmoke;
                        btnLiturgy.Foreground = Brushes.WhiteSmoke;
                        btnVideo.Background = Brushes.WhiteSmoke;
                        btnVideo.Foreground = Brushes.WhiteSmoke;
                        btnChromaStill.Background = Brushes.WhiteSmoke;
                        btnChromaStill.Foreground = Brushes.WhiteSmoke;
                        btnChromaVideo.Background = Brushes.Orange;
                        btnChromaVideo.Foreground = Brushes.Orange;
                        break;

                    case SlideType.Empty:
                        break;
                    default:
                        break;
                }
            }
        }
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

        public Slide Slide { get; private set; }

        private bool loaded = false;

        private void SelectedChanged()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (selected)
                {
                    border.BorderBrush = Brushes.Red;
                }
                else
                {
                    if (loaded)
                    {
                        border.BorderBrush = Brushes.LightBlue;
                    }
                    else
                    {
                        border.BorderBrush = Brushes.Gray;
                    }
                }

                if (loaded)
                {
                    BtnTakeInsert.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                    BtnTakeReplace.Background = Application.Current.FindResource("GrayLight") as RadialGradientBrush;
                }
                else
                {
                    BtnTakeInsert.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
                    BtnTakeReplace.Style = Application.Current.FindResource("SwitcherButton_Disabled") as Style;
                }
            });
        }

        private void ClickSlideMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Full;
            UpdateDurationUI();
        }

        private void ClickLiturgyMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Liturgy;
            UpdateDurationUI();
        }

        private void ClickVideoMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.Video;
            Dispatcher.Invoke(() =>
            {
                mediapreview.PlayMedia();
            });
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

                string file = ofd.FileName;
                var filename = Regex.Match(System.IO.Path.GetFileName(file), "\\d+_(?<type>[^-]*)-?(?<action>.*)\\..*");
                if (filename.Success)
                {
                    string name = filename.Groups["type"].Value;
                    string action = filename.Groups["action"].Value;
                    // look at the name to determine the type
                    switch (name)
                    {
                        case "Full":
                            Type = SlideType.Full;
                            break;
                        case "Liturgy":
                            Type = SlideType.Liturgy;
                            break;
                        case "Video":
                            Type = SlideType.Video;
                            break;
                        case "ChromaKeyVideo":
                            Type = SlideType.ChromaKeyVideo;
                            break;
                        case "ChromaKeyStill":
                            Type = SlideType.ChromaKeyStill;
                            break;
                        default:
                            Type = SlideType.Empty;
                            break;
                    }
                    Source = new Uri(ofd.FileName);
                    Slide = new Slide() { PreAction = action, Guid = Guid.NewGuid(), Source = ofd.FileName, Type = Type, };
                }
                else
                {
                    Source = new Uri(ofd.FileName);
                    string ext = System.IO.Path.GetExtension(ofd.FileName);
                    Type = SlideType.Full;
                    if (ext == ".mp4" || ext == ".MP4")
                    {
                        Type = SlideType.Video;
                    }
                    Slide = new Slide() { PreAction = "", Guid = Guid.NewGuid(), Source = ofd.FileName, Type = Type };
                }

                mediapreview.SetMedia(Source, Type);
                if (Slide.Type == SlideType.Video || Slide.Type == SlideType.ChromaKeyVideo)
                {
                    Dispatcher.Invoke(() =>
                    {
                        mediapreview.PlayMedia();
                    });
                    UpdateDurationUI();
                }

                // ask to open a key file

                // uses a default white key (fully opaque) if not

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select Alpha Key";
                openFileDialog.Filter = "Image (*.png)|*.png";
                if (openFileDialog.ShowDialog() == true)
                {
                    Slide.KeySource = openFileDialog.FileName; 
                }
                else
                {
                    Slide.KeySource = "pack://application:,,,/Keys/WhiteKey.png";
                }


                BtnTakeInsert.Style = (Style)Application.Current.FindResource("SwitcherButton");
                BtnTakeReplace.Style = (Style)Application.Current.FindResource("SwitcherButton");
                loaded = true;

                border.BorderBrush = Brushes.LightBlue;
                UpdateDurationUI();

            }
        }

        private void UpdateDurationUI()
        {
            if ((Slide?.Type == SlideType.Video || Slide?.Type == SlideType.ChromaKeyVideo ) && mediapreview.MediaLength != TimeSpan.Zero)
            {
                tbDuration.Text = mediapreview.MediaLength.ToString("\\T\\-mm\\:ss");
                tbDuration.Visibility = Visibility.Visible;
            }
            else
            {
                tbDuration.Visibility = Visibility.Hidden;
                tbDuration.Text = "";
            }
        }

        private void ClickTakeInsert(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Selected = true;
                TakeSlidePoolSource?.Invoke(this, Slide, false, Driven);
            }
        }

        private void ClickTakeReplace(object sender, RoutedEventArgs e)
        {
            if (loaded)
            {
                Selected = true;
                TakeSlidePoolSource?.Invoke(this, Slide, true, Driven);
            }
        }


        public void ShowHideShortcuts(bool show)
        {
            if (show)
            {
                ksc1.Visibility = Visibility.Visible;
                ksc2.Visibility = Visibility.Visible;
            }
            else
            {
                ksc1.Visibility = Visibility.Collapsed;
                ksc2.Visibility = Visibility.Collapsed;
            }
        }

        //public event EventHandler ClickTakeEvent;
        public event TakeSlidePoolEvent TakeSlidePoolSource;

        private string sourceIdNum = "#";
        public string SourceIDNum
        {
            get => sourceIdNum;
            set {
                sourceIdNum = value;
                tbNum1.Text = sourceIdNum;
                tbNum2.Text = sourceIdNum;
            }
        } 

        private void ClickChromaStillMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.ChromaKeyStill;
            UpdateDurationUI();
        }

        private void ClickChromaVideoMode(object sender, RoutedEventArgs e)
        {
            Type = SlideType.ChromaKeyVideo;
            UpdateDurationUI();
            Dispatcher.Invoke(() =>
            {
                mediapreview.PlayMedia();
            });
        }

        private void ClickToggleDrive(object sender, RoutedEventArgs e)
        {
            Driven = !Driven;
            if (Driven)
            {
                btnDriven.Background = Brushes.Orange;
                btnDriven.Foreground = Brushes.Orange;
            }
            else
            {
                btnDriven.Background = Brushes.WhiteSmoke;
                btnDriven.Foreground = Brushes.WhiteSmoke;
            }
        }
    }

    public delegate void TakeSlidePoolEvent(object sender, Slide s, bool replaceMode, bool driven);

}
