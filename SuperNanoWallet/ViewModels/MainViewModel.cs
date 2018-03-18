using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SuperNanoWallet.Models;
using SuperNanoWallet.Models.LightWallet;
using SuperNanoWallet.Models.WalletConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperNanoWallet.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(WalletConfig walletConfig, string password)
        {
            WalletConfig = walletConfig;
            this.password = password;
            foreach (var account in walletConfig.Accounts)
            {
                Accounts.Add(new Account { AccountNumber = account.AccountNumber, AmountBase = NanoDotNet.AmountBase.Nano });
            }

            if (!walletConfig.Accounts.Any())
            {
                NewAccount();
            }

            foreach (var account in Accounts)
            {
                account.Start();
            }

            // Set next account index
            nextIndex = walletConfig.Accounts.Max(a => a.Index ?? 0) + 1;
        }

        private uint nextIndex = 0;

        private ObservableCollection<Account> accounts = new ObservableCollection<Account>();
        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set { accounts = value; RaisePropertyChanged(nameof(Accounts)); }
        }


        private decimal btcPrice;
        public decimal BTCPrice
        {
            get { return btcPrice; }
            set { btcPrice = value; RaisePropertyChanged(nameof(BTCPrice)); }
        }

        public WalletConfig WalletConfig { get; }
              

        private ICommand newAccountCommand;
        private readonly string password;

        public ICommand NewAccountCommand => GetCommand(ref newAccountCommand, NewAccount);

        private void NewAccount()
        {
            // Create first account from seed
            var acc = NanoDotNet.Utils.GetPublicPrivateKey(WalletConfig.Seed, nextIndex);
            WalletConfig.Accounts.Add(new SavedAccount { AccountNumber = acc.Public.Address, Index = nextIndex, PrivateKey = acc.Private, PublicKey = acc.Public.Bytes });
            var account = new Account { AccountNumber = acc.Public.Address, AmountBase = NanoDotNet.AmountBase.Nano };
            Accounts.Add(account);
            nextIndex++;

            // TODO: encrypt and save config
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(WalletConfig));
            bytes = Encryption.Encrypt(bytes, password);

            File.WriteAllBytes("walletData.snw", bytes);

        }
    }
}
