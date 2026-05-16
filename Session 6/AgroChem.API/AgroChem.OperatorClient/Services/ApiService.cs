using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AgroChem.OperatorClient.Models;

namespace AgroChem.OperatorClient.Services
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

        public async Task<List<ActiveBatch>> GetActiveBatchesAsync()
        {
            var response = await _client.GetAsync("/api/operator/active-batches");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ActiveBatch>>(json);
        }

        public async Task<List<BatchProgramStep>> GetBatchProgramAsync(int batchId)
        {
            var response = await _client.GetAsync($"/api/operator/batch/{batchId}/program");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<BatchProgramStep>>(json);
        }

        public async Task<bool> StartStepAsync(int batchId, int stepOrder)
        {
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/api/operator/batch/{batchId}/step/{stepOrder}/start", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CompleteStepAsync(int batchId, int stepOrder, decimal? actualTemp, decimal? actualPressure, int? actualDuration, string comment)
        {
            var data = new { actualTemp, actualPressure, actualDuration, comment };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/api/operator/batch/{batchId}/step/{stepOrder}/complete", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<TelemetryData> GetTelemetryAsync(string equipmentName)
        {
            var response = await _client.GetAsync($"/api/operator/telemetry/{equipmentName}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TelemetryData>(json);
        }
    }
}