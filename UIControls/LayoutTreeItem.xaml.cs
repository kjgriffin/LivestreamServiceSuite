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

using Xenon.SlideAssembly;

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
        string lib;

        SaveLayoutToLibrary SaveLayout;

        Action updateCallback;

        public LayoutTreeItem(string libname, string layoutname, string layoutjson, string group, SaveLayoutToLibrary save, Action updatecallback)
        {
            InitializeComponent();
            name = layoutname;
            json = layoutjson;
            lib = libname;
            this.group = group;
            SaveLayout = save;
            tbDisplayName.Text = layoutname;
            updateCallback = updatecallback;
        }

        private void Click_EditLayout(object sender, RoutedEventArgs e)
        {
            LayoutDesigner designer = new LayoutDesigner(lib, name, json, group, SaveLayout, updateCallback);
            designer.Show();
        }
    }
}
