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
            ShowSelected(false);
        }

        public async void ShowSlide()
        {
            ImgDisplay.Source = null;
            VideoDisplay.Source = null;
            if (Slide?.MediaType == MediaType.Image)
            {
                ImgDisplay.Source = Slide.Bitmap.ConvertToBitmapImage();
            }
            if (Slide?.MediaType == MediaType.Video)
            {
                VideoDisplay.Source = new Uri(Slide.AssetPath);
                VideoDisplay.Play();
                VideoDisplay.Volume = 0;
                await Task.Delay(5000);
                VideoDisplay.Pause();
            }
        }

        public void PlaySlide()
        {
            if (Slide.MediaType == MediaType.Video)
            {
                VideoDisplay.Play();
            }
        }

        public void Clear()
        {
            Slide = null;
            ShowSlide();
        }

        public void ShowSelected(bool isselected)
        {
            if (isselected)
            {
                SelectionBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                SelectionBorder.BorderBrush = Brushes.Cyan;
            }
            else
            {
                SelectionBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                SelectionBorder.BorderBrush = Brushes.Black;
            }
        }

        private void OnControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(() => OnSlideClicked?.Invoke(this, Slide));
        }
    }
}
