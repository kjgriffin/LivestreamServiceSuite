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
using System.Windows.Shapes;

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

        public MacroEditor(string libname, Dictionary<string, string> macros)
        {
            InitializeComponent();

            Title = $"Edit Macros ({libname})";
            tbMName.Text = "New Macro";

            orig_Macros = macros;
            new_Macros = new Dictionary<string, string>(macros);

            UpdateMacroNamesUI();
        }

        private void UpdateMacroNamesUI()
        {
            lbMacroNames.Items.Clear();
            foreach (var macro in new_Macros)
            {
                lbMacroNames.Items.Add(macro.Key);
            }
            if (lbMacroNames.Items.Count > 0)
            {
                lbMacroNames.SelectedIndex = 0;
                lastSelection = lbMacroNames.Items[0].ToString();
                tbView.Text = new_Macros[lastSelection];
            }
        }

        string lastSelection = "";
        private void lbMacroNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMacroNames.SelectedIndex >= 0 && lastSelection != "")
            {
                // store last selection
                new_Macros[lastSelection] = tbView.Text;

                // show new selection
                lastSelection = lbMacroNames.SelectedItem.ToString();
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
            lastSelection = lbMacroNames.SelectedItem.ToString();
            new_Macros[lastSelection] = tbView.Text;

            Accepted = true;
            Close();
        }

        private void DiscardMacros(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }

        private void lbMacroNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (lbMacroNames.SelectedItem != null)
                {
                    lastSelection = lbMacroNames.SelectedItem.ToString();
                    new_Macros.Remove(lastSelection);
                    lastSelection = "";
                    UpdateMacroNamesUI();
                }
            }
        }
    }
}
