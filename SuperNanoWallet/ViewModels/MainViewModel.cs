using Newtonsoft.Json.Serialization;
using SuperNanoWallet.Models;
using SuperNanoWallet.Models.LightWallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SuperNanoWallet.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        //private bool x = true;
        //public void Test()
        //{
        //    x = false;
        //    (TestCommand as DelegateCommand).RaiseCanExecuteChanged();
        //}

        //private ICommand testCommand;
        //public ICommand TestCommand => GetCommand(ref testCommand, Test, () => x);

        public MainViewModel()
        {
            Accounts.Add(new Account { AccountNumber = "aasdasd" });
            Accounts.Add(new Account { AccountNumber = "aasdasd1231" });

            var client = new WalletEndpointClient("light.nano.org");

            client.ReceivedExchangeRateEvent += Client_ReceivedExchangeRateEvent;

            client.Start();

        }

        private void Client_ReceivedExchangeRateEvent(object sender, EventArgs<ExchangeRateEvent> e)
        {
            BTCPrice = e.Value.BTC;
        }

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



    }
}
