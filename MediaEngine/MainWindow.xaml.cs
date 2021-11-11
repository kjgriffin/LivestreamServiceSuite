using System;
using System.Collections.Generic;
using System.IO;
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
            files = Directory.GetFiles(@"").Select(x => (new Uri(x), Guid.NewGuid())).ToList();

            Task.Run(() => Prepare());
        }

        private async Task Prepare()
        {

            await player.CueMedia(files.First().uri, files.First().id);
            await player.CueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
            index += 1;

            player.PutMediaOnAir(files.First().id);
        }

        List<(Uri uri, Guid id)> files = new List<(Uri, Guid)>();
        private int index = 1;

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                player.PutMediaOnAir(files.Skip(index-1).First().id);
                await player.CueMedia(files.Skip(index).First().uri, files.Skip(index).First().id);
                index++;
            }
        }
    }
}
