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
using System.Windows.Shapes;

namespace Presenter
{
    /// <summary>
    /// Interaction logic for PresenterControl.xaml
    /// </summary>
    public partial class PresenterControl : Window
    {
        PresenterWindow _window;
        public PresenterControl(PresenterWindow window)
        {
            InitializeComponent();
            _window = window;
        }

        private void Next_Slide(object sender, RoutedEventArgs e)
        {
            _window.NextSlide();
        }

        private void Prev_Slide(object sender, RoutedEventArgs e)
        {
            _window.PrevSlide();
        }

        private void Play_Media(object sender, RoutedEventArgs e)
        {
            _window.StartMediaPlayback();
        }

        private void Pause_Media(object sender, RoutedEventArgs e)
        {
            _window.PauseMediaPlayback();
        }

        private void Replay_Media(object sender, RoutedEventArgs e)
        {
            _window.RestartMediaPlayback();
        }
    }
}
