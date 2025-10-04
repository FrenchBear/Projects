// CharDetailDialog
// Show more information on a character after a click on hyperlink
//
// In UWP version, can't show more than one ContentDialog on screen: drill-down explore
// is handled by recycling current window and implement a back navigation mechanism
//
// 2018-09-18   PV
// 2020-11-11   PV      1.3 Hyperlinks to block and subheader
// 2020-11-11   PV      nullable enable
// 2024-09-27   PV      WinUI3 version

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniDataWinUI3;

namespace UniSearchWinUI3;

public sealed partial class CharDetailDialog: ContentDialog, IDisposable
{
    readonly CharDetailViewModel ViewModel;

    internal CharDetailDialog(int codepoint, ViewModel mainViewModel)
    {
        InitializeComponent();

        ViewModel = new CharDetailViewModel(this, UniData.CharacterRecords[codepoint], mainViewModel);
        DataContext = ViewModel;

        Loaded += (s, e) =>
        {
            DefaultButton = ContentDialogButton.None;
            CloseButton.Focus(FocusState.Programmatic);
        };

        // Esc closes the window automatically, nothing to do
    }

    public void Dispose() => ViewModel.Dispose();

    // Static function for easy opening
    internal static async Task ShowDetail(int codepoint, ViewModel mainViewModel, XamlRoot xamlRoot)
    {
        var w = new CharDetailDialog(codepoint, mainViewModel)
        {
            XamlRoot = xamlRoot
        };
        await w.ShowAsync();
        w.Dispose();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) => ViewModel.Back();

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var records = new List<CharacterRecord> { ViewModel.SelectedChar };
        UniSearchWinUI3.ViewModel.DoCopyRecords("3", records);
    }
}
