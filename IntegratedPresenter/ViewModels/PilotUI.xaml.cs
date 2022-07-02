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
    /// Interaction logic for PilotUI.xaml
    /// </summary>
    public partial class PilotUI : UserControl
    {
        public PilotUI()
        {
            InitializeComponent();
        }

        List<IPilotAction> _curentActions = new List<IPilotAction>();
        int _slideNum;

        public void UpdateUI(bool PilotEnabled, List<IPilotAction> currentSlideActions, List<IPilotAction> nextSlideActions, int cSlideNum)
        {
            ellipseState.Fill = PilotEnabled ? Brushes.LimeGreen : Brushes.Red;

            UpdateForCam("pulpit", pvCurrent_plt, currentSlideActions);
            UpdateForCam("center", pvCurrent_ctr, currentSlideActions);
            UpdateForCam("lectern", pvCurrent_lec, currentSlideActions);

            UpdateForCam("pulpit", pvNext_plt, nextSlideActions);
            UpdateForCam("center", pvNext_ctr, nextSlideActions);
            UpdateForCam("lectern", pvNext_lec, nextSlideActions);


            // if we get an update that invalidates the slide number for the curent slide, then we should change the curent cache
            // and reset all the status
            if (cSlideNum != _slideNum)
            {
                _curentActions = currentSlideActions;
                _slideNum = cSlideNum;

                foreach (var action in _curentActions)
                {
                    action.Reset();
                }
            }
        }

        private void UpdateForCam(string cam, PilotCamPreview ctrl, List<IPilotAction> actions)
        {
            var action = actions.FirstOrDefault(x => x.CamName == cam);
            if (action != null)
            {
                ctrl.UpdateUI(action);
            }
            else
            {
                ctrl.ClearUI(cam);
            }
        }

        internal void UpdateUIStatus(string camName, string[] args)
        {
            Guid guid = Guid.Empty;
            if (!(args.Length > 0 && Guid.TryParse(args.Last(), out guid) && guid != Guid.Empty))
            {
                // ignore it because we don't understand what to do
                // it may be the result of a stale command...
                return;
            }
            var action = _curentActions.FirstOrDefault(x => x.CamName == camName && x.ReqIds.Contains(guid));
            if (action == null)
            {
                return;
            }

            action.StatusUpdate(args);

            if (camName == "pulpit")
            {
                pvCurrent_plt.UpdateUI(action);
            }
            if (camName == "center")
            {
                pvCurrent_ctr.UpdateUI(action);
            }
            if (camName == "lectern")
            {
                pvCurrent_lec.UpdateUI(action);
            }
        }
    }
}
