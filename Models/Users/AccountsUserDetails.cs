using System.ComponentModel.DataAnnotations;

namespace Ord.Accounts.Models.Users
{
    public class AccountsUserDetails
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        public string Email { get; set; }

        [Required]
        public string Mobile { get; set; }
    }
}
