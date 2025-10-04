// UniView CS UWP
// Simple prototype of an app showing Unicode text details
// ViewModel: Bindings of MainWindow
//
// 2020-12-18   PV      Moved out MainWindow
// 2023-01-23   PV      C#11

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UniView_WinUI3;

// No need to support INotifyPropertyChanged since we won't update SourceText from code except at the very beginning
// so we call Transform manually in this case
internal class ViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ObservableCollection<CodepointDetail> CodepointsCollection { get; set; } = new ObservableCollection<CodepointDetail>();

    public List<TextSample> SamplesCollection { get; } = new List<TextSample>();

    private int _CharactersCount;

    public int CharactersCount
    {
        get => _CharactersCount;
        set
        {
            if (_CharactersCount != value)
            {
                _CharactersCount = value;
                NotifyPropertyChanged(nameof(CharactersCount));
            }
        }
    }

    private int _CodepointsCount;

    public int CodepointsCount
    {
        get => _CodepointsCount;
        set
        {
            if (_CodepointsCount != value)
            {
                _CodepointsCount = value;
                NotifyPropertyChanged(nameof(CodepointsCount));
            }
        }
    }

    private int _GlyphsCount;

    public int GlyphsCount
    {
        get => _GlyphsCount;
        set
        {
            if (_GlyphsCount != value)
            {
                _GlyphsCount = value;
                NotifyPropertyChanged(nameof(GlyphsCount));
            }
        }
    }
}