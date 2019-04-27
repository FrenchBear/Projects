// Solitaire WPF
// class NewGameOptionsViewModel
// Simple VM for NewGameOptionsWindow binding
// 2019-04-18   PV

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SolWPF
{
    public class NewGameOptionsViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private int _GameSerial;
        public int GameSerial
        {
            get { return _GameSerial; }
            set
            {
                if (_GameSerial != value)
                {
                    _GameSerial = value;
                    NotifyPropertyChanged(nameof(GameSerial));
                }
            }
        }

        private bool? _IsWithAAndK;
        public bool? IsWithAAndK
        {
            get { return _IsWithAAndK; }
            set
            {
                if (_IsWithAAndK != value)
                {
                    _IsWithAAndK = value;
                    NotifyPropertyChanged(nameof(IsWithAAndK));
                }
            }
        }
    }
}
