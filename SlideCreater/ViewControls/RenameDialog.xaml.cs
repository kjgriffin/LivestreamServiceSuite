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
using System.Windows.Shapes;

namespace SlideCreater.ViewControls
{
    /// <summary>
    /// Interaction logic for RenameDialog_.xaml
    /// </summary>
    public partial class RenameDialog : Window
    {

        public bool Result = false;
        public string NewName = "";

        public RenameDialog(string oldname)
        {
            InitializeComponent();
            tbOldName.Text = oldname;
            tbHintText.Text = oldname;
        }

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClickRename(object sender, RoutedEventArgs e)
        {
            Result = true;
            NewName = tbNewName.Text;
            Close();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbNewName.Text))
            {
                tbHintText.Visibility = Visibility.Hidden;
            }
            else
            {
                tbHintText.Visibility = Visibility.Visible;
            }
        }
    }
}
