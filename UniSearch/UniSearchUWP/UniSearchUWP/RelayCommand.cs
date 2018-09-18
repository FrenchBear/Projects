// UnISearch RelayCommand for MVVM CommandBinding
// 2016-09-26   PV

using System;
using System.Windows.Input;

namespace UniSearchUWP
{

    /// <summary>
    /// A command whose sole purpose is to relay its functionality 
    /// to other objects by invoking delegates. 
    /// The default return value for the CanExecute method is 'true'.
    /// <see cref="RaiseCanExecuteChanged"/> needs to be called whenever
    /// <see cref="CanExecute"/> is expected to return a different value.
    /// </summary>
    public class RelayCommand<T> : ICommand, IRaiseCanExecuteChanged
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        /// <summary>
        /// Raised when RaiseCanExecuteChanged is called.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand"/> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        /// <summary>
        /// Method used to raise the <see cref="CanExecuteChanged"/> event
        /// to indicate that the return value of the <see cref="CanExecute"/>
        /// method has changed.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }



    internal class ZZRelayCommand<T> : ICommand
    {
        private readonly Predicate<T> canExecute;
        private readonly Action<T> execute;

        public ZZRelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        // canExecute is optional, and by default is assumed returning true (directly in CanExecute)
        public ZZRelayCommand(Action<T> execute)
            : this(execute, null)
        { }

        /* From ICommand */

        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke((T)parameter) ?? true;
        }

        /* From ICommand */

        public void Execute(object parameter)
        {
            execute?.Invoke((T)parameter);
        }



        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

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

        // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        //public event EventHandler CanExecuteChanged
        //{
        //    //add { CommandManager.RequerySuggested += value; }
        //    //remove { CommandManager.RequerySuggested -= value; }
        //    add { }
        //    remove { }
        //}
    }
}