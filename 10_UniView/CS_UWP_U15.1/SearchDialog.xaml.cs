// SearchWindow.xaml
// Dialog to select unicode character by name
//
// 2023-08-16   PV

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static UniViewNS.CurrentSearchOptions;

#nullable enable
//#pragma warning disable IDE0051 // Remove unused private members

namespace UniViewNS;

public sealed partial class SearchDialog: ContentDialog, IDisposable
{
    private readonly SearchViewModel VM;

    public SearchDialog()
    {
        InitializeComponent();
        VM = new SearchViewModel();
        DataContext = VM;

        Loaded += (s, e) => { 
            FilterTextBox.Focus(FocusState.Programmatic);
            VM.InitializeBindings();
        };
    }

    public void Dispose() { }

    // Main function called from outside to show modal search form, and return null (nothing selected or cancelled) or
    // a string representing character(s) selected
    internal string? GetChar()
    {
        // Multiple selection is allowed
        var sb = new StringBuilder();
        foreach (UnicodeSequence us in MatchesListView.SelectedItems.Cast<UnicodeSequence>())
        {
            if (VM.OutputName ?? false)
                sb.Append("{" + us.Name + "}");
            else if (VM.OutputCharacters ?? false)
                sb.Append(us.SequenceAsString);
            else
                foreach (int cp in us.Sequence)
                    sb.Append($"U+{cp:X4}");
        }

        return sb.ToString();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // No commanding here (didn't want to add RelayCommand) so Ok may be rejected if nothing is selected
        if (VM.SelectedSequence != null)
            Hide();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Hide();

    private void MatchesListView_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (VM.SelectedSequence != null)        // Just in case we double-click in a part of the list that is not a sequence
            Hide();
    }
}
