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

namespace UIControls
{
    /// <summary>
    /// Interaction logic for LayoutTreeItem.xaml
    /// </summary>
    public partial class LayoutTreeItem : UserControl
    {

        string json;
        string name;
        string group;

        public LayoutTreeItem(string layoutname, string layoutjson, string group)
        {
            InitializeComponent();
            name = layoutname;
            json = layoutjson;
            this.group = group;
            tbDisplayName.Text = layoutname;
        }

        private void Click_EditLayout(object sender, RoutedEventArgs e)
        {
            LayoutDesigner designer = new LayoutDesigner(name, json, group);
            designer.Show();
        }
    }
}
