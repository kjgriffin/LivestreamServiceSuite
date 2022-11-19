using DirectShowLib;

using System;
using System.Collections.Generic;
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

namespace AutoTrackGUI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AutoTrackUIWPF : Window
    {
        Dictionary<string, int> devices = new Dictionary<string, int>();


        List<PIPTracker> pipTrackers = new List<PIPTracker>()
        {
            new PIPTracker
            {
                CenterX = 1920/2,
                CenterY = 1080/2,
                HWidth = 1920/2,
                HHeight = 1080/2,
            }
        };

        public AutoTrackUIWPF()
        {
            InitializeComponent();

            int i = 0;
            foreach (var cam in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                devices[cam.Name] = i;
                i++;
            }

            foreach (var item in devices)
            {
                cbVideoSources.Items.Add(item.Key);
            }

            capture.OnBoundingBoxesFound += Capture_OnBoundingBoxesFound;

        }

        private void Capture_OnBoundingBoxesFound(object? sender, (List<OpenCvSharp.Rect> objs, long seqNum) e)
        {
            // TODO: reset watchdog timers
            // TODO: consider adding watchdog timers for pipTrackers

            // update all trackers
            pipTrackers.ForEach(x =>
            {
                // update with tracks
                x.Update(e.objs.Select(b => new BoundingBox(b)).ToList(), e.seqNum);

            });

            Dispatcher.Invoke(() =>
            {

                trackOverlay.Children.Clear();
                foreach (var tracker in pipTrackers)
                {
                    // update UI
                    foreach (var track in tracker.GetActiveTracks())
                    {
                        Border b = new Border();
                        b.BorderBrush = Brushes.Green;
                        b.BorderThickness = new Thickness(2);
                        b.Width = track.Box.Width;
                        b.Height = track.Box.Height;
                        b.Background = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0));
                        trackOverlay.Children.Add(b);
                        Canvas.SetLeft(b, track.Box.CenterX - track.Box.Width / 2);
                        Canvas.SetTop(b, track.Box.CenterY - track.Box.Height / 2);
                        Label l = new Label();
                        l.Content = $"ID: {track.TrackID}";
                        l.Foreground = new SolidColorBrush(Colors.White);
                        l.FontSize = 40;
                        trackOverlay.Children.Add(l);
                        Canvas.SetLeft(l, track.Box.CenterX);
                        Canvas.SetTop(l, track.Box.CenterY);
                    }
                }

            });

        }

        private void ClickStart(object sender, RoutedEventArgs e)
        {
            if (cbVideoSources.SelectedIndex != -1)
            {
                // perhaps this is ok?
                capture.Start(cbVideoSources.SelectedIndex);
            }
        }

        private void ClickStop(object sender, RoutedEventArgs e)
        {
            capture?.Stop();
        }
    }
}
