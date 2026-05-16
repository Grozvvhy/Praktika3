using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AgroChem.Laboratory.Models;

namespace AgroChem.Laboratory.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public ApiService(string baseUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<(bool Success, string Role, string FullName)> LoginAsync(string username, string password)
        {
            var data = new { username, password };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(json);
            if (result.success == true)
                return (true, result.role?.ToString(), result.fullName?.ToString());
            return (false, null, null);
        }

        public async Task<List<RawMaterialBatchForQC>> GetRawMaterialBatchesAsync()
        {
            var response = await _client.GetAsync("/api/quality/batches?type=raw");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<RawMaterialBatchForQC>>(json);
        }

        public async Task<List<FinalProductBatchForQC>> GetFinalProductBatchesAsync()
        {
            var response = await _client.GetAsync("/api/quality/batches?type=final");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<FinalProductBatchForQC>>(json);
        }

        public async Task<List<QualityStandard>> GetProductStandardsAsync(int productId)
        {
            var response = await _client.GetAsync($"/api/quality/standards?productId={productId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<QualityStandard>>(json);
        }

        public async Task<List<QualityStandard>> GetRawMaterialStandardsAsync(int rawMaterialId)
        {
            var response = await _client.GetAsync($"/api/quality/standards?rawMaterialId={rawMaterialId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<QualityStandard>>(json);
        }

        public async Task<bool> SaveTestResultsAsync(SaveTestResultRequest request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/quality/save", content);
            return response.IsSuccessStatusCode;
        }
    }
}