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
        public AutomationActionMonitor(TrackedAutomationAction action, Dictionary<string, bool> conditionals)
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
                case TrackedActionState.Skipped:
                    imgStatusIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/YellowSkip.png"));
                    break;
            }

            switch (_action?.RunType)
            {
                case TrackedActionRunType.Setup:
                    if (_action?.State == TrackedActionState.Done || _action?.State == TrackedActionState.Skipped)
                    {
                        imgTypeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/LightBlueBefore.png"));
                    }
                    else
                    {
                        imgTypeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/BlueBefore.png"));
                    }
                    break;
                case TrackedActionRunType.Main:
                    imgTypeIcon.Source = null;
                    break;
                case TrackedActionRunType.Note:
                    imgStatusIcon.Source = null;
                    imgTypeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/YellowWarn.png"));
                    break;
            }

            bool willRun = true;
            spReqCond.Children.Clear();
            foreach (var condReq in _action?.Action?.Conditions.OrderBy(x => x.Key))
            {
                bool cval = false;
                if (conditionals.TryGetValue(condReq.Key, out var condVal))
                {
                    willRun &= (condVal == condReq.Value);
                    cval = (condVal == condReq.Value);
                }
                else
                {
                    willRun = false;
                }

                // draw it with the current state
                TextBlock tbCond = new TextBlock();
                tbCond.Text = $"{(condReq.Value == false ? "!" : "")}{condReq.Key}";
                tbCond.Margin = new Thickness(5);
                tbCond.FontSize = 20;
                tbCond.FontWeight = FontWeights.Bold;
                if (cval)
                {
                    tbCond.Foreground = new SolidColorBrush(Color.FromRgb(3, 207, 8));
                }
                else
                {
                    tbCond.Foreground = new SolidColorBrush(Color.FromRgb(192, 0, 0));
                }
                spReqCond.Children.Add(tbCond);
            }
            if (!willRun)
            {
                imgCondeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/RedNoEntry.png"));
            }
            else if (_action?.Action?.Conditions?.Any() == true)
            {
                imgCondeIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Icons/GreenPlay.png"));
            }
            else
            {
                imgCondeIcon.Source = null;
            }
        }

    }
}
