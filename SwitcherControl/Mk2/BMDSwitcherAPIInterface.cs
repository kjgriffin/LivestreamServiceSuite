using BMDSwitcherAPI;

using SwitcherControl.Mk2.Monitors;

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace SwitcherControl.Mk2
{
    internal class BMDSwitcherAPIInterface
    {


        // managed interfaces
        internal IBMDSwitcher switcher { get; private set; }
        internal IBMDSwitcherMixEffectBlock mixEffect1 { get; private set; }
        internal IBMDSwitcherTransitionParameters transitionParameters { get; private set; }
        internal IBMDSwitcherInputAux inputAux { get; private set; }
        internal IBMDSwitcherKey upstreamKey1 { get; private set; }
        internal IBMDSwitcherDownstreamKey downstreamKey1 { get; private set; }
        internal IBMDSwitcherDownstreamKey downstreamKey2 { get; private set; }
        internal List<IBMDSwitcherInput> inputs { get; private set; }
        internal IBMDSwitcherMultiView multiview { get; private set; }
        // media players
        // media pool
        // flykeyer


        // monitors
        internal BMDSwitcherMonitor switcherMonitor { get; private set; }
        internal BMDSwitcherMixEffectBlockMonitor mixEffect1Monitor { get; private set; }
        internal BMDSwitcherTransitionParametersMonitor transitionMonitor { get; private set; }
        internal BMDSwitcherInputAuxMonitor inputAuxMonitor { get; private set; }
        internal BMDSwitcherUpstreamKeyMonitor upstreamKey1Monitor { get; private set; }
        internal BMDSwitcherDownstreamKeyMonitor downstreamKey1Monitor { get; private set; }
        internal BMDSwitcherDownstreamKeyMonitor downstreamKey2Monitor { get; private set; }
        internal List<BMDSwitcherInputMonitor> inputMonitors { get; private set; }
        internal BMDSwitcherMultiviewMonitor multiviewMonitor { get; private set; }


        public BMDSwitcherAPIInterface(IBMDSwitcher switcher)
        {
            if (switcher == null)
            {
                throw new ArgumentNullException("IBMDSwitcher was null");
            }
            this.switcher = switcher;
            Initialize();
        }

        private void Initialize()
        {
            RegisterCallbackMonitors();

        }

        private void RegisterCallbackMonitors()
        {
            switcherMonitor = new BMDSwitcherMonitor();
            mixEffect1Monitor = new BMDSwitcherMixEffectBlockMonitor();
            transitionMonitor = new BMDSwitcherTransitionParametersMonitor();
            inputAuxMonitor = new BMDSwitcherInputAuxMonitor();
            upstreamKey1Monitor = new BMDSwitcherUpstreamKeyMonitor();
            downstreamKey1Monitor = new BMDSwitcherDownstreamKeyMonitor();
            downstreamKey2Monitor = new BMDSwitcherDownstreamKeyMonitor();
            inputMonitors = new List<BMDSwitcherInputMonitor>();
            multiviewMonitor = new BMDSwitcherMultiviewMonitor();


        }


        public static bool TryDiscoverSwitcher(IPAddress address, out IBMDSwitcher switcher, out string message)
        {
            _BMDSwitcherConnectToFailure failure = 0;
            try
            {
                CBMDSwitcherDiscovery discovery = new CBMDSwitcherDiscovery();
                if (discovery != null)
                {
                    switcher = null;
                    message = "Failed to create switcher discovery.";
                    return false;
                }
                discovery.ConnectTo(address.ToString(), out switcher, out failure);
            }
            catch (COMException)
            {
                message = failure.ToString();
                switcher = null;
                return false;
            }
            message = "Connected";
            return true;
        }







    }
}
