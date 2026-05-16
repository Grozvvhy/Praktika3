using System;
using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class ProductionBatch
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
        public int CardId { get; set; }
        public virtual TechnologicalCard Card { get; set; }
        public DateTime StartDate { get; set; }
        [MaxLength(30)]
        public string Status { get; set; } // Planned, InProgress, QualityControl, Completed
        public decimal PlannedQuantity { get; set; }
    }
}