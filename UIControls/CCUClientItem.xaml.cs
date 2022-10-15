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
