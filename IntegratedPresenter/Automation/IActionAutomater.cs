using IntegratedPresenterAPIInterop;

using System.Threading.Tasks;

namespace Integrated_Presenter.Automation
{
    internal interface IActionAutomater
    {
        Task<ActionResult> PerformAutomationAction(AutomationAction task, string requesterID);
        void ProvideWatchInfo(ConditionWatchProvider watches);
        void UpdateDriver(IDeviceDriver driver);
    }
}
