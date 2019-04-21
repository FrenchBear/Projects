using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SolWPF
{
    public class GameSerialViewModel: INotifyPropertyChanged
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
    }
}
