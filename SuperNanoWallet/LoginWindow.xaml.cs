using Newtonsoft.Json;
using SuperNanoWallet.Models.WalletConfig;
using SuperNanoWallet.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SuperNanoWallet
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (this.DataContext as LoginViewModel);
            if (vm.SeedValid)
            {
                var main = new MainWindow(new WalletConfig { Seed = vm.Seed }, Password.Password);
                main.Show();
                this.Close();
            }
        }

        private void ExistingButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("walletData.snw"))
            {
                var bytes = File.ReadAllBytes("walletData.snw");
                var decrypted = Encryption.Decrypt(bytes, ExistingPassword.Password);

                var config = JsonConvert.DeserializeObject<WalletConfig>(Encoding.UTF8.GetString(decrypted));

                var main = new MainWindow(config, ExistingPassword.Password);
                main.Show();
                this.Close();
            }
        }
    }
}
