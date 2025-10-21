// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// FindWords View, Simple dialog to enter a word to search
//
// 2019-05-24   PV  First version

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bonza.Editor.ViewModel;

namespace Bonza.Editor.View
{
    /// <summary>
    /// Interaction logic for FindWord.xaml
    /// </summary>
    public partial class FindWordView : Window
    {
        public FindWordView(FindWordViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            Loaded += (sender, args) => { SearchTextBox.Focus(); };
        }
    }
}
