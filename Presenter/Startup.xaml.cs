using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presenter
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup()
        {
            InitializeComponent();
        }

        List<(string path, SlideType type)> _slides;

        private void Select_Presentation_Folder(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Presentation Files";
            if (ofd.ShowDialog() == true)
            {
                try
                {

                    FolderName.Content = System.IO.Path.GetDirectoryName(ofd.FileName);
                    _slides = new List<(string path, SlideType type)>();
                    Start.IsEnabled = true;
                    // get all image and video files
                    foreach (var file in Directory.GetFiles(System.IO.Path.GetDirectoryName(ofd.FileName)).OrderBy(s => int.Parse(Regex.Match(System.IO.Path.GetFileName(s), @"(?<order>\d+)_.*").Groups["order"].Value)))
                    {
                        // TODO: validate image files
                        _slides.Add((file, System.IO.Path.GetExtension(file) == ".mp4" ? SlideType.Video : SlideType.Image));
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable To Load Slides", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Start_Presentation(object sender, RoutedEventArgs e)
        {
            // sort slides to presentation order
            PresenterWindow display = new PresenterWindow(_slides);
            display.Show();
        }
    }
}
