using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Integrated_Presenter.ViewModels
{
    internal class UIValue<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        T _value = default(T);
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
