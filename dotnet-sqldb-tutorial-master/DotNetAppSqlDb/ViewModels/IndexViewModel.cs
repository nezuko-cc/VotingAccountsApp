using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DotNetAppSqlDb.ViewModels
{
    public class IndexViewModel
    {
        public int Count { get; set; }

        public AccountType? AccountType { get; set; }

        public string AdminSecretToken { get; set; }

        public string ErrorMsg { get; set; }
    }

    public enum AccountType
    {
        Normal,
        Urgent,
        Bad,
        WrongPassword,
        Other
    }
}
