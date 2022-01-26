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

namespace SlideCreater
{
    /// <summary>
    /// Interaction logic for CheckboxSelection.xaml
    /// </summary>
    public partial class CheckboxSelection : Window
    {
        List<(string name, bool value)> _fields = new List<(string, bool)>();
        public List<(string name, bool value)> Fields { get; private set; } = new List<(string, bool)>();
        public CheckboxSelection(string title, params (string, bool)[] fields)
        {
            InitializeComponent();
            Title = title;
            _fields = fields.ToList();
            LoadOptions();
        }
        private void LoadOptions()
        {
            lbitems.Items.Clear();
            foreach (var field in _fields)
            {
                CheckBox cb = new CheckBox();
                cb.Content = field.name;
                cb.IsChecked = field.value;
                lbitems.Items.Add(cb);
            }
        }

        private void Click_OK(object sender, RoutedEventArgs e)
        {
            Fields.Clear();
            foreach (var item in lbitems.Items)
            {
                CheckBox cb = (CheckBox)item;
                Fields.Add((cb.Content.ToString(), cb.IsChecked ?? false));
            }
            Close();
        }
    }
}
