// Solitaire WPF
// class NewGameOptionsViewModel
// Simple VM for NewGameOptionsWindow binding
//
// 2019-04-18   PV
// 2020-12-19   PV      Net5 C#9, nullable enable
// 2021-11-13   PV      Net6 C#10
// 2025-03-16   PV      Net9 C#13

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Solitaire;

public class NewGameOptionsViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ICommand OKCommand { get; private set; }

    private NewGameOptionsWindow? dlg;

    public NewGameOptionsViewModel()
    {
        OKCommand = new RelayCommand<object>(OKExecute, CanOK);
        GameSerial = 0;
        IsWithAAndK = false;
    }

    internal void SetWindow(NewGameOptionsWindow dlg)
        => this.dlg = dlg;

    private int _GameSerial;

    public int GameSerial
    {
        get => _GameSerial;
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
        get => _IsWithAAndK;
        set
        {
            if (_IsWithAAndK != value)
            {
                _IsWithAAndK = value;
                NotifyPropertyChanged(nameof(IsWithAAndK));
            }
        }
    }

    private bool CanOK(object obj) => !Validation.GetHasError(dlg?.GameSerialTextBox);

    private void OKExecute(object obj)
    {
        if (dlg is not null)
            dlg.DialogResult = true;
    }

    // =====================================================================================================
    // IDataErrorInfo

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static

    public string? Error => null;
#pragma warning restore IDE0079 // Remove unnecessary suppression

    public string? this[string name]
    {
        get
        {
            string? result = null;

            if (name == "GameSerialTextBox")
            {
                if (GameSerial < 0)
                    result = "Minimum value is 0";
                else if (GameSerial > 999_999)
                    result = "Maximum value is 999999";
            }
            return result;
        }
    }
}
