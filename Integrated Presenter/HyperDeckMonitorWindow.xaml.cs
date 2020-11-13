using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Integrated_Presenter
{

    public delegate void TextCommandEvent(object sender, string cmd);

    /// <summary>
    /// Interaction logic for HyperDeckMonitorWindow.xaml
    /// </summary>
    public partial class HyperDeckMonitorWindow : Window
    {

        public event TextCommandEvent OnTextCommand; 

        public bool IsClosed { get; set; } = false;
        public HyperDeckMonitorWindow()
        {
            InitializeComponent();
        }

        public void AddMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                tb.Text = tb.Text + msg;
            });
        }

        private void OnClosed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.EndsWith('\r'))
            {
                // send command
                string cmd = tbCmd.Text;
                OnTextCommand?.Invoke(this, cmd);
                tbCmd.Text = "";
            }
        }
    }
}
