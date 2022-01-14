namespace SwitcherControl.Mk2
{
    internal delegate void SwitcherAPICommand(BMDSwitcherAPIInterface api);
    class QueuedSwitcherWorkItem
    {
        public SwitcherAPICommand APIInvokeAction;
    }
}
