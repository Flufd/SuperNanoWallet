using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNanoWallet.Models.WalletConfig
{
    public class SavedAccount
    {
        public string AccountNumber { get; set; }
        public string Alias { get; set; }
        public uint? Index { get; set; }
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
    }
}
