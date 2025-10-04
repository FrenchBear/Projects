// UniView CS WPF
// Simple prototype of an app showing Unicode text details
// MainViewModel: Bindings of MainWindow
//
// 2020-12-18   PV      Moved out MainWindow
// 2023-11-20   PV      Net8 C#12
// 2024-09-15   PV      Unicode 16; Code cleanup

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UniView_CS_Net8;

// No need to support INotifyPropertyChanged since we won't update SourceText from code except at the very beginning
// so we call Transform manually in this case
internal sealed class MainViewModel: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // This property is only used to support delayed binding from MainWindow.xaml, and raise exent SourceUpdated after delay expired after InputText modified
    public string SourceText { get; set; } = string.Empty;

    public ObservableCollection<CodepointDetail> CodepointsCollection { get; set; } = [];

    public List<TextSample> SamplesCollection { get; } = [];

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
