using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Xenon.SlideAssembly;
using Xenon.SlideAssembly.LayoutManagement;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for LayoutTreeItem.xaml
    /// </summary>
    public partial class LayoutTreeItem : UserControl
    {

        string name;
        string group;
        string lib;

        List<string> libs;

        bool canedit;

        SaveLayoutToLibrary SaveLayout;

        Action updateCallback;
        Action<string, string, string> deleteCallback;

        GetLayoutPreview getLayoutPreview;
        GetLayoutSource getLayoutSource;

        public LayoutTreeItem(string libname, List<string> libs, string layoutname, GetLayoutSource getSource, string group, bool canedit, SaveLayoutToLibrary save, Action updatecallback, Action<string, string, string> deletecallback, GetLayoutPreview getLayoutPreview)
        {
            InitializeComponent();
            name = layoutname;
            getLayoutSource = getSource;
            lib = libname;
            this.libs = libs;
            this.canedit = canedit;
            this.group = group;
            SaveLayout = save;
            tbDisplayName.Text = layoutname;
            updateCallback = updatecallback;
            this.deleteCallback = deletecallback;
            this.getLayoutPreview = getLayoutPreview;

            if (!canedit)
            {
                btn_delete.IsEnabled = false;
                btn_delete.Visibility = Visibility.Collapsed;
            }
        }

        private void Click_EditLayout(object sender, RoutedEventArgs e)
        {
            LayoutDesigner designer = new LayoutDesigner(lib, libs, name, getLayoutSource(name, group, lib), group, canedit, SaveLayout, updateCallback, getLayoutPreview);
            designer.Show();
        }

        private void Click_Delete(object sender, RoutedEventArgs e)
        {
            deleteCallback?.Invoke(lib, group, name);
        }
    }
}
