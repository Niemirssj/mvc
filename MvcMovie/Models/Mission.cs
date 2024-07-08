using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMovie.Models
{
    public class Mission
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string MissionName { get; set; }

        [Required]
        public string Description { get; set; }

        public int MonsterId { get; set; }
        [ForeignKey("MonsterId")]
        public Monster Monster { get; set; }
    }
}
