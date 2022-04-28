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

using Xenon.SlideAssembly.LayoutManagement;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for LayoutGroupTreeItem.xaml
    /// </summary>
    public partial class LibraryMacrosTreeItem : TreeViewItem
    {

        public string LibName { get; set; }
        public string HName { get; set; }

        GetLibraryMacros getMacros;
        EditLibraryMacros editMacros;

        FindAllMacroReferences find;
        RenameMacroReferences rename;

        public LibraryMacrosTreeItem(string hname, string libName, bool editable, GetLibraryMacros getMacros, EditLibraryMacros editMacros, FindAllMacroReferences find, RenameMacroReferences rename)
        {
            InitializeComponent();

            HName = hname;
            LibName = libName;

            this.getMacros = getMacros;
            this.editMacros = editMacros;
            this.find = find;
            this.rename = rename;

            tbHeader.Text = HName;

            if (!editable)
            {
                btn_new.Visibility = Visibility.Collapsed;
                btn_new.IsEnabled = false;
            }

            this.getMacros = getMacros;
            this.editMacros = editMacros;

        }

        private void Click_EditMacros(object sender, RoutedEventArgs e)
        {
            MacroEditor editor = new MacroEditor(LibName, getMacros(LibName), find, rename);
            editor.ShowDialog();
            if (editor.Accepted)
            {
                editMacros(LibName, editor.new_Macros);
            }
        }
    }
}
