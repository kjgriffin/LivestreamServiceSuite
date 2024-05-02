using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Integrated_Presenter.Windows
{

    internal interface IAuxOverride
    {
        void HideAuxSource();
        void ShowAuxSource(VisualBrush brush);
    }

    /// <summary>
    /// Interaction logic for AuxPlayer.xaml
    /// </summary>
    public partial class AuxPlayer : Window
    {
        DispatcherTimer playbackTimer;

        internal IAuxOverride AuxDisplay { get; set; }

        public AuxPlayer()
        {
            InitializeComponent();
            playbackTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Render, playbackTimer_Tick, this.Dispatcher);
            player.UnloadedBehavior = System.Windows.Controls.MediaState.Manual;
            player.MediaEnded += Player_MediaEnded;
            player.MediaOpened += Player_MediaOpened;
            player.ScrubbingEnabled = true;
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            // reset timers etc.
            scrubber.Minimum = 0;
            scrubber.Maximum = player.NaturalDuration.TimeSpan.TotalSeconds;
            scrubber.Value = scrubber.Minimum;
        }


        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (Loop)
            {
                player.Position = TimeSpan.Zero;
                player.Play();
            }
        }

        bool IsScrubbing { get; set; } = false;
        private void playbackTimer_Tick(object sender, EventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan && !IsScrubbing)
            {
                var pos = player.Position.TotalSeconds;
                scrubber.Value = pos;
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            // load file??
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                lbFiles.Items.Clear();
                foreach (var file in files)
                {
                    lbFiles.Items.Add(file);
                }
                // open the first
                var first = files.Select(x => Path.GetFullPath(x)).FirstOrDefault(x => Path.GetExtension(x) == ".mp4");
                player.Source = new Uri(first);
            }
        }

        private void Click_Play(object sender, RoutedEventArgs e)
        {
            player.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
            player.Play();
        }

        private void Click_Pause(object sender, RoutedEventArgs e)
        {
            player.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
            player.Pause();
        }

        bool Loop { get; set; }
        private void Click_Loop(object sender, RoutedEventArgs e)
        {
            Loop = !Loop;
        }

        private void scrubber_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            IsScrubbing = true;
            player.Pause();
        }

        private void scrubber_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            IsScrubbing = false;
        }

        private void scrubber_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (player.NaturalDuration.HasTimeSpan)
            {
                player.Position = TimeSpan.FromSeconds(Math.Clamp(scrubber.Value, 0, player.NaturalDuration.TimeSpan.TotalSeconds));
            }
        }

        private void Click_Show(object sender, RoutedEventArgs e)
        {
            AuxDisplay?.ShowAuxSource(new VisualBrush(this.player));
        }

        private void Click_Hide(object sender, RoutedEventArgs e)
        {
            AuxDisplay?.HideAuxSource();
        }
    }
}
