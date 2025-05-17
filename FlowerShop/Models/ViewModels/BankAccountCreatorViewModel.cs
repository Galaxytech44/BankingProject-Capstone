using FlowerShop.Models.BankEntityFramework;
using System.Collections.Generic;

namespace FlowerShop.Models.ViewModels
{
    public class BankAccountCreatorViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public IEnumerable<Account> Accounts { get; set; }
    }
}