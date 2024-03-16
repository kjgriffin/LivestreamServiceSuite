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

namespace CCUI_UI
{
    /// <summary>
    /// Interaction logic for StatusItem.xaml
    /// </summary>
    public partial class StatusItem : UserControl
    {
        Brush redBrush;
        Brush greenBrush;
        public StatusItem()
        {
            InitializeComponent();
            redBrush = FindResource("redBrush") as Brush;
            greenBrush = FindResource("greenBrush") as Brush;
        }

        internal void Build(CCU_Report report)
        {
            if (!CheckAccess())
                Dispatcher.Invoke(Build, report);

            tbTime.Text = report.Timestamp;
            tbCmd.Text = report.CMDName;
            tbStatus.Text = report.CMDStatus;
            tbStatus.Foreground = greenBrush;
            if (!report.OK)
            {
                tbStatus.Foreground = redBrush;
            }
            tbUID.Text = report.UID.ToString();
        }
    }
}
