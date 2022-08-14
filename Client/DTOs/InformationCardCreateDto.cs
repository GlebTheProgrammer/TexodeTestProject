using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.DTOs
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
