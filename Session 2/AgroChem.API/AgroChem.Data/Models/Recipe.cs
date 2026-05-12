using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        [MaxLength(20)]
        public string Version { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } // Draft, Approved, Archived
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<RecipeComponent> Components { get; set; }
    }
}