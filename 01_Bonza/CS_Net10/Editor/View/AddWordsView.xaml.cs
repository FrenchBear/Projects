// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// AddWords View, Simple dialog to enter words to add to current layout
//
// 2017-08-05   PV      First version
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13
// 2026-01-20	PV		Net10 C#14

namespace Bonza.Editor.View;

public partial class AddWordsView: Window
{
    internal List<string> wordsList;

    internal AddWordsView(EditorModel model)
    {
        InitializeComponent();

        DataContext = new AddWordsViewModel(this, model);

        Loaded += (sender, args) => InputTextBlock.Focus();
    }
}
