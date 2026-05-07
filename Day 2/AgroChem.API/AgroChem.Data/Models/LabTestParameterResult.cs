using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class LabTestParameterResult
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public virtual LabTest Test { get; set; }
        public string ParameterName { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal? ActualValue { get; set; }
        public string Decision { get; set; }
    }
}