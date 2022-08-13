using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class InformationCardCreateDto
    {
        [Required]
        public string Image { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
