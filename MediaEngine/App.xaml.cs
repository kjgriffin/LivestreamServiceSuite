using MediaEngine.WPFPlayout.Test;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MediaEngine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStartup(object sender, StartupEventArgs e)
        {
            // load ffmpeg libraries...
            //Unosquare.FFME.Library.FFmpegDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"lib\ffmpeg");
            //Unosquare.FFME.Library.LoadFFmpeg();

            // start mainwindow
            TestHostWindow window = new TestHostWindow();
            window.Show();
        }

    }
}
