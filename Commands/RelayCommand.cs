﻿using System.Windows.Input;

namespace WebView2Traffic.Commands
{
    /// <summary>  
    /// A command whose sole purpose is to relay its functionality   
    /// to other objects by invoking delegates.   
    /// The default return value for the CanExecute method is 'true'.  
    /// <see cref="RaiseCanExecuteChanged"/> needs to be called whenever  
    /// <see cref="CanExecute"/> is expected to return a different value.  
    /// </summary>  
    public class RelayCommand : ICommand
    {
        #region Private members  
        /// <summary>  
        /// Creates a new command that can always execute.  
        /// </summary>  
        private readonly Action<object> _execute;

        /// <summary>  
        /// True if command is executing, false otherwise  
        /// </summary>  
        private readonly Predicate<object> _canExecute;
        #endregion

        /// <summary>  
        /// Initializes a new instance of <see cref="RelayCommand"/> that can always execute.  
        /// </summary>  
        /// <param name="execute">The execution logic.</param>  
        public RelayCommand(Action<object> execute) : this(execute, canExecute: null) { }

        /// <summary>  
        /// Initializes a new instance of <see cref="RelayCommand"/>.  
        /// </summary>  
        /// <param name="execute">The execution logic.</param>  
        /// <param name="canExecute">The execution status logic.</param>  
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>  
        /// Raised when RaiseCanExecuteChanged is called.  
        /// </summary>  
        public event EventHandler CanExecuteChanged;

        /// <summary>  
        /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.  
        /// </summary>  
        /// <param name="parameter">  
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.  
        /// </param>  
        /// <returns>True if this command can be executed; otherwise, false.</returns>  
        public bool CanExecute(object parameter) => _canExecute == null ? true : _canExecute(parameter);

        /// <summary>  
        /// Executes the <see cref="RelayCommand"/> on the current command target.  
        /// </summary>  
        /// <param name="parameter">  
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.  
        /// </param>  
        public void Execute(object parameter) => _execute(parameter);

        /// <summary>  
        /// Method used to raise the <see cref="CanExecuteChanged"/> event  
        /// to indicate that the return value of the <see cref="CanExecute"/>  
        /// method has changed.  
        /// </summary>  
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
