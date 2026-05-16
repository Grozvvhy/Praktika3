using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace AgroChem.TechnologistClient.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte[] Img { get; set; }
        public BitmapImage Image { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Form { get; set; }
        public bool IsActive { get; set; }
    }

    public class Recipe
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RecipeComponent> Components { get; set; } = new List<RecipeComponent>();
    }

    public class RecipeComponent
    {
        public int Id { get; set; }
        public int RawMaterialId { get; set; }
        public string RawMaterialName { get; set; }
        public decimal QuantityKg { get; set; }
    }

    public class ProcessStep
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public decimal? PlannedTempC { get; set; }
        public decimal? PlannedPressureBar { get; set; }
        public int? PlannedDurationMin { get; set; }
    }

    public class ProductionOrder
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal PlannedQuantityKg { get; set; }
        public string Status { get; set; }
        public DateTime PlannedStartDate { get; set; }
    }

    public class Batch
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int RecipeId { get; set; }
        public string RecipeVersion { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public List<BatchStep> Steps { get; set; } = new List<BatchStep>();
    }

    public class BatchStep
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public decimal? ActualTempC { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public int? ActualDurationMin { get; set; }
        public string OperatorComment { get; set; }
        public bool DeviationFlag { get; set; }
    }

    public class DeviationEvent
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public string BatchNumber { get; set; }
        public DateTime EventTime { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public bool IsResolved { get; set; }
    }

    public class ExtruderProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RecipeId { get; set; }
        public string ParametersJson { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
    }
}