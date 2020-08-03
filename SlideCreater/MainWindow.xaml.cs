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
            // put a system.drawing bitmap into image source

            Bitmap bmp = new Bitmap(1920, 1080);

            Graphics gfx = Graphics.FromImage(bmp);

            gfx.FillRectangle(System.Drawing.Brushes.Blue, 0, 0, 1920, 1080);

            // put it in image
            ImageBox.Source = bmp.ConvertToBitmapImage();




            // compile text
            XenonCompiler compiler = new XenonCompiler();
            Project proj = compiler.Compile(TbInput.Text);

            LiturgySlideRenderer sr = new LiturgySlideRenderer();
            sr.project = proj;

            slides = sr.Render(proj.Layouts.LiturgyLayout.GetRenderInfo());
            slide = 0;
        }

        List<Bitmap> slides;

        int slide = 0;

        private void Show(object sender, RoutedEventArgs e)
        {
            ImageBox.Source = slides[slide].ConvertToBitmapImage();
            if (slide + 1 < slides.Count)
            {
                slide += 1;
            }
        }
    }
}
