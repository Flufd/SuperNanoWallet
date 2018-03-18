using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SuperNanoWallet.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string seed = "";
        public string Seed
        {
            get { return seed; }
            set
            {
                seed = value;
                RaisePropertyChanged(nameof(Seed));
                RaisePropertyChanged(nameof(SeedValid));
               // (LoginCommand as DelegateCommand<string>)?.RaiseCanExecuteChanged();
            }
        }

        private Visibility newWalletVisibility = Visibility.Hidden;
        public Visibility NewWalletVisibility
        {
            get
            {
                return newWalletVisibility;
            }
            set
            {
                newWalletVisibility = value;
                RaisePropertyChanged("NewWalletVisibility");
            }
        }

        private Visibility existingWalletVisibility = Visibility.Hidden;
        public Visibility ExistingWalletVisibility
        {
            get
            {
                return existingWalletVisibility;
            }
            set
            {
                existingWalletVisibility = value;
                RaisePropertyChanged("ExistingWalletVisibility");
            }
        }

        public bool SeedValid => Seed.Length == 64 && Regex.Match(Seed, "^[a-fA-F0-9]{64}$").Success;

        public bool WalletExists;
        public LoginViewModel()
        {
            if (File.Exists("walletData.snw"))
            {
                WalletExists = true;
                ExistingWalletVisibility = Visibility.Visible;
                NewWalletVisibility = Visibility.Hidden;
            }
            else
            {
                ExistingWalletVisibility = Visibility.Hidden;
                NewWalletVisibility = Visibility.Visible;
            }
        }
    }


}
