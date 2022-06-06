using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Xenon.SlideAssembly.LayoutManagement;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for MacroEditor.xaml
    /// </summary>
    public partial class MacroEditor : Window
    {
        Dictionary<string, string> orig_Macros = new Dictionary<string, string>();
        public Dictionary<string, string> new_Macros = new Dictionary<string, string>();

        public bool Accepted = false;

        RenameMacroReferences renameMacro;
        FindAllMacroReferences findRefs;

        private Stack<(string, string)> renames = new Stack<(string, string)>();

        private string libname;

        public MacroEditor(string libname, Dictionary<string, string> macros, FindAllMacroReferences find, RenameMacroReferences rename)
        {
            InitializeComponent();

            Title = $"Edit Macros ({libname})";
            tbMName.Text = "New Macro";

            orig_Macros = macros;
            new_Macros = new Dictionary<string, string>(macros);

            this.findRefs = find;
            this.renameMacro = rename;
            this.libname = libname;

            UpdateMacroNamesUI();
        }

        private void UpdateMacroNamesUI()
        {
            lbMacroNames.Items.Clear();
            foreach (var macro in new_Macros)
            {
                lbMacroNames.Items.Add($"{macro.Key} ({findRefs(macro.Key, libname)})");
            }
            if (lbMacroNames.Items.Count > 0)
            {
                lbMacroNames.SelectedIndex = 0;
                GetSelection();
                tbView.Text = new_Macros[lastSelection];
            }
        }

        private void GetSelection()
        {
            lastSelection = Regex.Match(lbMacroNames.SelectedItem?.ToString() ?? "", @"(?<macro>.*) \([-\d]*\)$")?.Groups["macro"]?.Value ?? "";
        }

        string lastSelection = "";
        private void lbMacroNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMacroNames.SelectedIndex >= 0 && lastSelection != "")
            {
                // store last selection
                new_Macros[lastSelection] = tbView.Text;

                // show new selection
                GetSelection();
                tbView.Text = new_Macros[lastSelection].ToString();
            }
        }
        int id = 1;
        private void AddMacro(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbMName.Text))
            {
                return;
            }
            new_Macros[tbMName.Text] = "";
            tbMName.Text = $"New Macro {id++}";
            UpdateMacroNamesUI();
        }

        private void SaveMacros(object sender, RoutedEventArgs e)
        {
            GetSelection();
            new_Macros[lastSelection] = tbView.Text;

            Accepted = true;
            Close();
        }

        private void DiscardMacros(object sender, RoutedEventArgs e)
        {
            Accepted = false;

            // undo all the renaming in order
            while(renames.TryPop(out var rename))
            {
                this.renameMacro(rename.Item2, rename.Item1, libname);
            }

            Close();
        }

        private void lbMacroNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (lbMacroNames.SelectedItem != null)
                {
                    GetSelection();
                    new_Macros.Remove(lastSelection);
                    lastSelection = "";
                    UpdateMacroNamesUI();
                }
            }
        }

        private void RenameMacro(object sender, RoutedEventArgs e)
        {
            GetSelection();
            if (!string.IsNullOrWhiteSpace(tbMName.Text) && lastSelection != "")
            {
                // track what we're doing so discard can undo it
                renames.Push((lastSelection, tbMName.Text));
                
                renameMacro(lastSelection, tbMName.Text, libname);

                // rename it here too...
                var val = new_Macros[lastSelection];
                new_Macros.Remove(lastSelection);
                new_Macros[tbMName.Text] = val;

                lastSelection = "";

                UpdateMacroNamesUI();
            }
        }
    }
}
