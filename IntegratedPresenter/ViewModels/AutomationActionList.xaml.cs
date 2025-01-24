using SharedPresentationAPI.Presentation;

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
    /// Interaction logic for AutomationActionList.xaml
    /// </summary>
    public partial class AutomationActionList : UserControl
    {
        public AutomationActionList()
        {
            InitializeComponent();
        }

        public void ShowActionsText(ISlide slide, Dictionary<string, bool> conditionals)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => ShowActionsText(slide, conditionals));
            }

            ClearActionPreviews();
            if (slide == null)
            {
                return;
            }

            foreach (var action in slide.SetupActions)
            {
                spSetup.Children.Add(new AutomationActionMonitor(action, conditionals));
            }

            foreach (var action in slide.Actions)
            {
                spMain.Children.Add(new AutomationActionMonitor(action, conditionals));
            }

            string seqType = slide.AutoOnly ? "AUTO SCRIPT" : "SCRIPT";
            tbSEQ.Text = $"[{seqType}] {slide.Title}";
        }

        public void ClearActionPreviews()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(ClearActionPreviews);
            }
            spSetup.Children.Clear();
            spMain.Children.Clear();
        }

    }
}
