using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;
using AgroChem.Technologist.Helpers;

namespace AgroChem.Technologist.Pages
{
    public partial class BatchesPage : Page
    {
        public BatchesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            var client = new HttpClient { BaseAddress = new Uri(App.ApiBaseUrl) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var resp = await client.GetAsync("api/production-batches");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResponse<List<Batch>>>(json);
                DataGrid.ItemsSource = apiResp.Data;
            }
        }
    }

    public class Batch
    {
        public int Id { get; set; }
        public string RecipeVersion { get; set; }
        public string CardVersion { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; }
        public decimal PlannedQuantity { get; set; }
    }
}