using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DotNetAppSqlDb.Models
{
    public class Account
    {
        [Key]
        public string AccountId { get; set; }

        public string Password { get; set; }

        [Index("IX_TypeState", 1)]
        [StringLength(50)]
        public string AccountType { get; set; }

        [Index("IX_TypeState", 2)]
        [StringLength(50)]
        public string ClaimedState { get; set; }
    }


}
