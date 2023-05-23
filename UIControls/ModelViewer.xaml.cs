using System.Collections.Generic;
using System.Windows;

namespace UIControls
{
    /// <summary>
    /// Interaction logic for ModelViewer.xaml
    /// </summary>
    public partial class ModelViewer : Window
    {
        public ModelViewer(Dictionary<string, string> model, string keydesc, string valuedesc, string title)
        {
            InitializeComponent();
            this.Title = title;
            dvModel.SetBackingModel(model, keydesc, valuedesc);
        }

        public Dictionary<string, string> MarshalData()
        {
            return dvModel.MarshalBackingModel();
        }


    }
}
