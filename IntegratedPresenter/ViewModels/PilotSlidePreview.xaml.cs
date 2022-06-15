using Integrated_Presenter.Presentation;

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

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for PilotSlidePreview.xaml
    /// </summary>
    public partial class PilotSlidePreview : UserControl
    {

        public PilotSlidePreview()
        {
            InitializeComponent();
        }
        internal void UpdateUI(List<IPilotAction> currentSlideActions, string Name)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateUI(currentSlideActions, Name));
                return;
            }
            tbName.Text = Name;
            spDisplay.Children.Clear();

            foreach (var preset in currentSlideActions)
            {
                TextBlock tbCtrl = new TextBlock();
                tbCtrl.Foreground = Brushes.White;
                tbCtrl.Text = preset.DisplayInfo;
                spDisplay.Children.Add(tbCtrl);
            }
        }
    }
}
