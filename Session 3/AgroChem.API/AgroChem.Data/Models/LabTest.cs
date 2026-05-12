using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgroChem.Data.Models
{
    public class LabTest
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public virtual ProductionBatch Batch { get; set; }
        public DateTime AssignedAt { get; set; }
        public string TestType { get; set; }
        public virtual ICollection<LabTestParameterResult> Results { get; set; }
    }
}