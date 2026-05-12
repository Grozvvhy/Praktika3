using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;
using AgroChem.Technologist.Helpers;

namespace AgroChem.Technologist.Pages
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            LoadData();
        }
        private async void LoadData()
        {
            var client = new HttpClient { BaseAddress = new System.Uri(App.ApiBaseUrl) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var resp = await client.GetAsync("api/users");
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                var apiResp = JsonConvert.DeserializeObject<ApiResponse<List<UserDto>>>(json);
                DataGrid.ItemsSource = apiResp.Data;
            }
        }
    }
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string RoleName { get; set; }
        public bool IsArchived { get; set; }
    }
}