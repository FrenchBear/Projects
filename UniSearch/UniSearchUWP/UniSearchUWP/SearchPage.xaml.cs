// UniSearchUWP
// Unicode Character Search Tool, UWP version
// Learning app for UWP model
//
// 2018-09-18   PV


using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace UniSearchUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        ViewModel vm;

        public SearchPage()
        {
            this.InitializeComponent();

            vm = new ViewModel(this);
            DataContext = vm;

            CharacterFilterTextBox.Focus(FocusState.Programmatic);
        }

        private void CheckBox_Flip(object sender, RoutedEventArgs e)
        {
            vm.AfterCheckboxFlip();
        }

        private void CharListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.NotifyPropertyChanged(nameof(vm.SelChars));
        }
    }
}
