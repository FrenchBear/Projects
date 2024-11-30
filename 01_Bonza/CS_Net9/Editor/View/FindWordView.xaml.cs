// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// FindWords View, Simple dialog to enter a word to search
//
// 2019-05-24   PV      First version
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13

using System.Windows;
using Bonza.Editor.ViewModel;

namespace Bonza.Editor.View;

/// <summary>
/// Interaction logic for FindWord.xaml
/// </summary>
public partial class FindWordView: Window
{
    public FindWordView(FindWordViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;

        Loaded += (sender, args) => SearchTextBox.Focus();
    }
}
