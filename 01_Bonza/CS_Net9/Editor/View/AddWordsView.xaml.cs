﻿// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// AddWords View, Simple dialog to enter words to add to current layout
//
// 2017-08-05   PV      First version
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13

using Bonza.Editor.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace Bonza.Editor.View;

/// <summary>
/// Interaction logic for AddWordsView.xaml
/// </summary>
public partial class AddWordsView: Window
{
    internal List<string> wordsList;

    internal AddWordsView(Model.EditorModel model)
    {
        InitializeComponent();

        DataContext = new AddWordsViewModel(this, model);

        Loaded += (sender, args) => InputTextBlock.Focus();
    }
}
