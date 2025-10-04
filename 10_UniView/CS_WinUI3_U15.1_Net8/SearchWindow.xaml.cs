// SearchWindow code behind
// UniView WinUI3 version
//
// 2023-08-27   PV

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Text;

namespace UniView_WinUI3;

public sealed partial class SearchWindow: ContentDialog
{
    private readonly SearchViewModel VM;

    public SearchWindow()
    {
        InitializeComponent();

        VM = new SearchViewModel();
        MainGrid.DataContext = VM;

        MainGrid.Loaded += (s, e) =>
        {
            FilterTextBox.Focus(FocusState.Programmatic);
            VM.InitializeBindings();
        };
    }

    // Crash app
    //private void MatchesListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    //{
    //    if (VM.SelectedSequence != null)        // Just in case we double-click in a part of the list that is not a sequence
    //        Hide();
    //}

    public string? GetChar()
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
}
