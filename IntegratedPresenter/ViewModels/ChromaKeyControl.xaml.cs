using ATEMSharedState.SwitcherState;

using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using log4net;

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
    /// Interaction logic for ChromaKeyControl.xaml
    /// </summary>
    public partial class ChromaKeyControl : UserControl
    {
        IBMDSwitcherManager _switcher;
        ILog _logger;
        BMDUSKChromaSettings _activeChroma;
        Func<long, int> _convertSourceIDToButton;
        Func<long, int> _convertButtonToSourceID;

        public ChromaKeyControl()
        {
            InitializeComponent();

            _activeChroma = DefaultConfig.GetDefaultConfig().USKSettings.ChromaSettings;
        }

        public void SetLogger(ILog logger)
        {
            _logger = logger;
        }
        public void SetSwitcherDriver(IBMDSwitcherManager switcher)
        {
            _switcher = switcher;
        }
        public void InitUIDrivers(Func<long, int> convertSourceIDToButton, Func<int, int> convertButtonToSourceID)
        {
            _convertSourceIDToButton = convertSourceIDToButton;
            _convertButtonToSourceID = (x) => convertButtonToSourceID((int)x);
        }


        public void UpdateFromSwitcherState(BMDSwitcherState state)
        {
            _activeChroma = state.ChromaSettings;
            _activeChroma.FillSource = (int)state.USK1FillSource;

            if (!CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateFromSwitcherState(state);
                });
                return;
            }

            UpdateChromaSettingsUI(state);
        }

        private void UpdateChromaSettingsUI(BMDSwitcherState state)
        {
            UpdateButtonStyles(state.USK1FillSource);
            UpdateChromaSettings(state.ChromaSettings);
        }


        private void ClickChroma1(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma2(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma3(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma4(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma5(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma6(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma7(object sender, RoutedEventArgs e)
        {

        }

        private void ClickChroma8(object sender, RoutedEventArgs e)
        {

        }

        private void ClickApplyChromaSettings(object sender, RoutedEventArgs e)
        {
            _logger?.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()}");
            double hue = 0;
            double gain = 0;
            double lift = 0;
            double ysuppress = 0;
            int narrow = 0;
            double.TryParse(tbChromaHue.Text, out hue);
            double.TryParse(tbChromaGain.Text, out gain);
            double.TryParse(tbChromaLift.Text, out lift);
            double.TryParse(tbChromaYSuppress.Text, out ysuppress);
            int.TryParse(tbChromaNarrow.Text, out narrow);

            BMDUSKChromaSettings chromaSettings = new BMDUSKChromaSettings()
            {
                //FillSource = (int)((switcherState?.USK1FillSource) ?? _config.USKSettings.ChromaSettings.FillSource),
                FillSource = _activeChroma.FillSource,
                Hue = hue,
                Gain = gain,
                Lift = lift,
                YSuppress = ysuppress,
                Narrow = narrow
            };

            _switcher?.ConfigureUSK1Chroma(chromaSettings);
        }

        private void TextEntryMode(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void UpdateChromaSettings(BMDUSKChromaSettings chromaSettings)
        {
            tbChromaHue.Text = chromaSettings.Hue.ToString();
            tbChromaGain.Text = chromaSettings.Gain.ToString();
            tbChromaLift.Text = chromaSettings.Lift.ToString();
            tbChromaYSuppress.Text = chromaSettings.YSuppress.ToString();
            tbChromaNarrow.Text = chromaSettings.Narrow.ToString();
        }

        private void UpdateButtonStyles(long SourceID)
        {
            BtnChromaFillProgram1.Background = (_convertSourceIDToButton(SourceID) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram2.Background = (_convertSourceIDToButton(SourceID) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram3.Background = (_convertSourceIDToButton(SourceID) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram4.Background = (_convertSourceIDToButton(SourceID) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram5.Background = (_convertSourceIDToButton(SourceID) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram6.Background = (_convertSourceIDToButton(SourceID) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram7.Background = (_convertSourceIDToButton(SourceID) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnChromaFillProgram8.Background = (_convertSourceIDToButton(SourceID) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
        }

        internal void UpdateButtonLabels(List<ButtonSourceMapping> routing)
        {
            BtnChromaFillProgram1.Content = routing.First(x => x.ButtonId == 1).ButtonName;
            BtnChromaFillProgram2.Content = routing.First(x => x.ButtonId == 2).ButtonName;
            BtnChromaFillProgram3.Content = routing.First(x => x.ButtonId == 3).ButtonName;
            BtnChromaFillProgram4.Content = routing.First(x => x.ButtonId == 4).ButtonName;
            BtnChromaFillProgram5.Content = routing.First(x => x.ButtonId == 5).ButtonName;
            BtnChromaFillProgram6.Content = routing.First(x => x.ButtonId == 6).ButtonName;
            BtnChromaFillProgram7.Content = routing.First(x => x.ButtonId == 7).ButtonName;
            BtnChromaFillProgram8.Content = routing.First(x => x.ButtonId == 8).ButtonName;
        }

        internal void ShowHideShortcutsUI(bool showShortcuts)
        {
            var ShortcutVisibility = showShortcuts ? Visibility.Visible : Visibility.Hidden;
            ksc_cf1.Visibility = ShortcutVisibility;
            ksc_cf2.Visibility = ShortcutVisibility;
            ksc_cf3.Visibility = ShortcutVisibility;
            ksc_cf4.Visibility = ShortcutVisibility;
            ksc_cf5.Visibility = ShortcutVisibility;
            ksc_cf6.Visibility = ShortcutVisibility;
            ksc_cf7.Visibility = ShortcutVisibility;
            ksc_cf8.Visibility = ShortcutVisibility;
        }

        internal bool HasTextEntryFocus()
        {
            return tbChromaHue.IsFocused
                   || tbChromaGain.IsFocused
                   || tbChromaYSuppress.IsFocused
                   || tbChromaLift.IsFocused
                   || tbChromaNarrow.IsFocused;
        }

        internal void EnableControls(bool show)
        {
            var style = (show ? FindResource("SwitcherButton") : FindResource("SwitcherButton_Disabled")) as Style;
            BtnChromaFillProgram1.Style = style;
            BtnChromaFillProgram2.Style = style;
            BtnChromaFillProgram3.Style = style;
            BtnChromaFillProgram4.Style = style;
            BtnChromaFillProgram5.Style = style;
            BtnChromaFillProgram6.Style = style;
            BtnChromaFillProgram7.Style = style;
            BtnChromaFillProgram8.Style = style;

            var light = (show ? FindResource("GrayLight") : FindResource("OffLight")) as Brush;
            BtnChromaFillProgram1.Background = light;
            BtnChromaFillProgram2.Background = light;
            BtnChromaFillProgram3.Background = light;
            BtnChromaFillProgram4.Background = light;
            BtnChromaFillProgram5.Background = light;
            BtnChromaFillProgram6.Background = light;
            BtnChromaFillProgram7.Background = light;
            BtnChromaFillProgram8.Background = light;
        }

        internal void NewConfig(BMDSwitcherConfigSettings config)
        {
            //throw new NotImplementedException();
        }
    }
}
