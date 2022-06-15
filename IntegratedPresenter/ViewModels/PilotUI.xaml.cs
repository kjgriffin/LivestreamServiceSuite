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
    /// Interaction logic for PilotUI.xaml
    /// </summary>
    public partial class PilotUI : UserControl
    {
        public PilotUI()
        {
            InitializeComponent();
        }

        public void UpdateUI(bool PilotEnabled, List<IPilotAction> currentSlideActions, List<IPilotAction> nextSlideActions)
        {
            ellipseState.Fill = PilotEnabled ? Brushes.LimeGreen : Brushes.Red;
            pvCurrent.UpdateUI(currentSlideActions, "CURRENT SLIDE");
            pvNext.UpdateUI(nextSlideActions, "NEXT SLIDE");
        }

    }
}
