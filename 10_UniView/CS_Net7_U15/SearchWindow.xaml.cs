// SearchWindow.xaml
// Dialog to select unicode character by name
//
// 2023-08-16   PV

using System.Text;
using System.Windows;

namespace UniViewNS;

public partial class SearchWindow: Window
{
    private readonly SearchViewModel VM;

    public SearchWindow()
    {
        InitializeComponent();
        VM = new SearchViewModel();
        DataContext = VM;

        Loaded += (s, e) => FilterTextBox.Focus();
    }

    // Main function called from outside to show modal search form, and return null (nothing selected or cancelled) or
    // a string representing character(s) selected
    internal string? GetChar()
    {
        var res = ShowDialog();
        if (res == null || res.Value == false)
            return null;
        if (VM.SelectedSequence == null)
            return null;

        // Multiple selection is allowed
        var sb = new StringBuilder();
        foreach (UnicodeSequence us in MatchesListView.SelectedItems)
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
            DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Option_Click(object sender, RoutedEventArgs e) => VM.ApplyFilter();

    private void Matches_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (VM.SelectedSequence != null)        // Just in case we double-click in a part of the list that is not a sequence
            DialogResult = true;
    }
}
