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
                g_back.Dispatcher.Invoke(() =>
                {
                    // failure overrides all
                    if (_displayStates.HasFlag(WSTATES.RUN_FAILED))
                    {
                        g_back.Background = Brushes.Red;
                    }
                    else if (_displayStates.HasFlag(WSTATES.WARN_ONAIR))
                    {
                        g_back.Background = Brushes.OrangeRed;
                    }
                    else if (_displayStates.HasFlag(WSTATES.RUN_RUNNING))
                    {
                        g_back.Background = Brushes.LimeGreen;
                    }
                    else if (_displayStates.HasFlag(WSTATES.RUN_DONE))
                    {
                        g_back.Background = Brushes.Green;
                    }
                    else if (_displayStates.HasFlag(WSTATES.RUN_READY))
                    {
                        g_back.Background = Brushes.Teal;
                    }
                    else
                    {
                        g_back.Background = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                    }
                });
            }
        }

        public PilotCamPreview()
        {
            InitializeComponent();
            UpdateOnAirWarning(false);
        }

        public void HideManualReRun()
        {
            btnReFire.IsEnabled = false;
            btnReFire.Visibility = Visibility.Hidden;
        }

        string splitify(string input)
        {
            //if (input.Length > 12)
            //{
            //    return input.Substring(0, 6) + Environment.NewLine + input.Substring(6, 6) + Environment.NewLine + input.Substring(12);
            //}
            //if (input.Length > 6)
            //{
            //    return input.Substring(0, 6) + Environment.NewLine + input.Substring(6);
            //}
            //return input;

            // limit to 7 leters
            if (input.Length > 7)
            {
                return input.Substring(0, 7);
            }
            return input;
        }

        internal void UpdateOnAirWarning(bool warn)
        {
            if (warn && hasAction)
            {
                DisplayState |= WSTATES.WARN_ONAIR;
                //g_back.Background = Brushes.Red;
                tbOnAir.Visibility = Visibility.Visible;
            }
            else
            {
                DisplayState &= ~WSTATES.WARN_ONAIR;
                //g_back.Background = new SolidColorBrush(Color.FromRgb(55, 55, 55));
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

            //g_back.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));


            tbName.Text = action.CamName.ToUpper();
            tbPstName.Text = splitify(action.PresetName);
            tbPstInfo.Text = action.DisplayInfo;

            switch (action.Status)
            {
                case "READY":
                    tbStatus.Text = "READY";
                    tbStatus.Foreground = Brushes.Gray;
                    DisplayState |= WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState &= ~WSTATES.RUN_FAILED;
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;

                case "STARTED":
                    tbStatus.Text = "RUNNING";
                    tbStatus.Foreground = Brushes.Orange;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState |= WSTATES.RUN_RUNNING;
                    DisplayState &= ~WSTATES.RUN_FAILED;
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;

                case "DONE":
                    tbStatus.Text = "DONE";
                    tbStatus.Foreground = Brushes.LimeGreen;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState &= ~WSTATES.RUN_FAILED;
                    DisplayState |= WSTATES.RUN_DONE;
                    break;

                case "FAILED":
                    tbStatus.Text = "FAILED";
                    tbStatus.Foreground = Brushes.Red;
                    DisplayState &= ~WSTATES.RUN_READY;
                    DisplayState &= ~WSTATES.RUN_RUNNING;
                    DisplayState |= WSTATES.RUN_FAILED;
                    DisplayState &= ~WSTATES.RUN_DONE;
                    break;

                default:
                    tbStatus.Text = action.Status;
                    tbStatus.Foreground = Brushes.Yellow;
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

            //g_back.Background = new SolidColorBrush(Color.FromRgb(20, 20, 20));

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
            OnUserRequestForZoomBump?.Invoke(this, (-1, 100));
        }

        private void ClickZIn(object sender, RoutedEventArgs e)
        {
            OnUserRequestForZoomBump?.Invoke(this, (1, 100));
        }
    }
}
