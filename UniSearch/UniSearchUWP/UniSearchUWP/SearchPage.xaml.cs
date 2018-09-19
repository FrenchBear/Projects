﻿// UniSearchUWP
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

        // Some controls such as AutoSuggestBox do not get focused just by defining AccessKey property for some reason.
        // This generic helper launched by AccessKeyInvoked event makes it work.
        private void SetFocus(UIElement sender, Windows.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            if (sender is Control c)
                c.Focus(FocusState.Keyboard);
        }

        // Very, very stupid: can't find how to bind command to keyboard accelerator at page level in UWP.
        // Can create bindings on top level control such as Page main grid... but can only lauch an event, not a command!!!
        // So this event actually launches a command...
        private void KeyboardAccelerator_CopyRecordsInvoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            if (vm.CopyRecordsCommand.CanExecute("1"))
                vm.CopyRecordsCommand.Execute("1");
        }
    }
}
