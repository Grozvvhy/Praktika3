using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class RawMaterial
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Code { get; set; }
        public bool IsArchived { get; set; }
    }
}