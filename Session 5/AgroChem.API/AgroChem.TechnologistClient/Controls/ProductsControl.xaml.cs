using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using AgroChem.TechnologistClient.Models;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class ProductsControl : UserControl
    {
        private ApiService _api;
        private List<Product> _products;
        private Product _currentProduct;

        public ProductsControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadProducts();
        }

        private async void LoadProducts()
        {
            _products = await _api.GetProductsAsync();
            dgProducts.ItemsSource = _products;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            _currentProduct = new Product();
            editPanel.Visibility = Visibility.Visible;
            txtName.Text = "";
            txtType.Text = "";
            txtForm.Text = "";
            chkActive.IsChecked = true;
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgProducts.SelectedItem as Product;
            if (selected == null) return;
            _currentProduct = selected;
            txtName.Text = selected.Name;
            txtType.Text = selected.Type;
            txtForm.Text = selected.Form;
            chkActive.IsChecked = selected.IsActive;
            editPanel.Visibility = Visibility.Visible;
        }

        private async void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            _currentProduct.Name = txtName.Text;
            _currentProduct.Type = txtType.Text;
            _currentProduct.Form = txtForm.Text;
            _currentProduct.IsActive = chkActive.IsChecked ?? false;

            if (_currentProduct.Id == 0)
                await _api.CreateProductAsync(_currentProduct);
            else
                await _api.UpdateProductAsync(_currentProduct.Id, _currentProduct);

            editPanel.Visibility = Visibility.Collapsed;
            LoadProducts();
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e) => editPanel.Visibility = Visibility.Collapsed;

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgProducts.SelectedItem as Product;
            if (selected == null) return;
            if (MessageBox.Show("Удалить выбранную продукцию?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _api.DeleteProductAsync(selected.Id);
                LoadProducts();
            }
        }
    }
}