using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CCUI_UI
{

    internal delegate void RunPresetEvent(string pName);
    internal delegate void RemovePresetEvent(string pName);
    internal delegate void PresetSelected(string pName);

    /// <summary>
    /// Interaction logic for PresetControl.xaml
    /// </summary>
    public partial class PresetControl : UserControl
    {

        internal event RunPresetEvent OnRunPreset;
        internal event RemovePresetEvent OnRemovePreset;
        internal event PresetSelected OnPresetSelected;

        public string PresetName { get; private set; }

        public PresetControl(string pname)
        {
            InitializeComponent();
            tbName.Text = pname;
            PresetName = pname;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            OnRunPreset?.Invoke(PresetName);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            OnRemovePreset?.Invoke(PresetName);
        }

        private void _PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnPresetSelected?.Invoke(PresetName);
        }

    }
}
