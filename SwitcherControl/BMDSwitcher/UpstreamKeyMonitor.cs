using BMDSwitcherAPI;

namespace SwitcherControl.BMDSwitcher
{
    class UpstreamKeyMonitor : IBMDSwitcherKeyCallback
    {
        public event SwitcherEventHandler UpstreamKeyOnAirChanged;
        public event SwitcherEventHandler UpstreamKeyFillChanged;
        public event SwitcherEventHandler UpstreamKeyTypeChanged;

        public event SwitcherEventHandler UpstreamKeyCutChanged;

        public event SwitcherEventHandler UpstreamKeyMaskChanged;


        void IBMDSwitcherKeyCallback.Notify(_BMDSwitcherKeyEventType eventType)
        {
            switch (eventType)
            {
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputFillChanged:
                    UpstreamKeyFillChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeInputCutChanged:
                    UpstreamKeyCutChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeOnAirChanged:
                    UpstreamKeyOnAirChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeTypeChanged:
                    UpstreamKeyTypeChanged?.Invoke(this, null);
                    break;
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskTopChanged:
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskBottomChanged:
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskLeftChanged:
                case _BMDSwitcherKeyEventType.bmdSwitcherKeyEventTypeMaskRightChanged:
                    UpstreamKeyMaskChanged?.Invoke(this, null);
                    break;

            }
        }
    }
}
