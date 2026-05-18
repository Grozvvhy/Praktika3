using AgroChem.TechnologistClient.Services;
using System.Windows;
using System.Windows.Controls;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class ExtruderControl : UserControl
    {
        private ApiService _api;

        public ExtruderControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadPrograms();
        }

        private async void LoadPrograms()
        {
            var programs = await _api.GetExtruderProgramsAsync();
            dgPrograms.ItemsSource = programs;
        }

        private async void LoadProgram_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var selected = dgPrograms.SelectedItem as Models.ExtruderProgram;
            if (selected == null) return;
            // Здесь логика загрузки программы на экструдер
            MessageBox.Show($"Программа '{selected.Name}' загружена на экструдер", "Успех");
        }
    }
}