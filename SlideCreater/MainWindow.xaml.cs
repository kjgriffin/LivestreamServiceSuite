using SlideCreater.Compiler;
using SlideCreater.Renderer;
using SlideCreater.SlideAssembly;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CreaterEditorWindow : Window
    {
        public CreaterEditorWindow()
        {
            InitializeComponent();
        }

        private void Test_Button(object sender, RoutedEventArgs e)
        {
            // compile text
            XenonCompiler compiler = new XenonCompiler();
            Project proj = compiler.Compile(TbInput.Text);

            SlideRenderer sr = new SlideRenderer(proj);


            slides.Clear();
            for (int i = 0; i < proj.Slides.Count; i++)
            {
                slides.Add(sr.RenderSlide(i));
            }

            slidelist.Children.Clear();
            // add all slides to list
            foreach (var slide in slides)
            {
                SlideContentPresenter slideContentPresenter = new SlideContentPresenter();
                slideContentPresenter.Slide = slide;
                slideContentPresenter.ShowSlide();
                slidelist.Children.Add(slideContentPresenter);
            }
        }

        List<RenderedSlide> slides = new List<RenderedSlide>();

        int slide = 0;

        private void Show(object sender, RoutedEventArgs e)
        {
            FocusSlide.Slide = slides[slide];
            FocusSlide.ShowSlide();
            if (slide + 1 < slides.Count)
            {
                slide += 1;
            }
            else
            {
                slide = 0;
            }
        }
    }
}
