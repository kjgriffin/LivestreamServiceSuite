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
using System.Windows.Shapes;

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
