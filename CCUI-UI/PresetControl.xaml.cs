using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CCUI_UI
{

    internal delegate void RunPresetEvent(string pName);
    internal delegate void RemovePresetEvent(string pName);

    /// <summary>
    /// Interaction logic for PresetControl.xaml
    /// </summary>
    public partial class PresetControl : UserControl
    {

        internal event RunPresetEvent OnRunPreset;
        internal event RemovePresetEvent OnRemovePreset;

        public string PresetName { get; private set; }

        public PresetControl(string pname)
        {
            InitializeComponent();
            tbName.Text = pname;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            OnRunPreset?.Invoke(PresetName);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            OnRemovePreset?.Invoke(PresetName);
        }
    }
}
