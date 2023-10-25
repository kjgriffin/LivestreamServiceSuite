using Microsoft.Win32;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for BibleClassWizzard.xaml
    /// </summary>
    public partial class BibleClassWizzard : Window
    {

        public class BibleClassData
        {
            public string Title { get; set; }
            public string Presenter { get; set; }
            public string ThumbnailPath { get; set; }
            public DateTime Date { get; set; }

            /// <summary>
            /// False = at top
            /// </summary>
            public bool InsertAtCarret { get; set; }
            /// <summary>
            /// False = with source
            /// </summary>
            public bool PanelAtBottom { get; set; }
        }

        public BibleClassData Data { get; private set; }

        public BibleClassWizzard()
        {
            InitializeComponent();

            // setup defaults
            tbTITLE.Text = "";
            tbPRESENTER.Text = "Pastor Nolan Astley";

            DateTime nextSunday = DateTime.Now;
            nextSunday = nextSunday.AddDays(7 - (int)nextSunday.DayOfWeek);
            tbDATE.SelectedDate = nextSunday;
        }

        private void Click_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        private void Click_Ok(object sender, RoutedEventArgs e)
        {
            Data = new BibleClassData
            {
                Title = tbTITLE.Text,
                Presenter = tbPRESENTER.Text,
                Date = tbDATE.SelectedDate.Value,
                ThumbnailPath = thumbnailPath,
                InsertAtCarret = rbInsertCarret.IsChecked.Value,
                PanelAtBottom = rbPanelsEnd.IsChecked.Value,
            };

            DialogResult = true;
            this.Close();
        }

        string thumbnailPath = "";
        private void Border_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var first = files?.FirstOrDefault();
                if (!string.IsNullOrEmpty(first))
                {
                    thumbnailPath = first;
                    imgThumbnail.Source = new BitmapImage(new Uri(thumbnailPath));
                }
            }
        }

        private void Click_Thumbnail(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Add Image Asset";
            ofd.Filter = "Images (*.png;*.jpg;*.bmp;)|*.png;*.jpg;*.bmp;";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                thumbnailPath = ofd.FileName;
                imgThumbnail.Source = new BitmapImage(new Uri(thumbnailPath));
            }
        }
    }
}
