using ATEMSharedState.SwitcherState;

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
    /// Interaction logic for DVEPIPControl.xaml
    /// </summary>
    public partial class DVEPIPControl : UserControl
    {
        BMDSwitcherConfigSettings _config;
        IBMDSwitcherManager _switcher;
        ILog _logger;
        Func<long, int> _ConvertSourceIDToButton;
        Func<int, int> _ConvertButtonToSourceID;
        public DVEPIPControl()
        {
            InitializeComponent();
        }
        internal void NewConfig(BMDSwitcherConfigSettings config)
        {
            _config = config;
        }

        internal void InitUIDrivers(Func<long, int> convertSourceIDToButton, Func<int, int> convertButtonToSourceID)
        {
            _ConvertButtonToSourceID = convertButtonToSourceID;
            _ConvertSourceIDToButton = convertSourceIDToButton;
        }

        internal void SetLogger(ILog logger)
        {
            _logger = logger;
        }
        internal void SetSwitcherDriver(IBMDSwitcherManager switcher)
        {
            _switcher = switcher;
        }



        private void ClickPIP8(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(8));
        }

        private void ClickPIP7(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(7));
        }

        private void ClickPIP6(object sender, RoutedEventArgs e)
        {

            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(6));
        }

        private void ClickPIP5(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(5));
        }

        private void ClickPIP4(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(4));
        }

        private void ClickPIP3(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(3));
        }

        private void ClickPIP2(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(2));
        }

        private void ClickPIP1(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_ConvertButtonToSourceID(1));
        }

        internal void UpdateUIPIPPlaceKeys(BMDSwitcherState state)
        {
            PipPresetButton[] btns = new PipPresetButton[5]
            {
                pipprestbtn_1,
                pipprestbtn_2,
                pipprestbtn_3,
                pipprestbtn_4,
                pipprestbtn_5,
            };

            for (int i = 0; i < 5; i++)
            {
                btns[i].IsActive = false;
                btns[i].UpdateLayout();
                if (_config.PIPPresets.Presets.TryGetValue(i + 1, out var cfg))
                {
                    btns[i].PlaceName = cfg.Name;
                    btns[i].PIPPlace = cfg.Placement;
                }
            }

            UpdateUIPIPPlaceKeysActiveState(state);
        }

        private void UpdateUIPIPPlaceKeysActiveState(BMDSwitcherState state)
        {

            PipPresetButton[] btns = new PipPresetButton[5]
            {
                pipprestbtn_1,
                pipprestbtn_2,
                pipprestbtn_3,
                pipprestbtn_4,
                pipprestbtn_5,
            };

            for (int i = 0; i < 5; i++)
            {
                if (_config.PIPPresets.Presets.TryGetValue(i + 1, out var cfg))
                {
                    // check if config matches
                    btns[i].IsActive = cfg.Placement.Equivalent(state?.DVESettings);
                }
                else
                {
                    btns[i].IsActive = false;
                }
            }

        }

        private void pipprestbtn_1_OnClick()
        {
            _logger.Debug($"Click: PIP Run to Preset 1");
            SetupPIPToPresetPosition(1);
        }

        private void pipprestbtn_2_OnClick()
        {
            _logger.Debug($"Click: PIP Run to Preset 2");
            SetupPIPToPresetPosition(2);
        }

        private void pipprestbtn_3_OnClick()
        {
            _logger.Debug($"Click: PIP Run to Preset 3");
            SetupPIPToPresetPosition(3);
        }

        private void pipprestbtn_4_OnClick()
        {
            _logger.Debug($"Click: PIP Run to Preset 4");
            SetupPIPToPresetPosition(4);
        }

        private void pipprestbtn_5_OnClick()
        {
            _logger.Debug($"Click: PIP Run to Preset 5");
            SetupPIPToPresetPosition(5);
        }
        internal void SetupPIPToPresetPosition(int presetNum)
        {
            _logger.Debug($"Running {System.Reflection.MethodBase.GetCurrentMethod()} with arg presetNum {presetNum}");
            if (_config?.PIPPresets?.Presets?.ContainsKey(presetNum) == true && _switcher != null)
            {
                _logger.Debug($"Requesting switcher to recall preset ({presetNum}) as configured.");
                var cfg = _config.PIPPresets.Presets[presetNum];
                _logger.Debug($"(SetupPIPToPresetPosition) -- PlacePIP at {cfg}");
                var current = _switcher?.GetCurrentState().DVESettings;
                var config = cfg.Placement.PlaceOverride(current);
                _switcher?.SetPIPPosition(config);
            }
        }


        internal void EnableControls(bool Enabled)
        {
            Style bStyle = (Enabled ? FindResource("SwitcherButton") : FindResource("SwitcherButton_Disabled")) as Style;
            Style pStyle = (Enabled ? FindResource("PIPControlButton") : FindResource("InactivePIPControlButton")) as Style;
            Brush bBrush = (Enabled ? FindResource("GrayLight") : FindResource("OffLight")) as Brush;

            BtnPIPFillProgram1.Style = bStyle;
            BtnPIPFillProgram2.Style = bStyle;
            BtnPIPFillProgram3.Style = bStyle;
            BtnPIPFillProgram4.Style = bStyle;
            BtnPIPFillProgram5.Style = bStyle;
            BtnPIPFillProgram6.Style = bStyle;
            BtnPIPFillProgram7.Style = bStyle;
            BtnPIPFillProgram8.Style = bStyle;

            BtnPIPFillProgram1.Background = bBrush;
            BtnPIPFillProgram2.Background = bBrush;
            BtnPIPFillProgram3.Background = bBrush;
            BtnPIPFillProgram4.Background = bBrush;
            BtnPIPFillProgram5.Background = bBrush;
            BtnPIPFillProgram6.Background = bBrush;
            BtnPIPFillProgram7.Background = bBrush;
            BtnPIPFillProgram8.Background = bBrush;

            pipprestbtn_1.btn.Style = pStyle;
            pipprestbtn_2.btn.Style = pStyle;
            pipprestbtn_3.btn.Style = pStyle;
            pipprestbtn_4.btn.Style = pStyle;
            pipprestbtn_5.btn.Style = pStyle;
        }

        internal void UpdateFromSwitcherState(BMDSwitcherState switcherState)
        {
            BtnPIPFillProgram1.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram2.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram3.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram4.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram5.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram6.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram7.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram8.Background = (_ConvertSourceIDToButton(switcherState.USK1FillSource) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;

            UpdateUIPIPPlaceKeysActiveState(switcherState);
        }

        internal void UpdateButtonLabels(List<ButtonSourceMapping> routing)
        {
            BtnPIPFillProgram1.Content = routing.First(x => x.ButtonId == 1).ButtonName;
            BtnPIPFillProgram2.Content = routing.First(x => x.ButtonId == 2).ButtonName;
            BtnPIPFillProgram3.Content = routing.First(x => x.ButtonId == 3).ButtonName;
            BtnPIPFillProgram4.Content = routing.First(x => x.ButtonId == 4).ButtonName;
            BtnPIPFillProgram5.Content = routing.First(x => x.ButtonId == 5).ButtonName;
            BtnPIPFillProgram6.Content = routing.First(x => x.ButtonId == 6).ButtonName;
            BtnPIPFillProgram7.Content = routing.First(x => x.ButtonId == 7).ButtonName;
            BtnPIPFillProgram8.Content = routing.First(x => x.ButtonId == 8).ButtonName;
        }

        internal void ShowHideShortcutsUI(bool showShortcuts)
        {
            var visible = showShortcuts ? Visibility.Visible : Visibility.Hidden;
            ksc_pf1.Visibility = visible;
            ksc_pf2.Visibility = visible;
            ksc_pf3.Visibility = visible;
            ksc_pf4.Visibility = visible;
            ksc_pf5.Visibility = visible;
            ksc_pf6.Visibility = visible;
            ksc_pf7.Visibility = visible;
            ksc_pf8.Visibility = visible;
            pipprestbtn_1.KSCVIsible = showShortcuts;
            pipprestbtn_2.KSCVIsible = showShortcuts;
            pipprestbtn_3.KSCVIsible = showShortcuts;
            pipprestbtn_4.KSCVIsible = showShortcuts;
            pipprestbtn_5.KSCVIsible = showShortcuts;
        }
    }
}
