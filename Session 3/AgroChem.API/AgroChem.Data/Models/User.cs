using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; }
        [Required, MaxLength(50)]
        public string Login { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public bool IsArchived { get; set; }
    }
}