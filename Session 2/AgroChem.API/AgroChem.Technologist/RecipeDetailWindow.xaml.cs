using System.Windows;

namespace AgroChem.Technologist
{
    public partial class RecipeDetailWindow : Window
    {
        private int _recipeId;
        public RecipeDetailWindow(int recipeId)
        {
            InitializeComponent();
            _recipeId = recipeId;
        }
    }
}