using Xunit;
using AgroChem.Laboratory.Models;

namespace AgroChem.Tests.Laboratory
{
    public class ParameterEvaluationTests
    {
        private void EvaluateParameter(TestParameter param)
        {
            if (string.IsNullOrWhiteSpace(param.MeasuredValue))
            {
                param.Result = "⏳";
                return;
            }

            if (!double.TryParse(param.MeasuredValue, out double val))
            {
                param.Result = "❌ не число";
                return;
            }

            bool pass = false;
            if (param.StandardValue.Contains("-"))
            {
                var parts = param.StandardValue.Split('-');
                if (parts.Length == 2 && double.TryParse(parts[0], out double min) && double.TryParse(parts[1], out double max))
                    pass = val >= min && val <= max;
            }
            else if (param.StandardValue.StartsWith(">"))
            {
                if (double.TryParse(param.StandardValue.Substring(1), out double min))
                    pass = val > min;
            }
            else if (param.StandardValue.StartsWith("<"))
            {
                if (double.TryParse(param.StandardValue.Substring(1), out double max))
                    pass = val < max;
            }
            else
            {
                pass = param.MeasuredValue == param.StandardValue;
            }

            param.Result = pass ? "✅ pass" : "❌ fail";
        }

        [Fact]
        public void EvaluateParameter_ValueWithinRange_ReturnsPass()
        {
            var param = new TestParameter { StandardValue = "6.5-7.0", MeasuredValue = "6.8" };
            EvaluateParameter(param);
            Assert.Equal("✅ pass", param.Result);
        }

        [Fact]
        public void EvaluateParameter_ValueBelowMin_ReturnsFail()
        {
            var param = new TestParameter { StandardValue = ">97%", MeasuredValue = "95" };
            EvaluateParameter(param);
            Assert.Equal("❌ fail", param.Result);
        }

        [Fact]
        public void EvaluateParameter_EmptyValue_ReturnsPending()
        {
            var param = new TestParameter { StandardValue = ">97%", MeasuredValue = "" };
            EvaluateParameter(param);
            Assert.Equal("⏳", param.Result);
        }

        [Fact]
        public void EvaluateParameter_InvalidNumber_ReturnsError()
        {
            var param = new TestParameter { StandardValue = ">97%", MeasuredValue = "abc" };
            EvaluateParameter(param);
            Assert.Equal("❌ не число", param.Result);
        }
    }
}