using IntegratedPresenter.BMDSwitcher.Config;
using IntegratedPresenter.Main;

using SwitcherControl.BMDSwitcher;

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
    /// Interaction logic for StillVideoFullContentAutomationPreview.xaml
    /// </summary>
    public partial class StillVideoFullContentAutomationPreview : UserControl
    {
        public StillVideoFullContentAutomationPreview()
        {
            InitializeComponent();
        }

        private void DisableConditionalTrans()
        {
            rectIfTrans1.Fill = new SolidColorBrush(Colors.Gray);
            rectIfTrans2.Fill = new SolidColorBrush(Colors.Gray);
            rectIfTrans3.Fill = new SolidColorBrush(Colors.Gray);
            imgAutoTrans.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreyTransArrows.png"));
            tbAutoTransCamName.Text = "";
            tbAutoTransCamName.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void EnableConditionalTrans(string camName)
        {
            rectIfTrans1.Fill = new SolidColorBrush(Colors.Red);
            rectIfTrans2.Fill = new SolidColorBrush(Colors.Red);
            rectIfTrans3.Fill = new SolidColorBrush(Colors.Red);
            imgAutoTrans.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/RedTransArrows.png"));
            tbAutoTransCamName.Text = camName;
            tbAutoTransCamName.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void DisableDSKConditional()
        {
            rect1IfDSK.Fill = new SolidColorBrush(Colors.Gray);
            rect2IfDSK.Fill = new SolidColorBrush(Colors.Gray);
            rect3IfDSK.Fill = new SolidColorBrush(Colors.Gray);
            imgDKSDelay.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreyTimer.png"));
            imgDSK1Off.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/KeyGrey.png"));
        }

        private void EnableDSKConditional()
        {
            rect1IfDSK.Fill = new SolidColorBrush(Colors.Red);
            rect2IfDSK.Fill = new SolidColorBrush(Colors.Red);
            rect3IfDSK.Fill = new SolidColorBrush(Colors.Red);
            imgDKSDelay.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BlueTimer.png"));
            imgDSK1Off.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/KeyRed.png"));
        }

        private void DisableVideoPlaybackConditional()
        {
            rect1IfVideo.Fill = new SolidColorBrush(Colors.Gray);
            rect2IfVideo.Fill = new SolidColorBrush(Colors.Gray);
            rect3IfVideo.Fill = new SolidColorBrush(Colors.Gray);
            imgVideoPlay.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreyPlay.png"));
            imgVideoPreroll.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreyTimer.png"));
        }

        private void EnableVideoPlaybackConditional()
        {
            rect1IfVideo.Fill = new SolidColorBrush(Colors.Red);
            rect2IfVideo.Fill = new SolidColorBrush(Colors.Red);
            rect3IfVideo.Fill = new SolidColorBrush(Colors.Red);
            imgVideoPlay.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreenPlay.png"));
            imgVideoPreroll.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BlueTimer.png"));
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
