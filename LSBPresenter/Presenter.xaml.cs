using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace LSBPresenter
{

    enum WindowState
    {
        Fullscreen,
        Windowed,
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Presenter : Window
    {
        public Presenter()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                if (WindowState == System.Windows.WindowState.Maximized)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = System.Windows.WindowState.Normal;
                }
                else
                {
                    // force fullscreen 
                    WindowStyle = WindowStyle.None;
                    WindowState = System.Windows.WindowState.Maximized;
                }
            }
            if (e.Key == Key.Escape)
            {
                // exit fullscreen
                WindowState = System.Windows.WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            if (e.Key == Key.O)
            {
                openProject();
            }
        }

        private void openProject()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image to test";
            ofd.Filter = "Image | *.PNG";
            if (ofd.ShowDialog() == true)
            {

                //slidePlayer.Source = 
            }
        }

    }
}
