using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class TechnologicalStep
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public virtual TechnologicalCard Card { get; set; }
        public int StepNumber { get; set; }
        [MaxLength(100)]
        public string StepType { get; set; }
        public string ParametersJson { get; set; } // JSON с параметрами
        public bool IsMandatory { get; set; }
    }
}