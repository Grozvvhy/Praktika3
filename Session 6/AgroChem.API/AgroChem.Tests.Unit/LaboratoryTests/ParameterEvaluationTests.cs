using Xunit;
using AgroChem.Laboratory.Models;

namespace AgroChem.Tests.Unit.LaboratoryTests
{
    public class ParameterEvaluationTests
    {
        [Fact]
        public void EvaluateParameter_ValueWithinRange_ReturnsPass()
        {
            var param = new TestParameter { StandardValue = "6.5-7.0", MeasuredValue = "6.8" };
            param.Evaluate();
            Assert.Equal("✅ pass", param.Result);
        }

        [Fact]
        public void EvaluateParameter_ValueBelowMin_ReturnsFail()
        {
            var param = new TestParameter { StandardValue = ">97%", MeasuredValue = "95" };
            param.Evaluate();
            Assert.Equal("❌ fail", param.Result);
        }

        [Fact]
        public void EvaluateParameter_NonNumericInput_ReturnsNotNumber()
        {
            var param = new TestParameter { StandardValue = "6.5-7.0", MeasuredValue = "abc" };
            param.Evaluate();
            Assert.Equal("❌ не число", param.Result);
        }
    }
}