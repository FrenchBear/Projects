// CharDetailViewModel
// Support for CharDetailWindow binding
//
// 2018-09-15   PV
// 2020-11-12   PV      Added Synonyms, Comments and Cross-refs.  Block/Subheaders hyperlinks.  Copy buttons.  Scrollviewer

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UniDataNS;
using DirectDrawWrite;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable enable


namespace UniSearchNS
{
    internal class CharDetailViewModel // : INotifyPropertyChanged
    {
        // Private variables
        private readonly CharDetailWindow window;
        private readonly ViewModel mainViewModel;

        // Commands public interface
        public ICommand ShowDetailCommand { get; private set; }
        public ICommand NewFilterCommand { get; private set; }
        public ICommand CopyCharCommand { get; private set; }
        public ICommand CopyAllInfoCommand { get; private set; }

        // Constructor
        public CharDetailViewModel(CharDetailWindow window, CharacterRecord cr, ViewModel mainViewModel)
        {
            SelectedChar = cr;
            this.window = window;
            this.mainViewModel = mainViewModel;

            ShowDetailCommand = new RelayCommand<int>(ShowDetailExecute);
            NewFilterCommand = new RelayCommand<string>(NewFilterExecute);
            CopyCharCommand = new RelayCommand<object>(CopyCharExecute);
            CopyAllInfoCommand = new RelayCommand<object>(CopyAllInfoExecute);
        }


        // ==============================================================================================
        // Bindable properties

        public CharacterRecord SelectedChar { get; set; } = UniData.CharacterRecords[0];    // To avoid making it nullable

        public BitmapSource? SelectedCharImage
        {
            get
            {
                if (SelectedChar == null)
                    return null;
                else
                {
                    string s = SelectedChar.Character;
                    if (s.StartsWith("U+", StringComparison.Ordinal))
                        s = "U+ " + s[2..];
                    return D2DDrawText.GetBitmapSource(s);
                }
            }
        }

        public string Title => SelectedChar == null ? "Character Detail" : SelectedChar.CodepointHex + " – Character Detail";


        public UIElement? NormalizationNFDContent => NormalizationContent(NormalizationForm.FormD);

        public UIElement? NormalizationNFKDContent => NormalizationContent(NormalizationForm.FormKD);


        private UIElement? NormalizationContent(NormalizationForm form)
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

        private UIElement GetBlockHyperlink(string? content, string commandParameter)
        {
            var h = new Hyperlink(new Run(content))
            {
                Command = NewFilterCommand,
                CommandParameter = commandParameter
            };
            return new TextBlock(h);
        }

        public UIElement BlockContent => GetBlockHyperlink(SelectedChar.Block.BlockNameAndRange, "b:\"" + SelectedChar?.Block.BlockName + "\"");

        public UIElement SubheaderContent => GetBlockHyperlink(SelectedChar.Subheader, "s:\"" + SelectedChar?.Subheader + "\"");


        public UIElement? LowercaseContent => CaseContent(true);

        public UIElement? UppercaseContent => CaseContent(false);


        private UIElement? CaseContent(bool lower)
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

        public UIElement? SynonymsContent => GetExtraInfo(SelectedChar.Synonyms);
        public UIElement? CrossRefsContent => GetExtraInfo(SelectedChar.CrossRefs);
        public UIElement? CommentsContent => GetExtraInfo(SelectedChar.Comments);

        private readonly Regex reCP = new Regex(@"\b1?[0-9A-F]{4,5}\b");

        private UIElement? GetExtraInfo(List<string>? list)
        {
            if (list == null)
                return null;

            TextBlock tb = new TextBlock();
            foreach (string altName in list)
            {
                if (tb.Inlines.Count > 0)
                    tb.Inlines.Add(new LineBreak());
                string s = (altName[0..1].ToUpper() + altName[1..]).Replace(" - ", " ");
                int sp = 0;
                for (; ; )
                {
                    var ma = reCP.Match(s, sp);
                    if (!ma.Success)
                    {
                        tb.Inlines.Add(new Run(s[sp..]));
                        break;
                    }
                    if (ma.Index > sp)
                        tb.Inlines.Add(new Run(s[sp..ma.Index]));
                    int cp = Convert.ToInt32(ma.ToString(), 16);
                    tb.Inlines.Add(ViewModel.GetCodepointHyperlink(cp, ShowDetailCommand, true));
                    sp = ma.Index + ma.Length;
                }
            }
            return tb;
        }


        // ==============================================================================================
        // Commands

        private void ShowDetailExecute(int codepoint)
        {
            CharDetailWindow.ShowDetail(codepoint, mainViewModel);
        }

        private void NewFilterExecute(string filter)
        {
            window.Close();
            mainViewModel.CharNameFilter = filter;
        }
        private void CopyCharExecute(object _)
        {
            var records = new List<CharacterRecord> { SelectedChar };
            ViewModel.DoCopyRecords("0", records);
        }
        private void CopyAllInfoExecute(object _)
        {
            var records = new List<CharacterRecord> { SelectedChar };
            ViewModel.DoCopyRecords("3", records);
        }
    }
}
