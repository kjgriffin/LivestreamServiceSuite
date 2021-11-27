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
    /// Interaction logic for TbPromptDialog.xaml
    /// </summary>
    public partial class TbPromptDialog : Window
    {

        public string ResultValue { get; private set; }

        public TbPromptDialog(string title, string fname, string defaultVal = "")
        {
            InitializeComponent();
            Title = title;
            tbDescription.Text = fname;
            tbInput.Text = defaultVal;
            ResultValue = defaultVal;
        }

        private void Click_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Click_Ok(object sender, RoutedEventArgs e)
        {
            ResultValue = tbInput.Text;
            DialogResult = true;
            Close();
        }
    }
}
