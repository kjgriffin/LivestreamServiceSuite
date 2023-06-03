using IntegratedPresenterAPIInterop;

using System.Threading.Tasks;

namespace Integrated_Presenter.Automation
{
    internal interface IActionAutomater
    {
        Task<ActionResult> PerformAutomationAction(AutomationAction task);
        void ProvideWatchInfo(ConditionWatchProvider watches);
        void UpdateDriver(IDeviceDriver driver);
    }
}
