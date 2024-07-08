using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMovie.Models
{
    public class Character
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public UserAccount? UserAccount { get; set; }

        [Required]
        [MaxLength(50)]
        public string CharacterName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Class { get; set; } = string.Empty;

        public int Hp { get; set; } = 0;
        public int Mana { get; set; } = 0;
        public int Strength { get; set; } = 0;
        public int Intelligence { get; set; } = 0;
        public int Dexterity { get; set; } = 0;
        public int Experience { get; set; } = 0;
        public int Level { get; set; } = 1;

        
        [MaxLength(200)]
        public string Avatar { get; set; } = string.Empty;
    }
}
