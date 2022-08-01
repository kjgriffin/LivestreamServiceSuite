using Integrated_Presenter.Presentation;

using IntegratedPresenter.BMDSwitcher.Config;

using SwitcherControl.BMDSwitcher;

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

        List<IPilotAction> _lastCache = new List<IPilotAction>();
        List<IPilotAction> _curentActions = new List<IPilotAction>();
        int _slideNum;
        int mode = 0;

        public void UpdateUI(bool PilotEnabled, List<IPilotAction> currentSlideActions, List<IPilotAction> nextSlideActions, int cSlideNum, int mode)
        {
            this.mode = mode;
            ellipseState.Fill = PilotEnabled ? Brushes.LimeGreen : Brushes.Red;

            if (mode == 0)
            {
                tbSTDmode.FontWeight = FontWeights.Bold;
                tbSTDmode.Foreground = Brushes.White;

                tbLASTmode.FontWeight = FontWeights.Regular;
                tbLASTmode.Foreground = Brushes.Gray;
            }
            else if (mode == -1)
            {
                tbSTDmode.FontWeight = FontWeights.Regular;
                tbSTDmode.Foreground = Brushes.Gray;

                tbLASTmode.FontWeight = FontWeights.Bold;
                tbLASTmode.Foreground = Brushes.Orange;
            }

            List<IPilotAction> displayActions = mode == 0 ? currentSlideActions : _lastCache;

            UpdateForCam("pulpit", pvCurrent_plt, displayActions);
            UpdateForCam("center", pvCurrent_ctr, displayActions);
            UpdateForCam("lectern", pvCurrent_lec, displayActions);

            UpdateForCam("pulpit", pvNext_plt, nextSlideActions);
            UpdateForCam("center", pvNext_ctr, nextSlideActions);
            UpdateForCam("lectern", pvNext_lec, nextSlideActions);


            // if we get an update that invalidates the slide number for the curent slide, then we should change the curent cache
            // and reset all the status
            if (cSlideNum != _slideNum)
            {
                if (_curentActions.Any())
                {
                    // replace them
                    _lastCache = _curentActions;
                }
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
            var action = _curentActions.Concat(_lastCache).FirstOrDefault(x => x.CamName == camName && x.ReqIds.Contains(guid));
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

        internal void FireLast(int id, CCUI_UI.ICCPUPresetMonitor_Executor driver)
        {
            Dictionary<int, string> cams = new Dictionary<int, string> { [1] = "pulpit", [2] = "center", [3] = "lectern" };
            if (cams.TryGetValue(id, out string camName))
            {
                try
                {
                    IPilotAction action = null;
                    if (mode == 0)
                    {
                        action = _curentActions.FirstOrDefault(x => x.CamName == camName);
                    }
                    else if (mode == -1)
                    {
                        action = _lastCache.FirstOrDefault(x => x.CamName == camName);
                    }
                    action?.Reset();
                    action?.Execute(driver, 15);
                }
                catch (Exception ex)
                {

                }
            }
        }

        internal void FireOnSwitcherStateChangedForAutomation(BMDSwitcherState state, BMDSwitcherConfigSettings config, bool nextSlideGoesLive)
        {
            if (mode == -1)
            {
                pvCurrent_plt.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "left").First().PhysicalInputId);
                pvCurrent_ctr.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "center").First().PhysicalInputId);
                pvCurrent_lec.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "right").First().PhysicalInputId);
            }
            else
            {
                pvCurrent_plt.UpdateOnAirWarning(false);
                pvCurrent_ctr.UpdateOnAirWarning(false);
                pvCurrent_lec.UpdateOnAirWarning(false);
            }


            pvNext_plt.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "left").First().PhysicalInputId && nextSlideGoesLive);
            pvNext_ctr.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "center").First().PhysicalInputId && nextSlideGoesLive);
            pvNext_lec.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "right").First().PhysicalInputId && nextSlideGoesLive);
        }
    }
}
