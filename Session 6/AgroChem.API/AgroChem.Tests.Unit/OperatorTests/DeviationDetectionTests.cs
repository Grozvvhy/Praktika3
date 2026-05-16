using Xunit;
using AgroChem.OperatorClient.Models;

namespace AgroChem.Tests.Unit.OperatorTests
{
    public class DeviationDetectionTests
    {
        [Fact]
        public void CheckDeviation_TemperatureExceeds2Degrees_ReturnsDeviation()
        {
            var step = new BatchProgramStep { PlannedTempC = 80, ActualTempC = 82.5m };
            var result = step.CheckDeviation();
            Assert.True(result.HasDeviation);
            Assert.Equal("temperature_deviation", result.DeviationType);
        }

        [Fact]
        public void CheckDeviation_TemperatureWithinTolerance_NoDeviation()
        {
            var step = new BatchProgramStep { PlannedTempC = 80, ActualTempC = 81.5m };
            var result = step.CheckDeviation();
            Assert.False(result.HasDeviation);
        }

        [Fact]
        public void CheckDeviation_PressureExceeds0_5Bar_ReturnsDeviation()
        {
            var step = new BatchProgramStep { PlannedPressureBar = 3.0m, ActualPressureBar = 3.6m };
            var result = step.CheckDeviation();
            Assert.True(result.HasDeviation);
            Assert.Equal("pressure_deviation", result.DeviationType);
        }
    }
}