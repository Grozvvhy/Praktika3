using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;
using AgroChem.TechnologistClient.Models;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class TechMapControl : UserControl
    {
        private ApiService _api;
        private List<Recipe> _recipes;

        public TechMapControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadRecipes();
        }

        private async void LoadRecipes()
        {
            _recipes = await _api.GetRecipesAsync();
            cmbRecipes.ItemsSource = _recipes;
        }

        private async void Recipe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRecipes.SelectedItem is Recipe recipe)
            {
                var steps = await _api.GetProcessStepsAsync(recipe.Id);
                dgSteps.ItemsSource = steps;
            }
        }
    }
}