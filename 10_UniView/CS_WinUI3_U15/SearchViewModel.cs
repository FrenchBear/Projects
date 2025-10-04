// SearchViewModel.cs
// ViewModel of SearchWindow, binding and filtering
//
// 2023-08-16   PV      First version for v2.0

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UniDataNS;
using static UniView_WinUI3.CurrentSearchOptions;

namespace UniView_WinUI3;

class SearchViewModel: INotifyPropertyChanged
{
    //private readonly SearchWindow W;

    // Avoid event loops on ceckboxes groups
    public bool IgnoreClick;

    public static bool WindowLoaded = false;

    // INotifyPropertyChanged -------------------------
    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // -------------------------

    public SearchViewModel()
    {
        WindowLoaded = false;

        // Add codepoints
        foreach (var cr in UniData.CharacterRecords.Values)
        {
            SequenceType sequenceType = SequenceType.None;
            SequenceSubtype sequenceSubtype = SequenceSubtype.None;

            if (cr.Block.Level3Name == "Scripts")
            {
                sequenceType = SequenceType.CodepointScript;
                if ((cr.Block.Level1Name == "Latin" || cr.Block.Level1Name == "Greek") && cr.Block.BlockName != "Old Hungarian" && !cr.Name.StartsWith("COPTIC "))
                    sequenceSubtype = SequenceSubtype.CodepointLatinAndGreek;
            }
            else if (cr.Block.Level3Name == "Symbols and Punctuation")
            {
                sequenceType = SequenceType.CodepointSymbol;
                if (cr.Block.Level2Name == "Emoji & Pictographs")
                    sequenceSubtype = SequenceSubtype.CodepointEmoji;
            }
            else
                Debugger.Break();

            _Matches.Add(new UnicodeSequence(cr.Name, new int[] { cr.Codepoint }, sequenceType, sequenceSubtype));
        }

        // Add Emoji and ZWJ sequences
        foreach (var es in EmojiAndZWJSequences.EmojiSequenceRecords)
            _Matches.Add(es);

        ApplyFilter();
    }

    public void InitializeBindings()
    {
        NotifyPropertyChanged(nameof(OutputName));
        NotifyPropertyChanged(nameof(OutputCharacters));
        NotifyPropertyChanged(nameof(OutputCodepoints));

        NotifyPropertyChanged(nameof(SymbolNone));
        NotifyPropertyChanged(nameof(SymbolEmoji));
        NotifyPropertyChanged(nameof(SymbolAll));

        NotifyPropertyChanged(nameof(ScriptNone));
        NotifyPropertyChanged(nameof(ScriptLatin));
        NotifyPropertyChanged(nameof(ScriptAll));

        WindowLoaded = true;
    }

    public void ApplyFilter()
    {
        var fpb = new FilterPredicateBuilder(_Filter, false, false, SymbolFilter, ScriptFilter, SequenceFilter, ZWJFilter);
        MatchesFilteredList.Clear();
        foreach (UnicodeSequence item in _Matches.Where(cr => fpb.GetFilter(cr)))
            MatchesFilteredList.Add(item);
        NotifyPropertyChanged(nameof(MatchesFilteredList));
    }

    // Bindings -------------------------

    string _Filter = string.Empty;
    public string Filter
    {
        get => _Filter;
        set
        {
            _Filter = value;
            NotifyPropertyChanged(nameof(Filter));
            ApplyFilter();
        }
    }

    public SymbolFilter SymbolFilter
        => SymbolNone ?? false ? SymbolFilter.None :
           SymbolEmoji ?? false ? SymbolFilter.Emoji :
           SymbolAll ?? false ? SymbolFilter.All :
           throw new InvalidEnumArgumentException("Unknown SymbolFilter");

    public ScriptFilter ScriptFilter
        => ScriptNone ?? false ? ScriptFilter.None :
           ScriptLatin ?? false ? ScriptFilter.Latin :
           ScriptAll ?? false ? ScriptFilter.All :
           throw new InvalidEnumArgumentException("Unknown ScriptFilter");

    public EmojiSequenceFilter SequenceFilter
        => new(EmojiSequenceKeycaps, EmojiSequenceFlags, EmojiSequenceModifiers);

    public ZWJSequenceFilter ZWJFilter
        => new(ZWJSequenceFamily, ZWJSequenceRole, ZWJSequenceGendered, ZWJSequenceHair, ZWJSequenceOther);

    private readonly ObservableCollection<UnicodeSequence> _Matches = new();

    public ObservableCollection<UnicodeSequence> MatchesFilteredList { get; set; } = new ObservableCollection<UnicodeSequence>();

    public UnicodeSequence? SelectedSequence { get; set; }

    // Source Emoji sequences -------------------------

    public bool? EmojiSequenceAll
    {
        get => emojiSequenceAll;
        set
        {
            if (IgnoreClick)
                return;

            IgnoreClick = true;
            bool v = value == true;
            emojiSequenceAll = v;
            foreach (string index in new string[] { "Keycaps", "Flags", "Modifiers" })
                if (EmojiSequenceDictionary[index] != v)
                {
                    EmojiSequenceDictionary[index] = v;
                    NotifyPropertyChanged("EmojiSequence" + index);
                }

            NotifyPropertyChanged(nameof(EmojiSequenceAll));
            IgnoreClick = false;
            ApplyFilter();
        }
    }

    public bool EmojiSequenceKeycaps
    {
        get => EmojiSequenceDictionary["Keycaps"];
        set => UpdateEmojiSequenceCheckBox("Keycaps", value);
    }

    public bool EmojiSequenceFlags
    {
        get => EmojiSequenceDictionary["Flags"];
        set => UpdateEmojiSequenceCheckBox("Flags", value);
    }

    public bool EmojiSequenceModifiers
    {
        get => EmojiSequenceDictionary["Modifiers"];
        set => UpdateEmojiSequenceCheckBox("Modifiers", value);
    }

    private void UpdateEmojiSequenceCheckBox(string index, bool value)
    {
        if (!WindowLoaded)
            return;
        if (IgnoreClick)
            return;

        IgnoreClick = true;
        EmojiSequenceDictionary[index] = value;
        if (EmojiSequenceDictionary.Values.All(b => b))
            emojiSequenceAll = true;
        else if (EmojiSequenceDictionary.Values.All(b => !b))
            emojiSequenceAll = false;
        else
            emojiSequenceAll = null;
        NotifyPropertyChanged(nameof(EmojiSequenceAll));
        IgnoreClick = false;
        ApplyFilter();
    }

    // Source ZWJ sequences -------------------------

    public bool? ZWJSequenceAll
    {
        get => zwjSequenceAll;
        set
        {
            if (IgnoreClick)
                return;

            IgnoreClick = true;
            bool v = value == true;
            zwjSequenceAll = v;
            foreach (string index in new string[] { "Family", "Role", "Gendered", "Hair", "Other" })
                if (ZWJSequenceDictionary[index] != v)
                {
                    ZWJSequenceDictionary[index] = v;
                    NotifyPropertyChanged("ZWJSequence" + index);
                }

            NotifyPropertyChanged(nameof(ZWJSequenceAll));
            IgnoreClick = false;
            ApplyFilter();
        }
    }

    public bool ZWJSequenceFamily
    {
        get => ZWJSequenceDictionary["Family"];
        set => UpdateZWJSequenceCheckBox("Family", value);
    }

    public bool ZWJSequenceRole
    {
        get => ZWJSequenceDictionary["Role"];
        set => UpdateZWJSequenceCheckBox("Role", value);
    }

    public bool ZWJSequenceGendered
    {
        get => ZWJSequenceDictionary["Gendered"];
        set => UpdateZWJSequenceCheckBox("Gendered", value);
    }

    public bool ZWJSequenceHair
    {
        get => ZWJSequenceDictionary["Hair"];
        set => UpdateZWJSequenceCheckBox("Hair", value);
    }

    public bool ZWJSequenceOther
    {
        get => ZWJSequenceDictionary["Other"];
        set => UpdateZWJSequenceCheckBox("Other", value);
    }

    private void UpdateZWJSequenceCheckBox(string index, bool value)
    {
        if (!WindowLoaded)
            return;
        if (IgnoreClick)
            return;

        IgnoreClick = true;
        ZWJSequenceDictionary[index] = value;
        if (ZWJSequenceDictionary.Values.All(b => b))
            zwjSequenceAll = true;
        else if (ZWJSequenceDictionary.Values.All(b => !b))
            zwjSequenceAll = false;
        else
            zwjSequenceAll = null;
        NotifyPropertyChanged(nameof(ZWJSequenceAll));
        IgnoreClick = false;
        ApplyFilter();
    }

    // Radiobuttons -------------------

    protected void SetOptionProperty(ref bool? field, bool? newValue, [CallerMemberName] string propertyName = "?")
    {
        if (!WindowLoaded)
            return;
        if (field != newValue)
        {
            field = newValue;
            NotifyPropertyChanged(propertyName);
            ApplyFilter();
        }
    }

    // No need to call ApplyFilter for OutputProperty
    protected void SetOutputProperty(ref bool? field, bool? newValue, [CallerMemberName] string propertyName = "?")
    {
        if (!WindowLoaded)
            return;
        if (field != newValue)
        {
            field = newValue;
            NotifyPropertyChanged(propertyName);
        }
    }

    // Options
    public bool? OutputName { get => outputName; set => SetOutputProperty(ref outputName, value); }

    public bool? OutputCharacters { get => outputCharacters; set => SetOutputProperty(ref outputCharacters, value); }

    public bool? OutputCodepoints { get => outputCodepoints; set => SetOutputProperty(ref outputCodepoints, value); }

    // Source Symbols
    public bool? SymbolNone { get => symbolNone; set => SetOptionProperty(ref symbolNone, value); }

    public bool? SymbolEmoji { get => symbolEmoji; set => SetOptionProperty(ref symbolEmoji, value); }

    public bool? SymbolAll { get => symbolAll; set => SetOptionProperty(ref symbolAll, value); }

    // Source Scripts
    public bool? ScriptNone { get => scriptNone; set => SetOptionProperty(ref scriptNone, value); }

    public bool? ScriptLatin { get => scriptLatin; set => SetOptionProperty(ref scriptLatin, value); }

    public bool? ScriptAll { get => scriptAll; set => SetOptionProperty(ref scriptAll, value); }
}
