using System.ComponentModel.DataAnnotations;

namespace Ord.Accounts.Models.Users
{
    public class AccountsUserDeliveryAddress
    {
        public string UnitNumber { get; set; }

        public string ComplexName { get; set; }

        [Required]
        public string StreetAddress { get; set; }

        [Required]
        public string Suburb { get; set; }

        [Required]
        public int PostalCode { get; set; }

        [Required]
        public string Country { get; set; }
    }
}
