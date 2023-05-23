using System.Windows;

namespace CCUI_UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        CCPUPresetMonitor monitor;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Start();
        }

        public void Start()
        {
            monitor = new CCPUPresetMonitor(false);
        }

    }
}
