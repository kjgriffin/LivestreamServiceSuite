using ATEMSharedState.SwitcherState;

using System.Collections.Generic;
using System.ComponentModel;

namespace IntegratedPresenter.ViewModels
{
    public class SwitcherBusViewModel
    {
        public SwitcherBusButton[] Buttons { get; set; }

        public SwitcherBusViewModel(int numbuttons, List<(int num, string name)> buttons)
        {
            Buttons = new SwitcherBusButton[8];
            for (int i = 0; i < numbuttons; i++)
            {
                Buttons[i] = new SwitcherBusButton();
                Buttons[i].Lit = false;
                Buttons[i].Number = buttons[i].num;
                Buttons[i].Name = buttons[i].name;
            }
        }

        public void OnSwitcherStateChanged(BMDSwitcherState state)
        {
            foreach (var btn in Buttons)
            {
                btn.Lit = false;
                if (state.PresetID == btn.Number)
                {
                    btn.Lit = true;
                }
            }
        }
    }

    public class SwitcherBusButton : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Number { get; set; }
        private bool lit;
        public bool Lit
        {
            get => lit;
            set
            {
                lit = value;
                OnPropertyChanged("Lit");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
