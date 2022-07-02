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

        public PilotCamPreview()
        {
            InitializeComponent();
        }
        internal void UpdateUI(IPilotAction action)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateUI(action));
                return;
            }

            g_back.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));

            tbName.Text = action.CamName.ToUpper();
            tbPstName.Text = action.PresetName;
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
                    tbStatus.Text = "-----";
                    break;
            }

            tbaltstd.Text = "ALT";
            tbPstAlt.Text = action.AltName;

        }

        internal void ClearUI(string cname)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => ClearUI(cname));
                return;
            }

            g_back.Background = new SolidColorBrush(Color.FromRgb(20, 20, 20));

            tbName.Text = cname;

            tbPstName.Text = "";
            tbPstInfo.Text = "";

            tbStatus.Text = "";

            tbaltstd.Text = "";
            tbPstAlt.Text = "";

        }
    }
}
