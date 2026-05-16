using System;
using System.Windows.Media;

namespace AgroChem.OperatorClient.Models
{
    public class ActiveBatch
    {
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public string CurrentStepName { get; set; }
        public string EquipmentLine { get; set; }
        public string Status { get; set; }
    }

    public class BatchProgramStep
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public string Instruction { get; set; }
        public decimal? PlannedTempC { get; set; }
        public decimal? PlannedPressureBar { get; set; }
        public int? PlannedDurationMin { get; set; }
        public string Status { get; set; }
        public decimal? ActualTempC { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public int? ActualDurationMin { get; set; }
        public string OperatorComment { get; set; }

        public Brush StatusColor
        {
            get
            {
                if (Status == "completed") return Brushes.Green;
                if (Status == "in_progress") return Brushes.Orange;
                return Brushes.Gray;
            }
        }
    }

    public class TelemetryData
    {
        public decimal? Temperature { get; set; }
        public decimal? Pressure { get; set; }
        public int? ScrewSpeed { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}