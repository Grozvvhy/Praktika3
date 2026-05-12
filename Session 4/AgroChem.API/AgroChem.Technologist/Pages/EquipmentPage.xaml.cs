using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;
using AgroChem.Technologist.Helpers;

namespace AgroChem.Technologist.Pages
{
    public partial class EquipmentPage : Page
    {
        public EquipmentPage()
        {
            InitializeComponent();
            LoadData();
        }
        private async void LoadData()
        {
            var client = new HttpClient { BaseAddress = new System.Uri(App.ApiBaseUrl) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var resp = await client.GetAsync("api/equipment");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResponse<List<EquipmentDto>>>(json);
                DataGrid.ItemsSource = apiResp.Data;
            }
        }
    }
    public class EquipmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsArchived { get; set; }
    }
}