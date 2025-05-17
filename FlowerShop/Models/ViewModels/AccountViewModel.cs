using FlowerShop.Models.BankEntityFramework;
using System;
namespace FlowerShop.Models.ViewModels
{
    public class AccountViewModel
    {
        public int UserId { get; set; }

        public string AccountName { get; set; }

        public string AccountDesc { get; set; }

        public decimal Balance { get; set; }

        public decimal InterestRate { get; set; }

        public string AccountType { get; set; }
    }
}