using Xunit;
using AgroChem.OperatorClient.Models;

namespace AgroChem.Tests.Operator
{
    public class DeviationDetectionTests
    {
        private bool CheckTemperatureDeviation(decimal? actual, decimal? planned)
        {
            if (!actual.HasValue || !planned.HasValue) return false;
            return System.Math.Abs(actual.Value - planned.Value) > 2.0m;
        }

        private bool CheckPressureDeviation(decimal? actual, decimal? planned)
        {
            if (!actual.HasValue || !planned.HasValue) return false;
            return System.Math.Abs(actual.Value - planned.Value) > 0.5m;
        }

        [Fact]
        public void TemperatureDeviation_ExceedsTolerance_ReturnsTrue()
        {
            bool result = CheckTemperatureDeviation(83, 80);
            Assert.True(result);
        }

        [Fact]
        public void TemperatureDeviation_WithinTolerance_ReturnsFalse()
        {
            bool result = CheckTemperatureDeviation(81, 80);
            Assert.False(result);
        }

        [Fact]
        public void PressureDeviation_ExceedsTolerance_ReturnsTrue()
        {
            bool result = CheckPressureDeviation(3.8m, 3.0m);
            Assert.True(result);
        }

        [Fact]
        public void PressureDeviation_WithinTolerance_ReturnsFalse()
        {
            bool result = CheckPressureDeviation(3.2m, 3.0m);
            Assert.False(result);
        }

        [Fact]
        public void DeviationCheck_NullValues_ReturnsFalse()
        {
            bool result = CheckTemperatureDeviation(null, 80);
            Assert.False(result);
        }
    }
}