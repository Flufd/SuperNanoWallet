using NanoDotNet;
using SuperNanoWallet.Models.LightWallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
            client.ReceivedWorkEvent += Client_ReceivedWorkEvent;
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
                    TransactionType = a.Type,
                    Hash = a.Hash
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

        private string sendAddress;
        private decimal sendAmount;
        private byte[] privateKey;

        public void Send(string sendAddress, decimal sendAmount, byte[] privateKey)
        {
            // Get the work
            // Hash is the last hash
            var hash = Transactions.First().Hash;
            this.sendAddress = sendAddress;
            this.sendAmount = sendAmount;
            this.privateKey = privateKey;
            client.GenerateWork(hash);
        }

        private void Client_ReceivedWorkEvent(object sender, EventArgs<WorkEvent> e)
        {
            // Create the block
            var work = e.Value.Work;

            // What is the remaining balance
            var sendAmountString = sendAmount + "E" + (int)AmountBase.Nano;
            var sendAmountRaw = BigInteger.Parse(sendAmountString, System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint);
            var remainingRaw = this.Balance.Raw - sendAmountRaw;

            var y = 123;
            // sign a block

            var previous = Transactions.First().Hash;
            //var previous = "1E1EB1C48A42FE1EE245342D8E66FEE5761AEB840FFFE9ACC10199AF6F73939E";
            //var address = "xrb_13cwinwfd8uq65nj5m3hhrt5tmcjmk4a3zu7d737311er1eg6jtsqxwdp4oc";
            var balanceHex = remainingRaw.ToString("X");
            //balanceHex = "0000024840846ED118ABCC068DFFFFFF";
            string hash = Utils.HashSendBlock(previous, sendAddress, balanceHex);
            string sig = Utils.SignHash(hash, privateKey);

            // Send the block
            client.ProcessBlock("send", previous, sendAddress, remainingRaw, work, sig);
        }
    }
}

