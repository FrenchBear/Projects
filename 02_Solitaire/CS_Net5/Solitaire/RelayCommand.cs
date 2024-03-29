﻿// RelayCommand: Helper class for easy implementation of commands in VewModel through delegates
// A simple bonus is the generic interface to support parameter types less abstract than 'object'
//
// 2012-04-17   PV      First version
// 2016-09-26   PV	    Use in Solitaire
// 2020-11-13   PV      Full support for #nullable enable
// 2021-11-13   PV      Net6 C#10

using System;
using System.Windows.Input;

#nullable enable

namespace SolWPF;

internal class RelayCommand<T>: ICommand
{
    private readonly Predicate<T>? canExecute;
    private readonly Action<T> execute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
    {
        this.canExecute = canExecute;
        this.execute = execute;
    }

    // canExecute is optional, and by default is assumed returning true (directly in CanExecute)
    public RelayCommand(Action<T> execute) : this(execute, null)
    { }

    /* From ICommand */

<<<<<<< HEAD:02_Solitaire/CS_Net5/Solitaire/RelayCommand.cs
    public bool CanExecute(object? parameter)
    {
        if (canExecute == null)
            return true;
        if (parameter == null)
            return canExecute(default!);
        else
            return canExecute((T)parameter);
    }

    /* From ICommand */

    public void Execute(object? parameter) => execute?.Invoke((T)parameter!);
=======
    public bool CanExecute(object parameter) => canExecute == null || canExecute((T)parameter);

    /* From ICommand */

    public void Execute(object parameter) => execute?.Invoke((T)parameter);
>>>>>>> MASTER:01_Bonza/CS_Net6/Editor/Support/RelayCommand.cs

    // The 'black magic' part: according to help, CommandManager.RequerySuggested Event occurs when the
    // CommandManager """detects conditions that might change the ability of a command to execute"""...
    // Ok, it works, but exactly how does this detection works is still a mystery to me...
    //
    // Added info from CommandManager.InvalidateRequerySuggested Method:
    // The CommandManager only pays attention to certain conditions in determining when the command target has changed,
    // such as change in keyboard focus. In situations where the CommandManager does not sufficiently determine a change
    // in conditions that cause a command to not be able to execute, InvalidateRequerySuggested can be called to force
    // the CommandManager to raise the RequerySuggested event.

    /* From ICommand */

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
