using NanoDotNet;
using SuperNanoWallet.Models.LightWallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNanoWallet.Models
{
    public class Account : ObservableObject
    {
        private string accountNumber;
        public string AccountNumber
        {
            get { return accountNumber; }
            set { accountNumber = value; RaisePropertyChanged(nameof(AccountNumber)); }
        }

        public string ShortAccountNumber => AccountNumber.Substring(0, 9) + ".." + AccountNumber.Substring(AccountNumber.Length - 5, 5);

        private NanoAmount balance;
        public NanoAmount Balance
        {
            get { return balance; }
            set
            {
                balance = value;
                RaisePropertyChanged(nameof(Balance));
                RaisePropertyChanged(nameof(BalanceString));
            }
        }

        private ObservableCollection<AccountTransaction> transactions;
        public ObservableCollection<AccountTransaction> Transactions
        {
            get { return transactions; }
            set
            {
                transactions = value;
                RaisePropertyChanged(nameof(Transactions));
            }
        }


        public string BalanceString => $"{Balance.ToString(AmountBase)} {AmountBase}";


        private AmountBase amountBase;

        public AmountBase AmountBase
        {
            get { return amountBase; }
            set
            {
                amountBase = value;
                RaisePropertyChanged(nameof(AmountBase));
                RaisePropertyChanged(nameof(BalanceString));
            }
        }


        public void Start()
        {
            // find the account
            client = new WalletEndpointClient("light.nano.org");
            client.ReceivedExchangeRateEvent += Client_ReceivedExchangeRateEvent;
            client.WalletStartComplete += Client_WalletStartComplete;
            client.ReceivedAccountSummaryEvent += Client_ReceivedAccountSummaryEvent;
            client.ReceivedAccountHistoryEvent += Client_ReceivedAccountHistoryEvent;

            client.Start();
        }

        private void Client_ReceivedAccountHistoryEvent(object sender, EventArgs<AccountHistoryEvent> e)
        {
            // Convert history to transactions
            if (e.Value.History != null)
            {
                var transactions = new ObservableCollection<AccountTransaction>(
                e.Value.History.Select(a => new AccountTransaction
                {
                    Account = a.Account,
                    Amount = new NanoAmount(a.Amount, AmountBase.raw),
                    TransactionType = a.Type
                }));
                Transactions = transactions;
            }
        }

        private async void Client_ReceivedAccountSummaryEvent(object sender, EventArgs<AccountSummaryEvent> e)
        {
            Balance = new NanoAmount(e.Value.Balance, AmountBase.raw);
            await client.GetAccountHistory(AccountNumber, e.Value.BlockCount);
        }

        private async void Client_WalletStartComplete(object sender, EventArgs e)
        {
            await client.RegisterAccount(AccountNumber);
        }

        private void Client_ReceivedExchangeRateEvent(object sender, EventArgs<ExchangeRateEvent> e)
        {

        }

        private WalletEndpointClient client;

    }
}

