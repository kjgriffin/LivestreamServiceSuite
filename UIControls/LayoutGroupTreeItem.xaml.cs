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
    /// Interaction logic for LayoutGroupTreeItem.xaml
    /// </summary>
    public partial class LayoutGroupTreeItem : TreeViewItem
    {

        public string GroupName { get; set; }
        public string HName { get; set; }
        public Action<string> AddLayoutCallback { get; set; }

        public LayoutGroupTreeItem(string hname, string gname, bool editable, Action<string> action)
        {
            InitializeComponent();
            HName = hname;
            GroupName = gname;
            AddLayoutCallback = action;

            tbHeader.Text = HName;

            if (!editable)
            {
                btn_new.Visibility = Visibility.Collapsed;
                btn_new.IsEnabled = false;
            }
        }

        private void Click_NewLayout(object sender, RoutedEventArgs e)
        {
            AddLayoutCallback?.Invoke(GroupName);
        }
    }
}
