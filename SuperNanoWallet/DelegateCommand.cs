using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperNanoWallet
{
    public class DelegateCommand : ICommand
    {
        private readonly Action action;
        private readonly Func<bool> executionPredicate;
        private EventHandler _canExecuteChanged;

        public DelegateCommand(Action action, Func<bool> executionPredicate)
        {
            this.action = action;
            this.executionPredicate = executionPredicate;
        }

        public void Execute(object parameter)
        {
            action();
        }

        public bool CanExecute(object parameter)
        {
            return executionPredicate();
        }

        public event EventHandler CanExecuteChanged 
        {
            add
            {
                _canExecuteChanged += value;
            }
            remove
            {
                _canExecuteChanged -= value;
            }
        }

        public void RaiseCanExecuteChanged()
        {
             _canExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
