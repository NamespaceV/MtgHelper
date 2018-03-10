namespace ScryfallAPI
{
    using System;
    using System.Windows.Input;

    public class SimpleCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;
        public SimpleCommand(Action execute, Func<bool> canExecute = null)
        {
            if (canExecute == null)
            {
                canExecute = (() => true);
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void UpdateCanExecute()
        {
            CanExecuteChanged(this, new EventArgs());
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
