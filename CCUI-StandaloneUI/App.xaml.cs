using CCUI_UI;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CCUI_StandaloneUI
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
            Start(e.Args.Contains("debug-fake"));
        }

        public void Start(bool fake = false)
        {
            monitor = new CCPUPresetMonitor(false, fake);
        }

    }
}
