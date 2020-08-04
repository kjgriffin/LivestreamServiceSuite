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
    /// <summary>
    /// Interaction logic for SlideContentPresenter.xaml
    /// </summary>
    public partial class SlideContentPresenter : UserControl
    {

        public RenderedSlide Slide { get; set; }

        public SlideContentPresenter()
        {
            InitializeComponent();
        }

        public void ShowSlide()
        {
            if (Slide.MediaType == MediaType.Image)
            {
                ImgDisplay.Source = Slide.Bitmap.ConvertToBitmapImage(); 
            }
        }

    }
}
