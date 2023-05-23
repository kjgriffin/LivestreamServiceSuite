using System.Collections.Generic;
using System.Windows.Controls;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for DictionaryViewer.xaml
    /// </summary>
    public partial class DictionaryViewer : UserControl
    {
        public DictionaryViewer()
        {
            InitializeComponent();
        }

        public void SetBackingModel(Dictionary<string, string> model, string keyDesc, string valueDesc)
        {
            itemRvCtrl.Items.Clear();
            foreach (var kvp in model)
            {
                KeyValuePairViewer ctrl = new KeyValuePairViewer();

                ctrl.KeyDescription = keyDesc;
                ctrl.ValueDescription = valueDesc;
                ctrl.Key = kvp.Key;
                ctrl.Value = kvp.Value;
                itemRvCtrl.Items.Add(ctrl);
            }
        }

        public Dictionary<string, string> MarshalBackingModel()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var ctrl in itemRvCtrl.Items)
            {
                var kvp = ctrl as KeyValuePairViewer;
                if (kvp != null)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

    }
}
