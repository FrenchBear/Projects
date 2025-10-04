// UniSearch RelayCommand for MVVM CommandBinding
//
// 2016-09-26   PV
// 2020-01-02   PV      v3 Not Net Core
// 2020-11-13   PV      Full support for #nullable enable
// 2023-11-19   PV      1.8 Net8 C#12 (primary constructor)

using System;
using System.Windows.Input;

namespace UniSearch;

internal class RelayCommand<T>(Action<T> execute, Predicate<T>? canExecute = null): ICommand
{
    private readonly Predicate<T>? MyCanExecute = canExecute;
    private readonly Action<T> MyExecute = execute;

    public bool CanExecute(object? parameter)
    {
        if (MyCanExecute == null)
            return true;
        return parameter == null ? MyCanExecute(default!) : MyCanExecute((T)parameter);
    }

    public void Execute(object? parameter) 
        => MyExecute.Invoke((T)parameter!);

    // The 'black magic' part: according to help, CommandManager.RequerySuggested Event occurs when the
    // CommandManager """detects conditions that might change the ability of a command to MyExecute"""...
    // Ok, it works, but exactly how does this detection works is still a mystery to me...
    //
    // Added info from CommandManager.InvalidateRequerySuggested Method:
    // The CommandManager only pays attention to certain conditions in determining when the command target has changed,
    // such as change in keyboard focus. In situations where the CommandManager does not sufficiently determine a change
    // in conditions that cause a command to not be able to MyExecute, InvalidateRequerySuggested can be called to force
    // the CommandManager to raise the RequerySuggested event.

    /* From ICommand */

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}