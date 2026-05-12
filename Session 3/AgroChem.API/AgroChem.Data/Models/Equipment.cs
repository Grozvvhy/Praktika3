using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsArchived { get; set; } = false;
    }
}