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

        bool hasAction = false;

        public PilotCamPreview()
        {
            InitializeComponent();
            UpdateOnAirWarning(false);
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
                tbOnAir.Visibility = Visibility.Visible;
            }
            else
            {
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

            g_back.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));


            tbName.Text = action.CamName.ToUpper();
            tbPstName.Text = splitify(action.PresetName);
            tbPstInfo.Text = action.DisplayInfo;

            switch (action.Status)
            {
                case "READY":
                    tbStatus.Text = "READY";
                    tbStatus.Foreground = Brushes.Gray;
                    break;

                case "STARTED":
                    tbStatus.Text = "RUNNING";
                    tbStatus.Foreground = Brushes.Orange;
                    break;

                case "DONE":
                    tbStatus.Text = "DONE";
                    tbStatus.Foreground = Brushes.LimeGreen;
                    break;

                case "FAILED":
                    tbStatus.Text = "FAILED";
                    tbStatus.Foreground = Brushes.Red;
                    break;

                default:
                    tbStatus.Text = action.Status;
                    tbStatus.Foreground = Brushes.Yellow;
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

            g_back.Background = new SolidColorBrush(Color.FromRgb(20, 20, 20));

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
    }
}
