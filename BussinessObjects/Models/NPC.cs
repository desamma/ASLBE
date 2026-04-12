using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class NPC
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(50)]
        [Required]
        public string NPCType { get; set; } = string.Empty; // e.g., "Merchant", "QuestGiver", "DialogOnly", "Companion"

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UpdatedDate { get; set; }
    }
}
