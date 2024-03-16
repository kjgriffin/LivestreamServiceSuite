#define ZOOMBUMP_FEATURE

using SharedPresentationAPI.Presentation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for PilotSlidePreview.xaml
    /// </summary>
    public partial class PilotCamPreview : UserControl
    {

        public event EventHandler OnUserRequestForManualReRun;

        public event EventHandler<(int dir, int ammount)> OnUserRequestForZoomBump;

        bool hasAction = false;

        [Flags]
        enum WSTATES
        {
            CLEAR = 0,
            // run states
            RUN_READY = 1,
            RUN_RUNNING = 2,
            RUN_DONE = 4,
            RUN_FAILED = 8,

            WARN_ONAIR = 16,
        }

        WSTATES _displayStates = WSTATES.CLEAR;

        WSTATES DisplayState
        {
            get => _displayStates;
            set
            {
                _displayStates = value;

                // update state UI
                grStatus.Dispatcher.Invoke(() =>
                {
                    // failure overrides all
                    bdrOnAir.Background = dark;
                    tbStatus.Foreground = white;
                    if (_displayStates.HasFlag(WSTATES.RUN_FAILED))
                    {
                        grStatus.Background = red;
                    }
                    else if (_displayStates.HasFlag(WSTATES.WARN_ONAIR))
                    {
                        bdrOnAir.Background = red;
                        grStatus.Background = yellow;
                        tbStatus.Foreground = dark;
                    }
                    else if (_displayStates.HasFlag(WSTATES.RUN_RUNNING))
                    {
                        grStatus.Background = green;
                    }
                    else if (_displayStates.HasFlag(WSTATES.RUN_DONE))
                    {
                        grStatus.Background = darkGreen;
                    }
                    else if (_displayStates.HasFlag(WSTATES.RUN_READY))
                    {
                        grStatus.Background = blue;
                    }
                    else
                    {
                        grStatus.Background = dark;
                    }
                });
            }
        }

        Brush blue;
        Brush teal;
        Brush green;
        Brush darkGreen;
        Brush red;
        Brush yellow;
        Brush white;
        Brush gray;
        Brush dark;

        public PilotCamPreview()
        {
            InitializeComponent();
            UpdateOnAirWarning(false);

            blue = FindResource("lightBlueBrush") as Brush;
            teal = FindResource("tealBrush") as Brush;
            green = FindResource("greenBrush") as Brush;
            darkGreen = FindResource("darkGreenBrush") as Brush;
            red = FindResource("redBrush") as Brush;
            yellow = FindResource("yellowBrush") as Brush;
            white = FindResource("whiteBrush") as Brush;
            gray = FindResource("grayBrush") as Brush;
            dark = FindResource("darkBrush") as Brush;
        }

        public void HideManualReRun()
        {
            btnReFire.IsEnabled = false;
            btnReFire.Visibility = Visibility.Hidden;
            btnZIN.IsEnabled = false;
            btnZIN.Visibility = Visibility.Hidden;
            btnZOUT.IsEnabled = false;
            btnZOUT.Visibility = Visibility.Hidden;
        }

        public void EnableCarlsZoom()
        {
#if ZOOMBUMP_FEATURE
            btnZIN.IsEnabled = true;
            btnZIN.Visibility = Visibility.Visible;
            btnZOUT.IsEnabled = true;
            btnZOUT.Visibility = Visibility.Visible;
#else
            btnZIN.IsEnabled = false;
            btnZIN.Visibility = Visibility.Hidden;
            btnZOUT.IsEnabled = false;
            btnZOUT.Visibility = Visibility.Hidden;
#endif
        }

        List<string> splitify(string input)
        {
            List<string> res = new List<string>();
            if (input.Length <= 7)
            {
                res.Add(input);
            }
            else if (input.Length <= 14)
            {
                var l1 = string.Concat(input.Take(7));
                var l2 = string.Concat(input.Skip(7).Take(7));
                res.Add(l1);
                res.Add(l2);
            }
            else
            {
                // max 21 ??

                var l1 = string.Concat(input.Take(7));
                var l2 = string.Concat(input.Skip(7).Take(7));
                var l3 = string.Concat(input.Skip(14).Take(7));
                res.Add(l1);
                res.Add(l2);
                res.Add(l3);
            }
            return res;
        }

        internal void UpdateOnAirWarning(bool warn)
        {
            if (warn && hasAction)
            {
                DisplayState |= WSTATES.WARN_ONAIR;
                tbOnAir.Visibility = Visibility.Visible;
            }
            else
            {
                DisplayState &= ~WSTATES.WARN_ONAIR;
                tbOnAir.Visibility = Visibility.Hidden;
            }
        }

        internal void UpdateUI(IPilotAction action, PilotMode mode, string subInfo, bool showSubInfo)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateUI(action, mode, subInfo, showSubInfo));
                return;
            }

            if (action != null)
            {
                hasAction = true;
            }
            else
            {
                DisplayState = WSTATES.CLEAR;
            }


            tbName.Text = action.CamName.ToUpper();

            var splits = splitify(action.PresetName);
            tbPstName.Text = string.Join(Environment.NewLine, splits);


            tbPstInfo.Text = action.DisplayInfo;

            switch (action.Status)
            {
                case "READY":
                    tbStatus.Text = "READY";
                    //tbStatus.Foreground = Brushes.Gray;
                    DisplayState |= WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState &= ~WSTATES.RUN_FAILED;
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;

                case "STARTED":
                    tbStatus.Text = "RUNNING";
                    //tbStatus.Foreground = Brushes.Orange;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState |= WSTATES.RUN_RUNNING;
                    DisplayState &= ~WSTATES.RUN_FAILED;
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;

                case "DONE":
                    tbStatus.Text = "DONE";
                    //tbStatus.Foreground = Brushes.LimeGreen;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState &= ~WSTATES.RUN_FAILED;
                    DisplayState |= WSTATES.RUN_DONE;
                    break;

                case "FAILED":
                    tbStatus.Text = "FAILED";
                    //tbStatus.Foreground = Brushes.Red;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState |= WSTATES.RUN_FAILED;
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;

                default:
                    tbStatus.Text = action.Status;
                    //tbStatus.Foreground = Brushes.Yellow;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState |= WSTATES.RUN_FAILED; // mark failed??
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;
            }

            // if in LAST mode show std & emg
            // if in EMG mode show std
            // if std show EMG
            switch (mode)
            {
                case PilotMode.STD:
                    tbSubTitle.Text = showSubInfo ? "EMG" : "";
                    tbSubContent.Text = showSubInfo ? subInfo : "";
                    break;
                case PilotMode.LAST:
                    tbSubTitle.Text = showSubInfo ? "STD/EMG" : "";
                    tbSubContent.Text = showSubInfo ? subInfo : "";
                    break;
                case PilotMode.EMG:
                    tbSubTitle.Text = showSubInfo ? "STD" : "";
                    tbSubContent.Text = showSubInfo ? subInfo : "";
                    break;
            }
        }

        internal void ClearUI(string cname, PilotMode mode, string subInfo, bool showSubInfo)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => ClearUI(cname, mode, subInfo, showSubInfo));
                return;
            }

            hasAction = false;
            DisplayState = WSTATES.CLEAR;

            tbName.Text = cname;

            tbPstName.Text = "";
            tbPstInfo.Text = "";

            tbStatus.Text = "";

            if (showSubInfo)
            {
                switch (mode)
                {
                    case PilotMode.STD:
                        tbSubTitle.Text = "EMG";
                        tbSubContent.Text = subInfo;
                        break;
                    case PilotMode.LAST:
                        tbSubTitle.Text = "STD/EMG";
                        tbSubContent.Text = subInfo;
                        break;
                    case PilotMode.EMG:
                        tbSubTitle.Text = "STD";
                        tbSubContent.Text = subInfo;
                        break;
                }
            }
            else
            {
                tbSubTitle.Text = "";
                tbSubContent.Text = "";
            }


        }

        private void ClickReRun(object sender, RoutedEventArgs e)
        {
            OnUserRequestForManualReRun?.Invoke(this, new EventArgs());
        }

        private void ClickZOut(object sender, RoutedEventArgs e)
        {
            OnUserRequestForZoomBump?.Invoke(this, (-1, 10));
        }

        private void ClickZIn(object sender, RoutedEventArgs e)
        {
            OnUserRequestForZoomBump?.Invoke(this, (1, 10));
        }
    }
}
