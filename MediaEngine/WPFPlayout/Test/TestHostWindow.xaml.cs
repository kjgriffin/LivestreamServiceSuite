using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace MediaEngine.WPFPlayout.Test
{
    /// <summary>
    /// Interaction logic for TestHostWindow.xaml
    /// </summary>
    public partial class TestHostWindow : Window
    {

        TestDisplayWindow displayWindow = new TestDisplayWindow();
        public TestHostWindow()
        {
            InitializeComponent();
            files = Directory.GetFiles(@"D:\hcav-onedrive\OneDrive - Holy Cross Lutheran Church\livestream-slides\2021-05-15\english\slides_auto")
                .Where(x => System.IO.Path.GetExtension(x) == ".png" || System.IO.Path.GetExtension(x) == ".mp4")
                .OrderBy(x =>
                {
                    var n = Regex.Match(System.IO.Path.GetFileNameWithoutExtension(x), "^(\\d+)").Groups[0].Value;
                    if (int.TryParse(n, out int i))
                    {
                        return i;
                    }
                    return int.MaxValue;
                })
                .Select(x => (new Uri(x), Guid.NewGuid()))
                .ToList();
            displayWindow.Show();

            displayWindow.player.OnMediaPlaybackPositionChanged += Player_OnMediaPlaybackPositionChanged;

            Task.Run(() => Prepare());
        }

        private void Player_OnMediaPlaybackPositionChanged(TimeSpan currentPos, TimeSpan duration)
        {
            tbtimer.Dispatcher.Invoke(() => tbtimer.Text = (duration - currentPos).ToString("hh\\:mm\\:ss\\.ff"));
        }

        private async Task Prepare()
        {

            //await player.CueMedia(files.First().uri, files.First().id);
            //await player.CueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
            //index += 1;

            displayWindow.player.TryCueMedia(files.First().uri, files.First().id);
            await Task.Delay(1000);
            displayWindow.player.ShowCuedMedia(files[index].id);
            displayWindow.player.PlayCurrent();
            display1.Dispatcher.Invoke(() => display1.Fill = displayWindow.player.GetOnAirVisual());
            index++;
            displayWindow.player.TryCueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
            await Task.Delay(1000);
            display2.Dispatcher.Invoke(() => display2.Fill = displayWindow.player.GetVisualForCueRequest(files.Skip(index).First().id));
            //player.PutMediaOnAir(files.First().id);
        }

        List<(Uri uri, Guid id)> files = new List<(Uri, Guid)>();
        private int index = 0;

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {

                var s = displayWindow.player.ShowCuedMedia(files[index].id);
                displayWindow.player.PlayCurrent();
                display1.Dispatcher.Invoke(() => display1.Fill = displayWindow.player.GetOnAirVisual());
                index++;
                var f = files.Skip(index).First();
                var f1 = files.Skip(index + 1).First();
                var s1 = displayWindow.player.TryCueMedia(f.uri, f.id);
                var s2 = displayWindow.player.TryCueMedia(f1.uri, f1.id);
                var s3 = displayWindow.player.TryCueMedia(displayWindow.player.BlackSource.source, displayWindow.player.BlackSource.id);    
                display2.Dispatcher.Invoke(() => display2.Fill = displayWindow.player.GetVisualForCueRequest(files[index].id));
                display3.Dispatcher.Invoke(() => display3.Fill = displayWindow.player.GetVisualForCueRequest(files[index + 1].id));
                display4.Dispatcher.Invoke(() => display4.Fill = displayWindow.player.GetVisualForCueRequest(displayWindow.player.BlackSource.id));

                if (s1 == WPFPlayout.CueRequestResult.CueRejected_NoAvailablePlayer)
                {
                    Debugger.Break();
                }

            }
            if (e.Key == Key.Up)
            {
                displayWindow.player.PlayCurrent();
            }
            if (e.Key == Key.Down)
            {
                displayWindow.player.PauseCurrent();
            }
            if (e.Key == Key.H)
            {
                displayWindow.player.SetPlaybackPositionTimerResolution(HW4PoolMediaPlayer.TimerMode.HighRes);
            }
            if (e.Key == Key.L)
            {
                displayWindow.player.SetPlaybackPositionTimerResolution(HW4PoolMediaPlayer.TimerMode.LowRes);
            }
        }
    }
}
