using IntegratedPresenter.Main;

using SharedPresentationAPI.Presentation;

using System.Threading.Tasks;

namespace Integrated_Presenter.Automation
{
    internal interface ISlideScriptActionAutomater : IActionAutomater
    {
        Task ExecuteMainActions(ISlide s);
        Task ExecuteSetupActions(ISlide s);
    }
}
