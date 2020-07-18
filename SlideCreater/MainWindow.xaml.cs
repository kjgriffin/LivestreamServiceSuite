using SlideCreater.Compiler;
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
            compiler.Compile(TbInput.Text);



        }
    }
}
