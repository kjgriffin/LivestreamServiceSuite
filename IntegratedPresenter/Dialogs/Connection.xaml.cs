using System.Windows;

namespace IntegratedPresenter.Main
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : Window
    {
        public Connection(string title, string ipname, string defaultip)
        {
            InitializeComponent();
            Title = title;
            lbIP.Text = ipname;
            tbIP.Text = defaultip;
        }

        public string IP = string.Empty;
        private void Connect(object sender, RoutedEventArgs e)
        {
            IP = tbIP.Text;
            DialogResult = true;
            Close();
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
