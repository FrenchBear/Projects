// BonzaEditor - WPF Tool to prepare Bonza-style puzzles
// AddWords View, Simple dialog to enter words to add to current layout
//
// 2017-08-05   PV  First version


using Bonza.Editor.ViewModel;
using System.Windows;

namespace Bonza.Editor.View
{
    /// <summary>
    /// Interaction logic for AddWordsView.xaml
    /// </summary>
    public partial class AddWordsView : Window
    {
        private readonly AddWordsViewModel viewModel;

        public AddWordsView()
        {
            InitializeComponent();

            viewModel = new AddWordsViewModel(this);
            DataContext = viewModel;

            Loaded += (sender, args) => { InputTextBlock.Focus(); };
        }

        public void Test()
        {
            viewModel.InputText = viewModel.InputText + "Hello\r\nKitty";
        }
    }
}
