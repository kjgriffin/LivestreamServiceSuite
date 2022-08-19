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
using System.Windows.Shapes;

using Xenon.Renderer;

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for Inspector.xaml
    /// </summary>
    public partial class Inspector : Window
    {
        public Inspector()
        {
            InitializeComponent();
        }

        private RenderedSlide _slide;
        public RenderedSlide Slide
        {
            get => _slide;
            set
            {
                _slide = value;
                this.slideView.Slide = _slide;
            }
        }

        public bool WasClosed { get; internal set; } = false;

        internal void ShowSlide(RenderedSlide renderedSlide, bool previewkeys)
        {
            Slide = renderedSlide;
            slideView.ShowSlide(previewkeys);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WasClosed = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isFull = WindowState == WindowState.Maximized;

            if (e.Key == Key.F11 || e.Key == Key.F)
            {
                if (isFull)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.None;
                }
            }

            else if (e.Key == Key.Escape)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }

        }
    }
}
