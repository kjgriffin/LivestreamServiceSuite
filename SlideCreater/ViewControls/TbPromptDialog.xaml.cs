using System.Windows;

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
