// Usual RelayCommand for MVVM CommandBinding
//
// 2016-09-26   PV
// 2018-09-15   PV      Adaptation for UWP
// 2020-11-11   PV      #nullable enable
// 2023-11-20   PV      Net8 C#12 (primary constructor)

using System;
using System.Windows.Input;

#nullable enable

namespace RelayCommandNS;

// https://gist.github.com/JakeGinnivan/5166866
public interface IRaiseCanExecuteChanged
{
    void RaiseCanExecuteChanged();
}

// And an extension method to make it easy to raise changed events
public static class CommandExtensions
{
    public static void RaiseCanExecuteChanged(this ICommand command)
    {
        if (command is IRaiseCanExecuteChanged canExecuteChanged)
            canExecuteChanged.RaiseCanExecuteChanged();
    }
}

/// <summary>
/// A command whose sole purpose is to relay its functionality 
/// to other objects by invoking delegates. 
/// The default return value for the CanExecute method is 'true'.
/// <see cref="RaiseCanExecuteChanged"/> needs to be called whenever
/// <see cref="CanExecute"/> is expected to return a different value.
/// </summary>
/// <remarks>
/// Creates a new command.
/// </remarks>
/// <param name="execute">The execution logic.</param>
/// <param name="canExecute">The execution status logic.</param>
public class RelayCommand<T>(Action<T> execute, Predicate<T>? canExecute): ICommand, IRaiseCanExecuteChanged
{
    private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Predicate<T>? _canExecute = canExecute;

    /// <summary>
    /// Raised when RaiseCanExecuteChanged is called.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand(Action<T> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Determines whether this <see cref="RelayCommand{T}"/> can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null) return true;
        return (parameter == null) ? _canExecute(default!) : _canExecute((T)parameter);
    }

    /// <summary>
    /// Executes the <see cref="RelayCommand{T}"/> on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    public void Execute(object parameter) => _execute((T)parameter);

    /// <summary>
    /// Method used to raise the <see cref="CanExecuteChanged"/> event
    /// to indicate that the return value of the <see cref="CanExecute"/>
    /// method has changed.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
