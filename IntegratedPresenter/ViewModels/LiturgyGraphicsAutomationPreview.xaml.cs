using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for LiturgyGraphicsAutomationPreview.xaml
    /// </summary>
    public partial class LiturgyGraphicsAutomationPreview : UserControl
    {
        public LiturgyGraphicsAutomationPreview()
        {
            InitializeComponent();
        }

        private void DisableConditionalTrans()
        {
            rectIfTrans1.Fill = (Brush)FindResource("grayBrush");
            rectIfTrans2.Fill = (Brush)FindResource("grayBrush");
            rectIfTrans3.Fill = (Brush)FindResource("grayBrush");
            btnTransArrows.Fill = (Brush)FindResource("grayBrush");
            btnTransDelay.Fill = (Brush)FindResource("grayBrush");
            tbAutoTransCamName.Text = "";
            tbAutoTransCamName.Foreground = (Brush)FindResource("grayBrush");
        }

        private void EnableConditionalTrans(string camName)
        {
            rectIfTrans1.Fill = new SolidColorBrush(Colors.Red);
            rectIfTrans2.Fill = new SolidColorBrush(Colors.Red);
            rectIfTrans3.Fill = new SolidColorBrush(Colors.Red);
            btnTransArrows.Fill = (Brush)FindResource("redBrush");
            btnTransDelay.Fill = (Brush)FindResource("tealBrush");
            tbAutoTransCamName.Text = camName;
            tbAutoTransCamName.Foreground =(Brush)FindResource("redBrush");
        }

        public void FireOnSwitcherStateChanged(BMDSwitcherState state, BMDSwitcherConfigSettings config)
        {
            if (state.ProgramID == config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
            {
                EnableConditionalTrans(config.Routing.FirstOrDefault(r => r.PhysicalInputId == state.PresetID)?.LongName.ToUpper() ?? $"INPUT {state.PresetID}");
            }
            else
            {
                DisableConditionalTrans();
            }
        }


    }
}
