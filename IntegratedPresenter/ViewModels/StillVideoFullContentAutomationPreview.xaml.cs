using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;

using IntegratedPresenterAPIInterop;

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for StillVideoFullContentAutomationPreview.xaml
    /// </summary>
    public partial class StillVideoFullContentAutomationPreview : UserControl
    {
        Brush gray;
        Brush red;
        Brush green;
        Brush teal;

        public StillVideoFullContentAutomationPreview()
        {
            InitializeComponent();
            gray = FindResource("grayBrush") as Brush;
            red = FindResource("redBrush") as Brush;
            green = FindResource("greenBrush") as Brush;
            teal = FindResource("tealBrush") as Brush;
        }

        private void DisableConditionalTrans()
        {
            rectIfTrans1.Fill = gray;
            rectIfTrans2.Fill = gray;
            rectIfTrans3.Fill = gray;
            imgAutoTrans.Fill = gray;
            tbAutoTransCamName.Text = "";
            tbAutoTransCamName.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void EnableConditionalTrans(string camName)
        {
            rectIfTrans1.Fill = red;
            rectIfTrans2.Fill = red;
            rectIfTrans3.Fill = red;
            imgAutoTrans.Fill = red;
            tbAutoTransCamName.Text = camName;
            tbAutoTransCamName.Foreground = red;
        }

        private void DisableDSKConditional()
        {
            rect1IfDSK.Fill = gray;
            rect2IfDSK.Fill = gray;
            rect3IfDSK.Fill = gray;
            imgDSKDelay.Fill = gray;
            imgDSK1Off.Fill = gray;
        }

        private void EnableDSKConditional()
        {
            rect1IfDSK.Fill = red;
            rect2IfDSK.Fill = red;
            rect3IfDSK.Fill = red;
            imgDSKDelay.Fill = teal;
            imgDSK1Off.Fill = red;
        }

        private void DisableVideoPlaybackConditional()
        {
            rect1IfVideo.Fill = gray;
            rect2IfVideo.Fill = gray;
            rect3IfVideo.Fill = gray;
            imgVideoPlay.Fill = gray;
            imgVideoPreroll.Fill = gray;
        }

        private void EnableVideoPlaybackConditional()
        {
            rect1IfVideo.Fill = red;
            rect2IfVideo.Fill = red;
            rect3IfVideo.Fill = red;
            imgVideoPlay.Fill = green;
            imgVideoPreroll.Fill = teal;
        }


        public void FireOnSwitcherStateChanged(BMDSwitcherState state, BMDSwitcherConfigSettings config)
        {
            if (state.ProgramID == config.Routing.Where(r => r.KeyName == "slide").First().PhysicalInputId)
            {
                DisableConditionalTrans();
            }
            else
            {
                EnableConditionalTrans(config.Routing.FirstOrDefault(r => r.KeyName == "slide")?.LongName.ToUpper() ?? $"SLIDE");
            }

            if (state.DSK1OnAir)
            {
                EnableDSKConditional();
            }
            else
            {
                DisableDSKConditional();
            }
        }

        public void FireOnSlideTypeChanged(SlideType? type)
        {
            if (type == SlideType.Video)
            {
                EnableVideoPlaybackConditional();
            }
            else
            {
                DisableVideoPlaybackConditional();
            }
        }

    }
}
