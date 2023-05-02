using ATEMSharedState.SwitcherState;

using IntegratedPresenter.BMDSwitcher.Config;

using SharedPresentationAPI.Presentation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Integrated_Presenter.ViewModels
{

    public enum PilotMode
    {
        STD,
        LAST,
        ALT,
        EMG,
    }

    public delegate void PilotModeChangedArgs(PilotMode newMode);
    public delegate void TogglePilotModeArgs();

    /// <summary>
    /// Interaction logic for PilotUI.xaml
    /// </summary>
    public partial class PilotUI : UserControl
    {

        Dictionary<int, string> cams = new Dictionary<int, string> { [1] = "pulpit", [2] = "center", [3] = "lectern", [4] = "organ" };

        public PilotUI()
        {
            InitializeComponent();
            pvCurrent_plt.OnUserRequestForManualReRun += PvCurrent_plt_OnUserRequestForManualReRun;
            pvCurrent_ctr.OnUserRequestForManualReRun += PvCurrent_ctr_OnUserRequestForManualReRun;
            pvCurrent_lec.OnUserRequestForManualReRun += PvCurrent_lec_OnUserRequestForManualReRun;
            pvCurrent_org.OnUserRequestForManualReRun += PvCurrent_lec_OnUserRequestForManualReRun;

            pvCurrent_plt.OnUserRequestForZoomBump += PvCurrent_plt_OnUserRequestForZoomBump;
            pvCurrent_ctr.OnUserRequestForZoomBump += PvCurrent_ctr_OnUserRequestForZoomBump;
            pvCurrent_lec.OnUserRequestForZoomBump += PvCurrent_lec_OnUserRequestForZoomBump;
            pvCurrent_org.OnUserRequestForZoomBump += PvCurrent_org_OnUserRequestForZoomBump;

            pvNext_plt.HideManualReRun();
            pvNext_ctr.HideManualReRun();
            pvNext_lec.HideManualReRun();
            pvNext_org.HideManualReRun();
        }

        private void PvCurrent_org_OnUserRequestForZoomBump(object sender, (int dir, int ammount) e)
        {
            OnUserRequestForManualZoomBump?.Invoke(this, ("organ", e.dir, e.ammount));
        }

        private void PvCurrent_lec_OnUserRequestForZoomBump(object sender, (int dir, int ammount) e)
        {
            OnUserRequestForManualZoomBump?.Invoke(this, ("lectern", e.dir, e.ammount));
        }

        private void PvCurrent_ctr_OnUserRequestForZoomBump(object sender, (int dir, int ammount) e)
        {
            OnUserRequestForManualZoomBump?.Invoke(this, ("center", e.dir, e.ammount));
        }

        private void PvCurrent_plt_OnUserRequestForZoomBump(object sender, (int dir, int ammount) e)
        {
            OnUserRequestForManualZoomBump?.Invoke(this, ("pulpit", e.dir, e.ammount));
        }

        private void PvCurrent_lec_OnUserRequestForManualReRun(object sender, EventArgs e)
        {
            OnUserRequestForManualReRun?.Invoke(this, 3);
        }

        private void PvCurrent_ctr_OnUserRequestForManualReRun(object sender, EventArgs e)
        {
            OnUserRequestForManualReRun?.Invoke(this, 2);
        }

        private void PvCurrent_plt_OnUserRequestForManualReRun(object sender, EventArgs e)
        {
            OnUserRequestForManualReRun?.Invoke(this, 1);
        }
        private void PvCurrent_org_OnUserRequestForManualReRun(object sender, EventArgs e)
        {
            OnUserRequestForManualReRun?.Invoke(this, 4);
        }


        public event PilotModeChangedArgs OnModeChanged;
        public event TogglePilotModeArgs OnTogglePilotMode;
        public event EventHandler<int> OnUserRequestForManualReRun;
        public event EventHandler<(string cam, int dir, int zms)> OnUserRequestForManualZoomBump;

        Dictionary<string, IPilotAction> _lastNamedCache = new Dictionary<string, IPilotAction>();
        Dictionary<string, IPilotAction> _emergencyActions = new Dictionary<string, IPilotAction>();

        List<IPilotAction> _curentActions = new List<IPilotAction>();
        int _slideNum;
        PilotMode mode = PilotMode.STD;


        internal void ClearState(Dictionary<string, IPilotAction> calculatedLast, Dictionary<string, IPilotAction> calculatedEmg)
        {
            //_lastNamedCache?.Clear();
            //_emergencyActions?.Clear();
            _lastNamedCache = calculatedLast;
            _emergencyActions = calculatedEmg;
            _curentActions?.Clear();
            _slideNum = -1;
        }


        private string GetSubInfoForCam(string camName, PilotMode mode)
        {
            switch (mode)
            {
                case PilotMode.STD:
                    _emergencyActions.TryGetValue(camName, out var a);
                    return a?.PresetName ?? "";
                case PilotMode.LAST:
                    string x = "";
                    x += _curentActions?.FirstOrDefault(x => x.CamName == camName)?.PresetName + "\n" ?? "-\n";
                    _emergencyActions.TryGetValue(camName, out var e);
                    x += e?.PresetName ?? "-";
                    return x;
                case PilotMode.EMG:
                    return _curentActions?.FirstOrDefault(x => x.CamName == camName)?.PresetName ?? "-";
            }
            return "";
        }


        public void UpdateUI(bool PilotEnabled, List<IPilotAction> currentSlideActions, List<IPilotAction> nextSlideActions, List<IPilotAction> emergencyActions, int cSlideNum, PilotMode mode)
        {

            foreach (var ea in emergencyActions)
            {
                _emergencyActions[ea.CamName] = ea;
            }

            this.mode = mode;
            ellipseState.Fill = PilotEnabled ? Brushes.LimeGreen : Brushes.Red;

            if (mode == PilotMode.STD)
            {
                tbSTDmode.FontWeight = FontWeights.Bold;
                tbSTDmode.Foreground = Brushes.White;

                tbLASTmode.FontWeight = FontWeights.Regular;
                tbLASTmode.Foreground = Brushes.Gray;

                tbEMGmode.FontWeight = FontWeights.Regular;
                tbEMGmode.Foreground = Brushes.Gray;
            }
            else if (mode == PilotMode.LAST)
            {
                tbSTDmode.FontWeight = FontWeights.Regular;
                tbSTDmode.Foreground = Brushes.Gray;

                tbLASTmode.FontWeight = FontWeights.Bold;
                tbLASTmode.Foreground = Brushes.Orange;

                tbEMGmode.FontWeight = FontWeights.Regular;
                tbEMGmode.Foreground = Brushes.Gray;
            }
            else if (mode == PilotMode.EMG)
            {
                tbSTDmode.FontWeight = FontWeights.Regular;
                tbSTDmode.Foreground = Brushes.Gray;

                tbLASTmode.FontWeight = FontWeights.Regular;
                tbLASTmode.Foreground = Brushes.Gray;

                tbEMGmode.FontWeight = FontWeights.Bold;
                tbEMGmode.Foreground = Brushes.OrangeRed;
            }

            // if we get an update that invalidates the slide number for the curent slide, then we should change the curent cache
            // and reset all the status
            if (cSlideNum != _slideNum)
            {
                if (_curentActions.Any())
                {
                    // replace them
                    foreach (var a in _curentActions)
                    {
                        _lastNamedCache[a.CamName] = a;
                    }
                }
                _curentActions = currentSlideActions;
                _slideNum = cSlideNum;

                foreach (var action in _curentActions)
                {
                    action.Reset();
                }
            }

            List<IPilotAction> displayActions = new List<IPilotAction>();
            switch (mode)
            {
                case PilotMode.STD:
                    displayActions = _curentActions;
                    break;
                case PilotMode.LAST:
                    displayActions = _lastNamedCache.Values.ToList();
                    break;
                case PilotMode.ALT:
                    break;
                case PilotMode.EMG:
                    displayActions = _emergencyActions.Values.ToList();
                    break;
            }


            UpdateForCam("pulpit", pvCurrent_plt, displayActions, GetSubInfoForCam("pulpit", mode), true);
            UpdateForCam("center", pvCurrent_ctr, displayActions, GetSubInfoForCam("center", mode), true);
            UpdateForCam("lectern", pvCurrent_lec, displayActions, GetSubInfoForCam("lectern", mode), true);
            UpdateForCam("organ", pvCurrent_org, displayActions, GetSubInfoForCam("organ", mode), true);

            UpdateForCam("pulpit", pvNext_plt, nextSlideActions, "", false);
            UpdateForCam("center", pvNext_ctr, nextSlideActions, "", false);
            UpdateForCam("lectern", pvNext_lec, nextSlideActions, "", false);
            UpdateForCam("organ", pvNext_org, nextSlideActions, "", false);

        }

        private void UpdateForCam(string cam, PilotCamPreview ctrl, List<IPilotAction> actions, string subInfo, bool showSubInfo)
        {
            var action = actions.FirstOrDefault(x => x.CamName == cam);
            if (action != null)
            {
                ctrl.UpdateUI(action, mode, subInfo, showSubInfo);
            }
            else
            {
                ctrl.ClearUI(cam, mode, subInfo, showSubInfo);
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
            var allactions = _curentActions.Concat(_lastNamedCache.Values)
                                           .Concat(_emergencyActions.Values);
            var action = allactions.FirstOrDefault(x => x.CamName == camName && x.ReqIds.Contains(guid));
            if (action == null)
            {
                return;
            }

            action.StatusUpdate(args);

            if (camName == "pulpit")
            {
                pvCurrent_plt.UpdateUI(action, mode, GetSubInfoForCam(camName, mode), true);
            }
            if (camName == "center")
            {
                pvCurrent_ctr.UpdateUI(action, mode, GetSubInfoForCam(camName, mode), true);
            }
            if (camName == "lectern")
            {
                pvCurrent_lec.UpdateUI(action, mode, GetSubInfoForCam(camName, mode), true);
            }
            if (camName == "organ")
            {
                pvCurrent_org.UpdateUI(action, mode, GetSubInfoForCam(camName, mode), true);
            }
        }

        internal void FireLast(int id, CCUI_UI.ICCPUPresetMonitor_Executor driver)
        {
            if (cams.TryGetValue(id, out string camName))
            {
                try
                {
                    IPilotAction action = null;
                    if (mode == 0)
                    {
                        action = _curentActions.FirstOrDefault(x => x.CamName == camName);
                    }
                    else if (mode == PilotMode.LAST)
                    {
                        _lastNamedCache.TryGetValue(camName, out action);
                    }
                    else if (mode == PilotMode.EMG)
                    {
                        _emergencyActions.TryGetValue(camName, out action);
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
            if (mode == PilotMode.LAST || mode == PilotMode.EMG)
            {
                pvCurrent_plt.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "left").First().PhysicalInputId);
                pvCurrent_ctr.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "center").First().PhysicalInputId);
                pvCurrent_lec.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "right").First().PhysicalInputId);
                pvCurrent_org.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "organ").First().PhysicalInputId);
            }
            else
            {
                pvCurrent_plt.UpdateOnAirWarning(false);
                pvCurrent_ctr.UpdateOnAirWarning(false);
                pvCurrent_lec.UpdateOnAirWarning(false);
                pvCurrent_org.UpdateOnAirWarning(false);
            }


            pvNext_plt.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "left").First().PhysicalInputId && nextSlideGoesLive);
            pvNext_ctr.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "center").First().PhysicalInputId && nextSlideGoesLive);
            pvNext_lec.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "right").First().PhysicalInputId && nextSlideGoesLive);
            pvNext_org.UpdateOnAirWarning(state.ProgramID == config.Routing.Where(r => r.KeyName == "organ").First().PhysicalInputId && nextSlideGoesLive);
        }

        private void ellipseState_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnTogglePilotMode?.Invoke();
        }

        private void tbSTDmode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnModeChanged?.Invoke(PilotMode.STD);
        }

        private void tbEMGmode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnModeChanged?.Invoke(PilotMode.EMG);
        }

        private void tbLASTmode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnModeChanged?.Invoke(PilotMode.LAST);
        }
    }
}
