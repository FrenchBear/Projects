// http://jake.ginnivan.net/awaitable-RelayCommand/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UniSearchUWP
{
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
    


    // http://jake.ginnivan.net/awaitable-delegatecommand/
    public interface IAsyncCommand : IAsyncCommand<object>
    {
    }

    public interface IAsyncCommand<in T> : IRaiseCanExecuteChanged
    {
        Task ExecuteAsync(T obj);
        bool CanExecute(object obj);
        ICommand Command { get; }
    }

    public class AwaitableDelegateCommand : AwaitableRelayCommand<object>, IAsyncCommand
    {
        public AwaitableDelegateCommand(Func<Task> executeMethod)
            : base(o => executeMethod())
        {
        }

        public AwaitableDelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
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
            //ToDo: Rewrite to avoid exception with T=int and parameter=null
            return !_isExecuting && _underlyingCommand.CanExecute(parameter==null ? default(T) : (T)parameter);
            /*
            try
            {
                return !_isExecuting && _underlyingCommand.CanExecute((T)parameter);
            }
            catch (Exception)
            {
                return !_isExecuting;
            }
            */
        }

        public event EventHandler CanExecuteChanged
        {
            add { _underlyingCommand.CanExecuteChanged += value; }
            remove { _underlyingCommand.CanExecuteChanged -= value; }
        }


        // Should return a Task, but ICommand interface expects void for Execute
        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            _underlyingCommand.RaiseCanExecuteChanged();
        }
    }
}
