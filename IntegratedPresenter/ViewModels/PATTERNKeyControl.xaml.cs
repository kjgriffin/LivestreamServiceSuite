using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;

using SwitcherControl.BMDSwitcher;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public PATTERNKeyControl()
        {
            InitializeComponent();
            ShowShortcuts(false);
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
            // fill source
            BtnPIPFillProgram1.Background = (_convertSourceIDToButton(state.USK1FillSource) == 1 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram2.Background = (_convertSourceIDToButton(state.USK1FillSource) == 2 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram3.Background = (_convertSourceIDToButton(state.USK1FillSource) == 3 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram4.Background = (_convertSourceIDToButton(state.USK1FillSource) == 4 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram5.Background = (_convertSourceIDToButton(state.USK1FillSource) == 5 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram6.Background = (_convertSourceIDToButton(state.USK1FillSource) == 6 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram7.Background = (_convertSourceIDToButton(state.USK1FillSource) == 7 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;
            BtnPIPFillProgram8.Background = (_convertSourceIDToButton(state.USK1FillSource) == 8 ? Application.Current.FindResource("RedLight") : Application.Current.FindResource("GrayLight")) as RadialGradientBrush;

            // pattern params

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
    }
}
