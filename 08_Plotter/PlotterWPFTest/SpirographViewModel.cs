// Spirograph ViewModel
// Parameters of Spirograph chart bound to SpirographUserControl
//
//2021-12-17    PV

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlotterWPFTest;

internal class SpirographViewModel: INotifyPropertyChanged
{
    private readonly SpirographUserControl View;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public SpirographViewModel(SpirographUserControl view)
        => View = view;

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            View.PlotChart();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Fixed wheel teeth count
    /// </summary>
    public int Z1
    {
        get => _Z1;
        set => SetProperty(ref _Z1, value);
    }
    private int _Z1 = 60;

    /// <summary>
    /// Mobile wheel teeth count
    /// </summary>
    public int Z2
    {
        get => _Z2;
        set => SetProperty(ref _Z2, value);
    }
    private int _Z2 = 45;

    /// <summary>
    /// Position of pen on mobile wheel radius, 0=center, 1=edge
    /// </summary>
    public double K2 { get => _K2; set => SetProperty(ref _K2, value); }
    private double _K2 = 0.8;

    public int PenWidth { get => _PenWidth; set => SetProperty(ref _PenWidth, value); }
    private int _PenWidth = 1;

    public List<string> PenColors { get => _PenColors; set => SetProperty(ref _PenColors, value); }
    internal List<string> _PenColors = new();

    public int PenColorIndex { get => _PenColorIndex; set => SetProperty(ref _PenColorIndex, value); }
    private int _PenColorIndex;

}
