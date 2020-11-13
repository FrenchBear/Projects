﻿// CharDetailDialog
// Show more information on a character after a click on hyperlink
//
// In UWP version, can't show more than one ContentDialog on screen: drill-down explore
// is handled by recycling current window and implement a back navigation mechanism
//
// 2018-09-18   PV
// 2020-11-11   PV      1.3 Hyperlinks to block and subheader
// 2020-11-11   PV      nullable enable


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniDataNS;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable


namespace UniSearchUWPNS
{
    public sealed partial class CharDetailDialog : ContentDialog
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

        // Static function for easy opening
        internal async static Task ShowDetail(int codepoint, ViewModel mainViewModel)
        {
            var w = new CharDetailDialog(codepoint, mainViewModel);
            await w.ShowAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Back();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var records = new List<CharacterRecord> { ViewModel.SelectedChar };
            UniSearchUWPNS.ViewModel.DoCopyRecords("3", records);
        }
    }
}
