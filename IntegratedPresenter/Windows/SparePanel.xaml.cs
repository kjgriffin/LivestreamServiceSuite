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

namespace Integrated_Presenter.Windows
{
    /// <summary>
    /// Interaction logic for SparePanel.xaml
    /// </summary>
    public partial class SparePanel : Window
    {
        public event EventHandler OnReleaseFocus;
        public SparePanel()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OnReleaseFocus?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
