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
            rectIfTrans1.Fill = new SolidColorBrush(Colors.Gray);
            rectIfTrans2.Fill = new SolidColorBrush(Colors.Gray);
            rectIfTrans3.Fill = new SolidColorBrush(Colors.Gray);
            imgAutoTrans.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreyTransArrows.png"));
            imgTransDelay.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreyTimer.png"));
            tbAutoTransCamName.Text = "";
            tbAutoTransCamName.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void EnableConditionalTrans(string camName)
        {
            rectIfTrans1.Fill = new SolidColorBrush(Colors.Red);
            rectIfTrans2.Fill = new SolidColorBrush(Colors.Red);
            rectIfTrans3.Fill = new SolidColorBrush(Colors.Red);
            imgAutoTrans.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/RedTransArrows.png"));
            imgTransDelay.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BlueTimer.png"));
            tbAutoTransCamName.Text = camName;
            tbAutoTransCamName.Foreground = new SolidColorBrush(Colors.Red);
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
