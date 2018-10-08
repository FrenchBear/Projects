﻿// CharDetailViewModel
// Support for CharDetailWindow binding
//
// 2018-09-15   PV


using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UniDataNS;
using DirectDrawWrite;
using System.Text;
using System.Windows.Controls;
using System.Globalization;


namespace UniSearchNS
{
    internal class CharDetailViewModel // : INotifyPropertyChanged
    {
        /*
        // INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        */

        // Commands public interface
        public ICommand ShowDetailCommand { get; private set; }


        // Constructor
        public CharDetailViewModel(CharacterRecord cr)
        {
            SelectedChar = cr;
            ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
        }


        // ==============================================================================================
        // Bindable properties

        public CharacterRecord SelectedChar { get; set; }

        public BitmapSource SelectedCharImage
        {
            get
            {
                if (SelectedChar == null)
                    return null;
                else
                {
                    string s = SelectedChar.Character;
                    if (s.StartsWith("U+", StringComparison.Ordinal))
                        s = "U+ " + s.Substring(2);
                    return D2DDrawText.GetBitmapSource(s);
                }
            }
        }

        public string Title => SelectedChar == null ? "Character Detail" : SelectedChar.CodepointHex + " – Character Detail";


        public Object NormalizationNFDContent => NormalizationContent(NormalizationForm.FormD);

        public Object NormalizationNFKDContent => NormalizationContent(NormalizationForm.FormKD);


        private UIElement NormalizationContent(NormalizationForm form)
        {
            if (!SelectedChar.IsPrintable) return null;

            string s = SelectedChar.Character;
            string sn = s.Normalize(form);
            if (s == sn) return null;
            if (form == NormalizationForm.FormKD && sn == s.Normalize(NormalizationForm.FormD))
                return new TextBlock { Text = "Same as NFD" };

            StackPanel sp = new StackPanel();
            foreach (var cr in sn.EnumCharacterRecords())
                sp.Children.Add(ViewModel.GetStrContent(cr.Codepoint, ShowDetailCommand));
            return sp;
        }



        public Object LowercaseContent => CaseContent(true);

        public Object UppercaseContent => CaseContent(false);


        private UIElement CaseContent(bool lower)
        {
            if (!SelectedChar.IsPrintable) return null;

            string s = SelectedChar.Character;
            string sc = lower ? s.ToLowerInvariant() : s.ToUpperInvariant();
            if (s == sc) return null;

            StackPanel sp = new StackPanel();
            foreach (var cr in sc.EnumCharacterRecords())
                sp.Children.Add(ViewModel.GetStrContent(cr.Codepoint, ShowDetailCommand));
            return sp;
        }


        // ==============================================================================================
        // Commands

        private void ShowDetailExecute(int codepoint)
        {
            CharDetailWindow.ShowDetail(codepoint);
        }


    }
}