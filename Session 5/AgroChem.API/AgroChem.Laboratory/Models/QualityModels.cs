using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace AgroChem.Laboratory.Models
{
    public class RawMaterialBatchForQC
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; }
        public string MaterialName { get; set; }
        public decimal QuantityKg { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string QcStatus { get; set; }
    }

    public class FinalProductBatchForQC
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; }
        public string ProductName { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public DateTime? StartTime { get; set; }
        public string Status { get; set; }
        public string QcStatus { get; set; }
    }

    public class QualityStandard
    {
        public string ParameterName { get; set; }
        public string StandardValue { get; set; }
        public string Unit { get; set; }
    }

    public class TestParameter : INotifyPropertyChanged
    {
        private string _measuredValue;
        private string _result;

        public string ParameterName { get; set; }
        public string StandardValue { get; set; }
        public string Unit { get; set; }

        public string MeasuredValue
        {
            get => _measuredValue;
            set { _measuredValue = value; OnPropertyChanged(); OnPropertyChanged(nameof(ResultColor)); }
        }

        public string Result
        {
            get => _result;
            set { _result = value; OnPropertyChanged(); OnPropertyChanged(nameof(ResultColor)); }
        }

        public Brush ResultColor
        {
            get
            {
                if (string.IsNullOrEmpty(Result)) return Brushes.Gray;
                if (Result.Contains("pass") || Result.Contains("✅")) return Brushes.Green;
                if (Result.Contains("fail") || Result.Contains("❌")) return Brushes.Red;
                return Brushes.Gray;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TestParameterDto
    {
        public string ParameterName { get; set; }
        public string MeasuredValue { get; set; }
        public string StandardValue { get; set; }
        public string Unit { get; set; }
        public string Result { get; set; }
    }

    public class SaveTestResultRequest
    {
        public int? BatchId { get; set; }
        public int? RawMaterialBatchId { get; set; }
        public string SampleType { get; set; }
        public List<TestParameterDto> Parameters { get; set; }
        public string Decision { get; set; }
        public string AnalystComment { get; set; }
        public string AnalystName { get; set; }
    }
}