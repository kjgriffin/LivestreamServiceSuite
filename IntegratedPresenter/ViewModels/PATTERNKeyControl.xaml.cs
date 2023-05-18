using ATEMSharedState.SwitcherState;

using Configurations.SwitcherConfig;

using IntegratedPresenter.BMDSwitcher.Config;

using SwitcherControl.BMDSwitcher;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for PATTERNKeyControl.xaml
    /// </summary>
    public partial class PATTERNKeyControl : UserControl
    {

        Func<long, int> _convertSourceIDToButton;
        Func<long, int> _convertButtonToSourceID;
        IBMDSwitcherManager _switcher;

        BMDUSKPATTERNSettings _activePattern;

        bool UIDriven = true;

        public PATTERNKeyControl()
        {
            InitializeComponent();
            EnableHandlers();
            ShowShortcuts(false);

            // get a state
            // assume default until otherwise instructed??
            _activePattern = DefaultConfig.GetDefaultConfig().USKSettings.PATTERNSettings;
        }

        private void EnableHandlers()
        {
            slide_size.ValueChanged += Slide_size_ValueChanged;
            slide_sym.ValueChanged += Slide_sym_ValueChanged;
            slide_sharp.ValueChanged += Slide_sharp_ValueChanged;
            slide_x.ValueChanged += Slide_x_ValueChanged;
            slide_y.ValueChanged += Slide_y_ValueChanged;
        }

        private void Slide_y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!UIDriven)
            {
                return;
            }
            DisableHandlers();
            var val = _activePattern.Copy();
            val.YOffset = e.NewValue;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void Slide_x_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!UIDriven)
            {
                return;
            }
            DisableHandlers();
            var val = _activePattern.Copy();
            val.XOffset = e.NewValue;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void Slide_sharp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!UIDriven)
            {
                return;
            }
            DisableHandlers();
            var val = _activePattern.Copy();
            val.Softness = e.NewValue;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void Slide_sym_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!UIDriven)
            {
                return;
            }
            DisableHandlers();
            var val = _activePattern.Copy();
            val.Symmetry = e.NewValue;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void Slide_size_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!UIDriven)
            {
                return;
            }
            DisableHandlers();
             var val = _activePattern.Copy();
            val.Size = e.NewValue;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void DisableHandlers()
        {
            //UIDriven = false;
            /*
            slide_size.ValueChanged -= Slide_size_ValueChanged;
            slide_sym.ValueChanged -= Slide_sym_ValueChanged;
            slide_sharp.ValueChanged -= Slide_sharp_ValueChanged;
            slide_x.ValueChanged -= Slide_x_ValueChanged;
            slide_y.ValueChanged -= Slide_y_ValueChanged;
            */
        }

        public void InitUIDrivers(Func<long, int> convertSourceIDToButton, Func<int, int> convertButtonToSourceID)
        {
            _convertSourceIDToButton = convertSourceIDToButton;
            _convertButtonToSourceID = (x) => convertButtonToSourceID((int)x);
        }

        public void SetSwitcherDriver(IBMDSwitcherManager switcher)
        {
            _switcher = switcher;
        }

        public void EnableControls(bool enabled)
        {
            string style = enabled ? "SwitcherButton" : "SwitcherButton_Disabled";

            BtnPIPFillProgram1.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram2.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram3.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram4.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram5.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram6.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram7.Style = (Style)Application.Current.FindResource(style);
            BtnPIPFillProgram8.Style = (Style)Application.Current.FindResource(style);
        }

        public void UpdateFromSwitcherState(BMDSwitcherState state)
        {
            _activePattern = state.PATTERNSettings;
            _activePattern.DefaultFillSource = (int)state.USK1FillSource;

            if (!CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateFromSwitcherState(state);
                });
                return;
            }
            // fill source
            BtnPIPFillProgram1.Background = (_convertSourceIDToButton(state.USK1FillSource) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram2.Background = (_convertSourceIDToButton(state.USK1FillSource) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram3.Background = (_convertSourceIDToButton(state.USK1FillSource) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram4.Background = (_convertSourceIDToButton(state.USK1FillSource) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram5.Background = (_convertSourceIDToButton(state.USK1FillSource) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram6.Background = (_convertSourceIDToButton(state.USK1FillSource) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram7.Background = (_convertSourceIDToButton(state.USK1FillSource) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram8.Background = (_convertSourceIDToButton(state.USK1FillSource) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;

            UpdatePatternSettingUI(state);
        }

        private void UpdatePatternSettingUI(BMDSwitcherState state)
        {
            // update pattern params on UI
            // TODO: determine if we need to temp. disable events on controls so they don't get us stuck in a change loop

            Brush selected = new SolidColorBrush(Colors.Orange);
            Brush gray = new SolidColorBrush(Color.FromRgb(0x97, 0x97, 0x97));

            // select which pattern is active
            ptn_vbar.Fill = state.PATTERNSettings.PatternType == "h-bar" ? selected : gray;
            ptn_hbar.Fill = state.PATTERNSettings.PatternType == "v-bar" ? selected : gray;
            ptn_vbarn.Fill = state.PATTERNSettings.PatternType == "h-barn" ? selected : gray;
            ptn_hbarn.Fill = state.PATTERNSettings.PatternType == "v-barn" ? selected : gray;
            ptn_tbox.Fill = state.PATTERNSettings.PatternType == "t-box" ? selected : gray;
            ptn_circle_iris.Fill = state.PATTERNSettings.PatternType == "circle-iris" ? selected : gray;
            ptn_diamond_iris.Fill = state.PATTERNSettings.PatternType == "diamond-iris" ? selected : gray;
            ptn_ldiag.Fill = state.PATTERNSettings.PatternType == "l-diag" ? selected : gray;
            ptn_rdiag.Fill = state.PATTERNSettings.PatternType == "r-diag" ? selected : gray;
            ptn_rect_iris.Fill = state.PATTERNSettings.PatternType == "rect-iris" ? selected : gray;
            ptn_tl_box.Fill = state.PATTERNSettings.PatternType == "tl-box" ? selected : gray;
            ptn_tc_box.Fill = state.PATTERNSettings.PatternType == "tc-box" ? selected : gray;
            ptn_tr_box.Fill = state.PATTERNSettings.PatternType == "tr-box" ? selected : gray;
            ptn_rc_box.Fill = state.PATTERNSettings.PatternType == "rc-box" ? selected : gray;
            ptn_br_box.Fill = state.PATTERNSettings.PatternType == "br-box" ? selected : gray;
            ptn_bc_box.Fill = state.PATTERNSettings.PatternType == "bc-box" ? selected : gray;
            ptn_bl_box.Fill = state.PATTERNSettings.PatternType == "bl-box" ? selected : gray;
            ptn_lc_box.Fill = state.PATTERNSettings.PatternType == "lc-box" ? selected : gray;

            // update other params
            tbSize.Text = $"{state.PATTERNSettings.Size:0.00}";
            tbSym.Text = $"{state.PATTERNSettings.Symmetry:0.00}";
            tbSoft.Text = $"{state.PATTERNSettings.Softness:0.00}";
            tbX.Text = $"{state.PATTERNSettings.XOffset:0.00}";
            tbY.Text = $"{state.PATTERNSettings.YOffset:0.00}";

            BtnINVERT.Foreground = state.PATTERNSettings.Inverted ? Brushes.Orange : Brushes.White;

            UIDriven = false;

            slide_x.Value = state.PATTERNSettings.XOffset;
            slide_y.Value = state.PATTERNSettings.YOffset;
            slide_size.Value = state.PATTERNSettings.Size;
            slide_sym.Value = state.PATTERNSettings.Symmetry;
            slide_sharp.Value = state.PATTERNSettings.Softness;

            UIDriven = true;
        }

        public void ShowShortcuts(bool show)
        {
            var vis = show ? Visibility.Visible : Visibility.Hidden;
            ksc_pf1.Visibility = vis;
            ksc_pf2.Visibility = vis;
            ksc_pf3.Visibility = vis;
            ksc_pf4.Visibility = vis;
            ksc_pf5.Visibility = vis;
            ksc_pf6.Visibility = vis;
            ksc_pf7.Visibility = vis;
            ksc_pf8.Visibility = vis;
        }

        private void ClickPIP1(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(1));
        }
        private void ClickPIP2(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(2));
        }
        private void ClickPIP3(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(3));
        }
        private void ClickPIP4(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(4));
        }
        private void ClickPIP5(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(5));
        }
        private void ClickPIP6(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(6));
        }
        private void ClickPIP7(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(7));
        }
        private void ClickPIP8(object sender, RoutedEventArgs e)
        {
            _switcher?.PerformUSK1FillSourceSelect(_convertButtonToSourceID(8));
        }

        internal void UpdateButtonLabels(List<ButtonSourceMapping> routing)
        {
            foreach (var btn in routing)
            {
                switch (btn.ButtonId)
                {
                    case 1:
                        BtnPIPFillProgram1.Content = btn.ButtonName;
                        break;
                    case 2:
                        BtnPIPFillProgram2.Content = btn.ButtonName;
                        break;
                    case 3:
                        BtnPIPFillProgram3.Content = btn.ButtonName;
                        break;
                    case 4:
                        BtnPIPFillProgram4.Content = btn.ButtonName;
                        break;
                    case 5:
                        BtnPIPFillProgram5.Content = btn.ButtonName;
                        break;
                    case 6:
                        BtnPIPFillProgram6.Content = btn.ButtonName;
                        break;
                    case 7:
                        BtnPIPFillProgram7.Content = btn.ButtonName;
                        break;
                    case 8:
                        BtnPIPFillProgram8.Content = btn.ButtonName;
                        break;
                }
            }

        }

        private void center_y(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var val = _activePattern.Copy();
            val.YOffset = 0.5;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void center_x(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var val = _activePattern.Copy();
            val.XOffset = 0.5;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void cener_sym(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var val = _activePattern.Copy();
            val.Symmetry = 0.5;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void ClickInvert(object sender, RoutedEventArgs e)
        {
            var val = _activePattern.Copy();
            val.Inverted = !_activePattern.Inverted;
            _switcher?.ConfigureUSK1PATTERN(val);
        }


        private void SetPatternType(string pattern)
        {
            // TODO: figure out if we should instead reset other params on changing type...
            var val = _activePattern.Copy();
            val.PatternType = pattern;
            _switcher?.ConfigureUSK1PATTERN(val);
        }

        private void setvbar(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("h-bar");
        }

        private void sethbar(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("v-bar");
        }

        private void setvbarn(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("h-barn");

        }
        private void sethbarn(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("v-barn");

        }

        private void settbar(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("t-box");
        }

        private void setciris(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("circle-iris");

        }

        private void setdiris(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("diamond-iris");
        }

        private void setldiag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("l-diag");
        }

        private void setrdiag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("r-diag");
        }

        private void setriris(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("rect-iris");
        }

        private void settl(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("tl-box");
        }

        private void settc(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("tc-box");
        }

        private void settr(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("tr-box");
        }

        private void setcr(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("rc-box");
        }

        private void setbr(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("br-box");
        }

        private void setbc(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("bc-box");
        }

        private void setbl(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("bl-box");
        }

        private void setcl(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetPatternType("lc-box");
        }

    }
}
