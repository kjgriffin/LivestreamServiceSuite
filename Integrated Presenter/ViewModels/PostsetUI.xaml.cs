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

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for PostsetUI.xaml
    /// </summary>
    public partial class PostsetUI : UserControl
    {
        public PostsetUI()
        {
            InitializeComponent();
        }

        public void SetPostset(int id)
        {
            Dispatcher.Invoke(() =>
            {
                cam1.Background = Brushes.Transparent;
                cam2.Background = Brushes.Transparent;
                cam3.Background = Brushes.Transparent;
                cam4.Background = Brushes.Transparent;
                cam5.Background = Brushes.Transparent;
                cam6.Background = Brushes.Transparent;
                cam7.Background = Brushes.Transparent;
                cam8.Background = Brushes.Transparent;
                switch (id)
                {
                    case 1:
                        cam1.Background = Brushes.Green;
                        break;
                    case 2:
                        cam2.Background = Brushes.Green;
                        break;
                    case 3:
                        cam3.Background = Brushes.Green;
                        break;
                    case 4:
                        cam4.Background = Brushes.Green;
                        break;
                    case 5:
                        cam5.Background = Brushes.Green;
                        break;
                    case 6:
                        cam6.Background = Brushes.Green;
                        break;
                    case 7:
                        cam7.Background = Brushes.Green;
                        break;
                    case 8:
                        cam8.Background = Brushes.Green;
                        break;
                }
            });
        }
    }
}
