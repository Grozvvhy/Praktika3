using Xunit;
using AgroChem.TechnologistClient.Models; // предполагаем, что модели есть

namespace AgroChem.Tests.Unit.TechnologistTests
{
    public class RecipeValidationTests
    {
        [Fact]
        public void Recipe_WithEmptyName_ShouldBeInvalid()
        {
            var recipe = new Recipe { Name = "" };
            Assert.False(recipe.IsValid());
        }

        [Fact]
        public void RecipeComponent_WithZeroQuantity_ShouldBeInvalid()
        {
            var component = new RecipeComponent { QuantityKg = 0 };
            Assert.False(component.IsValid());
        }

        [Fact]
        public void Recipe_WithNegativeComponentQuantity_ShouldBeInvalid()
        {
            var component = new RecipeComponent { QuantityKg = -10 };
            Assert.False(component.IsValid());
        }
    }
}