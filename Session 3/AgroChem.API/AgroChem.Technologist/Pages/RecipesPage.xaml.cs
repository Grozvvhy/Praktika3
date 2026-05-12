using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using AgroChem.Technologist.Helpers;

namespace AgroChem.Technologist.Pages
{
    public partial class RecipesPage : Page
    {
        private readonly HttpClient _client;

        public RecipesPage()
        {
            InitializeComponent();
            _client = new HttpClient { BaseAddress = new Uri(App.ApiBaseUrl) };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            LoadData();
        }

        private async void LoadData()
        {
            var resp = await _client.GetAsync("api/recipes");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResponse<List<RecipeItem>>>(json);
                DataGrid.ItemsSource = apiResp.Data;
            }
        }

        private void OnRefresh(object sender, RoutedEventArgs e) => LoadData();
        private void OnCreate(object sender, RoutedEventArgs e) { }
        private void OnOpen(object sender, RoutedEventArgs e) { }
    }

    public class RecipeItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
    }
}