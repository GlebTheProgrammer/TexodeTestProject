using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class InformationCardReadDto
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
    }
}
