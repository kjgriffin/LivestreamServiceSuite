using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace UIControls
{
    /// <summary>
    /// Interaction logic for KeyValuePairViewer.xaml
    /// </summary>
    public partial class KeyValuePairViewer : UserControl, INotifyPropertyChanged
    {
        private string m_KeyDescription;
        private string m_ValueDescription;
        private string m_Key;
        private string m_Value;

        public string KeyDescription
        {
            get => m_KeyDescription;
            set
            {
                m_KeyDescription = value;
                OnPropertyChanged();
            }
        }
        public string ValueDescription
        {
            get => m_ValueDescription;
            set
            {
                m_ValueDescription = value;
                OnPropertyChanged();
            }
        }
        public string Key
        {
            get => m_Key;
            set
            {
                m_Key = value;
                OnPropertyChanged();
            }
        }
        public string Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                OnPropertyChanged();
            }
        }

        public KeyValuePairViewer()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
