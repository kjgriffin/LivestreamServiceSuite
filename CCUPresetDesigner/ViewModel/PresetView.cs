using CCUPresetDesigner.DataModel;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCUPresetDesigner.ViewModel
{
    public class PresetView : INotifyPropertyChanged
    {
        internal CamPresetMk2 OriginalData { get; set; }

        private bool isSelected;
        internal bool IsSelected { get => isSelected; set { isSelected = value; NotifyPropertyChanged(); } }

        internal string Camera => OriginalData?.Camera ?? "";


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        #endregion
    }
}
