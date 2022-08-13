using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class InformationCard
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
