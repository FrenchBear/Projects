using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlotterWPFTest;

public abstract class BaseViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PlotChart();
            return true;
        }
        return false;
    }

    public abstract void PlotChart();
}
