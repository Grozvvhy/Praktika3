using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroChem.Data.Models
{
    public class RecipeComponent
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
        public int RawMaterialId { get; set; }
        public virtual RawMaterial RawMaterial { get; set; }
        [Column(TypeName = "decimal")]
        public decimal Percentage { get; set; }
        public int LoadOrder { get; set; }
    }
}