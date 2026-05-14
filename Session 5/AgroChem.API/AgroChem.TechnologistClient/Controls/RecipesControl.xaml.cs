using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;
using AgroChem.TechnologistClient.Models;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class RecipesControl : UserControl
    {
        private ApiService _api;

        public RecipesControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadRecipes();
        }

        private async void LoadRecipes()
        {
            var recipes = await _api.GetRecipesAsync();
            dgRecipes.ItemsSource = recipes;
        }
    }
}