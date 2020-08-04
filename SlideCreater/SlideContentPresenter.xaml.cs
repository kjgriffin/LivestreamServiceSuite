using SlideCreater.Renderer;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public void ShowSlide()
        {
            if (Slide.MediaType == MediaType.Image)
            {
                ImgDisplay.Source = Slide.Bitmap.ConvertToBitmapImage();
            }
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
