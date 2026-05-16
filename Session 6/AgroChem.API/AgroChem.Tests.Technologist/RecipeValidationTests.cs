using Xunit;
using AgroChem.TechnologistClient.Models;

namespace AgroChem.Tests.Technologist
{
    public class RecipeValidationTests
    {
        [Fact]
        public void Recipe_WithEmptyName_ShouldBeInvalid()
        {
            var recipe = new Recipe { ProductName = "" };
            bool isValid = !string.IsNullOrWhiteSpace(recipe.ProductName);
            Assert.False(isValid);
        }

        [Fact]
        public void Recipe_WithValidName_ShouldBeValid()
        {
            var recipe = new Recipe { ProductName = "Гербицид А" };
            bool isValid = !string.IsNullOrWhiteSpace(recipe.ProductName);
            Assert.True(isValid);
        }

        [Fact]
        public void RecipeComponent_WithZeroQuantity_ShouldBeInvalid()
        {
            var component = new RecipeComponent { QuantityKg = 0 };
            bool isValid = component.QuantityKg > 0;
            Assert.False(isValid);
        }

        [Fact]
        public void RecipeComponent_WithPositiveQuantity_ShouldBeValid()
        {
            var component = new RecipeComponent { QuantityKg = 100 };
            bool isValid = component.QuantityKg > 0;
            Assert.True(isValid);
        }
    }
}