using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace AgroChem.Tests.API
{
    public class ApiIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl = "https://localhost:44308";

        public ApiIntegrationTests()
        {
            _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        }

        [Fact]
        public async Task Login_ValidTechnologist_ReturnsSuccess()
        {
            var data = new { username = "tech.ivanov", password = "123456" };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            Assert.True(result.success);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsFalse()
        {
            var data = new { username = "tech.ivanov", password = "wrong" };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            Assert.False(result.success);
        }

        [Fact]
        public async Task GetProducts_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/products");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetActiveBatches_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/operator/active-batches");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetBatchProgram_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/operator/batch/1/program");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}