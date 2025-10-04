// Usual RelayCommand for MVVM CommandBinding, awaitable version
// http://jake.ginnivan.net/awaitable-RelayCommand/
//
// 2016-09-15   PV
// 2020-11-11   PV      nullable enable
// 2023-11-20   PV      Net8 C#12 (primary constructor)

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniSearchWinUI3;

// http://jake.ginnivan.net/awaitable-delegatecommand/
public interface IAsyncCommand: IAsyncCommand<object>
{
}

public interface IAsyncCommand<in T>: IRaiseCanExecuteChanged
{
    Task ExecuteAsync(T param);
    bool CanExecute(object param);
    ICommand Command { get; }
}

public partial class AwaitableRelayCommand: AwaitableRelayCommand<object>, IAsyncCommand
{
    public AwaitableRelayCommand(Func<Task> executeMethod)
        : base(o => executeMethod())
    {
    }

    public AwaitableRelayCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        : base(o => executeMethod(), o => canExecuteMethod())
    {
    }
}

public partial class AwaitableRelayCommand<T>(Func<T, Task> executeMethod, Predicate<T>? canExecuteMethod): IAsyncCommand<T>, ICommand
{
    private readonly Func<T, Task> _executeMethod = executeMethod;
    private readonly RelayCommand<T> _underlyingCommand = new(x => { }, canExecuteMethod);
    private bool _isExecuting;

    public AwaitableRelayCommand(Func<T, Task> executeMethod)
        : this(executeMethod, _ => true)
    {
    }

    public async Task ExecuteAsync(T param)
    {
        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _executeMethod(param);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public ICommand Command => this;

    public bool CanExecute(object? parameter) =>
        // PV: Replace null by default(T), otherwise we've problems with RelayCommand<int> when CanExecute is not specified
        !_isExecuting && _underlyingCommand.CanExecute(parameter == null ? default : (T)parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => _underlyingCommand.CanExecuteChanged += value;
        remove => _underlyingCommand.CanExecuteChanged -= value;
    }

    // Should return a Task, but ICommand interface expects void for Execute
    public async void Execute(object? parameter) => await ExecuteAsync((T)parameter!);

    public void RaiseCanExecuteChanged() => _underlyingCommand.RaiseCanExecuteChanged();
}
