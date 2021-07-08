using Integrated_Presenter.BMDSwitcher.Config;
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

        public void SetPostset(int id, BMDMultiviewerSettings config)
        {

            // figure out which 'window' corresponds to the provided camera id
            Dictionary<int, int> mappedwindows = new Dictionary<int, int>();
            mappedwindows[8] = config.Window2;
            mappedwindows[7] = config.Window3;
            mappedwindows[6] = config.Window4;
            mappedwindows[5] = config.Window5;
            mappedwindows[4] = config.Window6;
            mappedwindows[3] = config.Window7;
            mappedwindows[2] = config.Window8;
            mappedwindows[1] = config.Window9;

            Dispatcher.Invoke(() =>
            {
                cam1.Background = mappedwindows[1] == id ? Brushes.Green : Brushes.Transparent;
                cam2.Background = mappedwindows[2] == id ? Brushes.Green : Brushes.Transparent;
                cam3.Background = mappedwindows[3] == id ? Brushes.Green : Brushes.Transparent;
                cam4.Background = mappedwindows[4] == id ? Brushes.Green : Brushes.Transparent;
                cam5.Background = mappedwindows[5] == id ? Brushes.Green : Brushes.Transparent;
                cam6.Background = mappedwindows[6] == id ? Brushes.Green : Brushes.Transparent;
                cam7.Background = mappedwindows[7] == id ? Brushes.Green : Brushes.Transparent;
                cam8.Background = mappedwindows[8] == id ? Brushes.Green : Brushes.Transparent;
            });
        }
    }
}
