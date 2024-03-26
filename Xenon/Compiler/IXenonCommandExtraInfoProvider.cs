using IntegratedPresenter.BMDSwitcher.Config;

namespace Xenon.Compiler
{
    public interface IXenonCommandExtraInfoProvider
    {
        public BMDSwitcherConfigSettings ProjectConfigState { get; }
    }
}
