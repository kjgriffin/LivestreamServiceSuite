using System.Windows;

namespace IntegratedPresenter.Main
{
    /// <summary>
    /// Interaction logic for InputForm.xaml
    /// </summary>
    public partial class InputForm : Window
    {
        public InputForm(string defaultvalue, string inputname)
        {
            InitializeComponent();
            tbinput.Text = defaultvalue;
            tbname.Text = inputname;
        }

        public string UserInput
        {
            get => tbinput.Text;
        }

        private void ClickSet(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
