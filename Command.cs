using System;
using System.Windows.Input;

namespace PlaylistRandomizer
{
    class Command : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Command(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        private readonly Func<object, bool> canExecute;
        public bool CanExecute(object parameter) => this.canExecute == null || this.canExecute(parameter);

        private readonly Action<object> execute;
        public void Execute(object parameter) => this.execute(parameter);
    }
}
