using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperNanoWallet.ViewModels
{
    public class BaseViewModel : ObservableObject
    {
        protected ICommand GetCommand(ref ICommand reference, Action action, Func<bool> executionPredicate)
        {
            if (reference == null)
            {
                reference = new DelegateCommand(action, executionPredicate);
            }
            return reference;
        }

        protected ICommand GetCommand(ref ICommand reference, Action action)
        {
            return GetCommand(ref reference, action, () => true);
        }

        protected ICommand GetCommand<T>(ref ICommand reference, Action<T> action, Func<bool> executionPredicate)
        {
            if (reference == null)
            {
                reference = new DelegateCommand<T>(action, executionPredicate);
            }
            return reference;
        }

        protected ICommand GetCommand<T>(ref ICommand reference, Action<T> action)
        {
            return GetCommand(ref reference, action, () => true);
        }
    }
}
