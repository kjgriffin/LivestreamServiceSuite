using IntegratedPresenter.Main;

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
    /// Interaction logic for AutomationActionMonitor.xaml
    /// </summary>
    public partial class AutomationActionMonitor : UserControl
    {
        TrackedAutomationAction _action;
        public AutomationActionMonitor(TrackedAutomationAction action)
        {
            InitializeComponent();
            _action = action;

            // Update View
            tbMessage.Text = _action?.Action?.Message;

            tbCommand.Text = _action?.Action?.Action.ToString();

            switch (_action?.State)
            {
                case TrackedActionState.Ready:
                    imgStatusIcon.Source = null;
                    break;
                case TrackedActionState.Started:
                    imgStatusIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/OrangeSandTimer.png"));
                    break;
                case TrackedActionState.Done:
                    imgStatusIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreenCheck.png"));
                    break;
            }

            switch (_action?.RunType)
            {
                case TrackedActionRunType.Setup:
                    imgTypeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BlueBefore.png"));
                    break;
                case TrackedActionRunType.Main:
                    imgTypeIcon.Source = null;
                    break;
                case TrackedActionRunType.Note:
                    imgStatusIcon.Source = null;
                    imgTypeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/YellowWarn.png"));
                    break;
            }
        }

    }
}
