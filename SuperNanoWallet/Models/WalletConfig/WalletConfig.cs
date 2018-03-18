using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNanoWallet.Models.WalletConfig
{
    public class WalletConfig
    {
        public string Seed { get; set; }

        public List<SavedAccount> Accounts { get; set; } = new List<SavedAccount>();
    }
}
