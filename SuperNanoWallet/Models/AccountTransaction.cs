using NanoDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNanoWallet.Models
{
    public class AccountTransaction
    {
        public string Account { get; set; }
        public string ShortAccountNumber => Account.Substring(0, 9) + "..." + Account.Substring(Account.Length - 5, 5);
        public NanoAmount Amount { get; set; }
        public string AmountString => Amount.ToString(AmountBase.Nano);
        public TransactionType TransactionType { get; set; }
    }
}
