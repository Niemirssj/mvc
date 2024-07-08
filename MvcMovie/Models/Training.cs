using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMovie.Models
{
    public class Training
    {
        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }
        [ForeignKey("CharacterId")]
        public Character Character { get; set; }

        [Required]
        public DateTime CompletionDate { get; set; }

        [Required, MaxLength(50)]
        public string StatName { get; set; }

        public int StatPoints { get; set; }

        [Required]
        public bool IsCompleted { get; set; }
    }
}
