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

namespace IntegratedPresenter.Main
{
    /// <summary>
    /// Interaction logic for ConnectComPort.xaml
    /// </summary>
    public partial class ConnectComPort : Window
    {

        public string Port = "";
        public bool Selected = false;

        public ConnectComPort(List<string> names)
        {
            InitializeComponent();
            foreach (var name in names)
            {
                cbportnames.Items.Add(name);
            }
        }

        private void ClickConnect(object sender, RoutedEventArgs e)
        {
            if (cbportnames.SelectedItem != null)
            {
                Port = (string)cbportnames.SelectedItem;
                Selected = true;
            }
            Close();
        }

        private void ClickCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
