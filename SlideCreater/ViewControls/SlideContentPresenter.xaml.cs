using System;
using System.Collections.Generic;
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

using Xenon.Renderer;
using Xenon.SlideAssembly;
using Xenon.Helpers;

namespace SlideCreater
{

    public delegate void SlideClickedEvent(object sender, RenderedSlide slide);
    /// <summary>
    /// Interaction logic for SlideContentPresenter.xaml
    /// </summary>
    public partial class SlideContentPresenter : UserControl
    {

        public event SlideClickedEvent OnSlideClicked;
        public RenderedSlide Slide { get; set; }

        public SlideContentPresenter()
        {
            InitializeComponent();
        }

        public void ShowSlide()
        {
            ImgDisplay.Source = null;
            VideoDisplay.Source = null;
            if (Slide?.MediaType == MediaType.Image)
            {
                VideoDisplay.Visibility = Visibility.Hidden;
                textDisplay.Visibility = Visibility.Hidden;
                ImgDisplay.Visibility = Visibility.Visible;
                ImgDisplay.Source = Slide.Bitmap.ConvertToBitmapImage();
            }
            if (Slide?.MediaType == MediaType.Video)
            {
                ImgDisplay.Visibility = Visibility.Hidden;
                textDisplay.Visibility = Visibility.Hidden;
                VideoDisplay.Visibility = Visibility.Visible;
                VideoDisplay.Source = new Uri(Slide.AssetPath);
                VideoDisplay.MediaEnded += VideoDisplay_MediaEnded;
                VideoDisplay.Volume = 0;
                VideoDisplay.Play();
            }
            if (Slide?.MediaType == MediaType.Audio)
            {
                VideoDisplay.Visibility = Visibility.Hidden;
                textDisplay.Visibility = Visibility.Visible;
                ImgDisplay.Visibility = Visibility.Visible;
                ImgDisplay.Source = new BitmapImage(new Uri("pack://application:,,,/ViewControls/Images/musicnote.png"));
                textDisplay.Text = Slide.Name + Slide.CopyExtension;
            }
            if (Slide?.MediaType == MediaType.Text)
            {
                VideoDisplay.Visibility = Visibility.Hidden;
                ImgDisplay.Visibility = Visibility.Hidden;
                textDisplay.Visibility = Visibility.Visible;
                textDisplay.Text = Slide.Text;
            }
        }

        private void VideoDisplay_MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoDisplay.Stop();
            VideoDisplay.Volume = 0;
            VideoDisplay.Play();
        }

        public void PlaySlide()
        {
            if (Slide?.MediaType == MediaType.Video)
            {
                VideoDisplay.Play();
            }
        }

        public void Clear()
        {
            Slide = null;
            textDisplay.Text = "Empty Slide";
            textDisplay.Visibility = Visibility.Visible;
            ImgDisplay.Visibility = Visibility.Hidden;
            VideoDisplay.Visibility = Visibility.Hidden;
            ShowSlide();
        }

    }
}
