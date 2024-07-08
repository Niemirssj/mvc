using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models
{
    public class Monster
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public int Hp { get; set; }
        public int Mana { get; set; }
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Dexterity { get; set; }
        public int ExperienceReward { get; set; }
    }
}
