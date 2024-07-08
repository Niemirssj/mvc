using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Login { get; set; }

        [Required, MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(100)]
        public string Password { get; set; }

        [Required, MaxLength(50)]
        public string AccountType { get; set; } = "user"; // Default value is "user"
    }
}
