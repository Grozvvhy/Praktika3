using System.Windows;
using AgroChem.Technologist.Helpers;

namespace AgroChem.Technologist
{
    public partial class EditProductWindow : Window
    {
        public Product Product { get; private set; }

        public EditProductWindow(Product product = null)
        {
            InitializeComponent();
            if (product != null)
            {
                NameBox.Text = product.Name;
                CodeBox.Text = product.Code;
                Product = product;
            }
            else
            {
                Product = new Product();
            }
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            Product.Name = NameBox.Text;
            Product.Code = CodeBox.Text;
            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsArchived { get; set; }
    }
}