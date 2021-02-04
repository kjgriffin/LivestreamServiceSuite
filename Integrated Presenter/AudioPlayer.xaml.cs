using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter
{
    /// <summary>
    /// Interaction logic for AudioPlayer.xaml
    /// </summary>
    public partial class AudioPlayer : Window
    {

        MainWindow _parent;

        public AudioPlayer(MainWindow parent)
        {
            _parent = parent;
            InitializeComponent();
            ShowHideShortcuts(parent.ShowShortcuts);
            audioplayer.LoadedBehavior = MediaState.Manual;
            audioplayer.MediaOpened += Audioplayer_MediaOpened;
            PlaybackTimer.Elapsed += PlaybackTimer_Elapsed;
        }

        public void ShowHideShortcuts(bool show)
        {
            Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    ksc_1.Visibility = Visibility.Visible;
                    ksc_2.Visibility = Visibility.Visible;
                    ksc_3.Visibility = Visibility.Visible;
                    ksc_4.Visibility = Visibility.Visible;
                }
                else
                {
                    ksc_1.Visibility = Visibility.Collapsed;
                    ksc_2.Visibility = Visibility.Collapsed;
                    ksc_3.Visibility = Visibility.Collapsed;
                    ksc_4.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void PlaybackTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    TbAudioTimeRemaining.Text = (audioplayer.NaturalDuration - audioplayer.Position).TimeSpan.ToString("mm\\:ss");
                    TbAudioTimeCurrent.Text = audioplayer.Position.ToString("mm\\:ss");
                    TbAudioTimeDurration.Text = audioplayer.NaturalDuration.TimeSpan.ToString("mm\\:ss");
                }
                catch (Exception)
                {
                }
            });
        }

        private void Audioplayer_MediaOpened(object sender, RoutedEventArgs e)
        {
        }

        public Uri AudioSource { get; set; }
        public string Filename { get; set; }

        private Timer PlaybackTimer = new Timer();

        private void ClickLoadAudio(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                string filename = ofd.FileName;
                openAudio(filename);
            }
        }

        private void openAudio(string filename)
        {
            try
            {
                tbFileName.Text = filename;
                AudioSource = new Uri(filename);
                audioplayer.Source = AudioSource;
            }
            catch (Exception)
            {
            }
            Dispatcher.Invoke(() =>
            {
                StopAudio();
                PlaybackTimer.Interval = 500;
                PlaybackTimer.Start();
                BtnPlayAudio.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnPauseAudio.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnStopAudio.Style = Application.Current.FindResource("SwitcherButton") as Style;
                BtnRestartAudio.Style = Application.Current.FindResource("SwitcherButton") as Style;
            });

        }

        public void OpenAudio(string filename)
        {
            openAudio(filename);
        }

        private void ClickRestartAudio(object sender, RoutedEventArgs e)
        {
            RestartAudio();
        }

        private void ClickStopAudio(object sender, RoutedEventArgs e)
        {
            StopAudio();
        }

        private void ClickPauseAudio(object sender, RoutedEventArgs e)
        {
            PauseAudio();
        }

        private void ClickPlayAudio(object sender, RoutedEventArgs e)
        {
            PlayAudio();
        }


        public void RestartAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioplayer.Stop();
                PlayAudio();
            });
        }

        public void StopAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioplayer.Stop();
            });
        }

        public void PauseAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioplayer.Pause();
            });
        }

        public void PlayAudio()
        {
            Dispatcher.Invoke(() =>
            {
                audioplayer.Play();
            });
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _parent.Focus();
            }

            if (e.Key == Key.F1)
            {
                RestartAudio();
            }
            if (e.Key == Key.F2)
            {
                StopAudio();
            }
            if (e.Key == Key.F3)
            {
                PauseAudio();
            }
            if (e.Key == Key.F4)
            {
                PlayAudio();
            }
            e.Handled = true;
        }
    }
}
