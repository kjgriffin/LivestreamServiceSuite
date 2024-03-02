using IntegratedPresenterAPIInterop;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Integrated_Presenter.ViewModels
{
    /// <summary>
    /// Interaction logic for AutomationActionMonitor.xaml
    /// </summary>
    public partial class AutomationActionMonitor : UserControl
    {
        TrackedAutomationAction _action;

        /// <summary>
        /// DUMMY FOR TEST ONLY
        /// </summary>
        public AutomationActionMonitor()
        {
            InitializeComponent();

        }

        public AutomationActionMonitor(TrackedAutomationAction action, Dictionary<string, bool> conditionals)
        {
            InitializeComponent();
            _action = action;

            // Update View
            string display = string.IsNullOrEmpty(_action?.Action.Message) ? _action?.Action.Action.ToString() : _action?.Action.Message;
            tbMessage.Text = display;

            //tbCommand.Text = _action?.Action?.Action.ToString();

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

            bool willRun = _action?.Action?.ExpectedConditions?.HasConditions == true ? false : true;
            spReqCond.Children.Clear();

            SolidColorBrush green = new SolidColorBrush(Color.FromRgb(3, 207, 8));
            SolidColorBrush red = new SolidColorBrush(Color.FromRgb(192, 0, 0));

            int i = 0;
            foreach (var pexpr in _action?.Action?.ExpectedConditions?.Products)
            {
                // this is the evaluation of the entire term
                bool termValue = SumOfProductExpression.EvaluateProductTerm(pexpr, conditionals);
                willRun |= termValue;

                TextBlock tbProduct = new TextBlock();
                tbProduct.Margin = new Thickness(1);
                tbProduct.FontSize = 20;
                tbProduct.FontWeight = FontWeights.Bold;


                TextDecoration underline = new TextDecoration(TextDecorationLocation.Underline, termValue ? new Pen(green, 2) : new Pen(red, 2), 2, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);

                tbProduct.TextDecorations.Add(underline);

                // keep the evaluation of each factor within the term
                int j = 1;
                int cnt = pexpr.Count;
                foreach (var cfactor in pexpr)
                {
                    var cfval = false;
                    if (conditionals.TryGetValue(cfactor.Key, out var cval))
                    {
                        cfval = (cval == cfactor.Value);
                    }

                    // draw it with the current state
                    Run tbCond = new Run($"{(cfactor.Value == false ? "!" : "")}{cfactor.Key}");
                    if (cfval)
                    {
                        tbCond.Foreground = green;
                    }
                    else
                    {
                        tbCond.Foreground = red;
                    }
                    tbProduct.Inlines.Add(tbCond);

                    if (j < cnt)
                    {
                        Run tbProdOp = new Run("*");
                        tbProdOp.Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                        tbProduct.Inlines.Add(tbProdOp);
                    }

                    j++;
                }

                spReqCond.Children.Add(tbProduct);

                if (i < _action?.Action?.ExpectedConditions?.Products?.Count - 1)
                {
                    TextBlock tbSum = new TextBlock();
                    tbSum.Text = "+";
                    tbSum.Foreground = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                    tbSum.FontSize = 20;
                    tbSum.FontWeight = FontWeights.Bold;
                    spReqCond.Children.Add(tbSum);
                }
                i++;
            }

            if (!willRun)
            {
                imgCondIcon.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Icons/RedNoEntry.png"));
                imgCondBrush.Color = Color.FromRgb(0xcf, 0x3a, 0x53);
            }
            else if (_action?.Action?.ExpectedConditions?.HasConditions == true)
            {
                imgCondIcon.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Icons/GreenPlay.png"));
                imgCondBrush.Color = Color.FromRgb(0x51, 0xdb, 0x8f);
            }
            else
            {
                imgCondIcon.ImageSource = null;
                imgCondBrush.Color = Colors.Transparent;
            }
        }

    }
}
