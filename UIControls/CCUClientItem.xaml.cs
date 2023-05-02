using System;
using System.Windows;
using System.Windows.Controls;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for CCUClientItem.xaml
    /// </summary>
    public partial class CCUClientItem : UserControl
    {

        public event EventHandler OnDeleteRequest;
        public event EventHandler OnEditsMade;

        public CCUClientItem(string clientName, string addr, string port, string association)
        {
            InitializeComponent();
            tbName.Text = clientName;
            tbIP.Text = addr;
            tbPort.Text = port;
            tbAssociation.Text = association;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            OnEditsMade?.Invoke(this, new EventArgs());
        }

        private void ClickDelete(object sender, RoutedEventArgs e)
        {
            OnDeleteRequest?.Invoke(this, new EventArgs());
        }
    }
}
