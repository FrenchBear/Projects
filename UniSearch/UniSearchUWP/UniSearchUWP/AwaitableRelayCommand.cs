// Usual RelayCommand for MVVM CommandBinding, awaitable version
// http://jake.ginnivan.net/awaitable-RelayCommand/
//
// 2016-09-15   PV

using System;
using System.Threading.Tasks;
using System.Windows.Input;


namespace RelayCommandNS
{
    // http://jake.ginnivan.net/awaitable-delegatecommand/
    public interface IAsyncCommand : IAsyncCommand<object>
    {
    }

    public interface IAsyncCommand<in T> : IRaiseCanExecuteChanged
    {
        Task ExecuteAsync(T param);
        bool CanExecute(object param);
        ICommand Command { get; }
    }

    public class AwaitableRelayCommand : AwaitableRelayCommand<object>, IAsyncCommand
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

    public class AwaitableRelayCommand<T> : IAsyncCommand<T>, ICommand
    {
        private readonly Func<T, Task> _executeMethod;
        private readonly RelayCommand<T> _underlyingCommand;
        private bool _isExecuting;

        public AwaitableRelayCommand(Func<T, Task> executeMethod)
            : this(executeMethod, _ => true)
        {
        }

        public AwaitableRelayCommand(Func<T, Task> executeMethod, Predicate<T> canExecuteMethod)
        {
            _executeMethod = executeMethod;
            _underlyingCommand = new RelayCommand<T>(x => { }, canExecuteMethod);
        }

        public async Task ExecuteAsync(T obj)
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _executeMethod(obj);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public ICommand Command { get { return this; } }

        public bool CanExecute(object parameter)
        {
            // PV: Replace null by default(T), otherwise we've problems with RelayCommand<int> when CanExecute is not specified
            return !_isExecuting && _underlyingCommand.CanExecute(parameter==null ? default(T) : (T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { _underlyingCommand.CanExecuteChanged += value; }
            remove { _underlyingCommand.CanExecuteChanged -= value; }
        }



        // Should return a Task, but ICommand interface expects void for Execute
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            await ExecuteAsync((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            _underlyingCommand.RaiseCanExecuteChanged();
        }
    }
}
