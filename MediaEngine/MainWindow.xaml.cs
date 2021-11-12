using MediaEngine.WPFPlayout.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TestHostWindow w = new TestHostWindow();
            w.Show();

            files = Directory.GetFiles(@"D:\hcav-onedrive\OneDrive - Holy Cross Lutheran Church\livestream-slides\2021-05-15\english\slides_auto").Where(x => System.IO.Path.GetExtension(x) == ".png" || System.IO.Path.GetExtension(x) == ".mp4").Select(x => (new Uri(x), Guid.NewGuid())).ToList();

            Task.Run(() => Prepare());
        }

        private async Task Prepare()
        {

            //await player.CueMedia(files.First().uri, files.First().id);
            //await player.CueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
            //index += 1;

            player.TryCueMedia(files.First().uri, files.First().id);
            await Task.Delay(1000);
            player.ShowCuedMedia(files[index].id);
            player.PlayCurrent();
            index++;
            player.TryCueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
            await Task.Delay(1000);
            //player.PutMediaOnAir(files.First().id);
        }

        List<(Uri uri, Guid id)> files = new List<(Uri, Guid)>();
        private int index = 0;

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {

                var s = player.ShowCuedMedia(files[index].id);
                player.PlayCurrent();
                index++;
                var f = files.Skip(index).First();
                var s1 = player.TryCueMedia(f.uri, f.id);

                if (s1 == WPFPlayout.CueRequestResult.CueRejected_NoAvailablePlayer)
                {
                    Debugger.Break();
                }


                //ffplayer.Close();
                //await ffplayer.Open(files.Skip(index).First().uri);
                //player.PutMediaOnAir(files.Skip(index - 1).First().id);
                //await player.CueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
                //await player.CueMedia(files.Skip(index + 1).First().uri, files.Skip(index + 1).First().id);
                //await player.CueMedia(files.Skip(index + 2).First().uri, files.Skip(index + 2).First().id);
            }
        }
    }
}
