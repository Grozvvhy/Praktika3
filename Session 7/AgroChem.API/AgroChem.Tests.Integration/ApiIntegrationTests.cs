using AgroChem.API; // пространство имён вашего API (Startup)
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AgroChem.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        // --- Позитивные тесты ---

        [Fact]
        public async Task Auth_Login_ValidTechnologist_ReturnsSuccess()
        {
            var request = new { username = "tech.ivanov", password = "123456" };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            Assert.True(result.success);
            Assert.Equal("technologist", result.role.ToString());
        }

        [Fact]
        public async Task Operator_GetActiveBatches_ReturnsOkAndNotEmpty()
        {
            var response = await _client.GetAsync("/api/operator/active-batches");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var batches = JsonConvert.DeserializeObject<List<object>>(json);
            Assert.NotNull(batches);
        }

        [Fact]
        public async Task Quality_GetRawMaterialBatches_ReturnsOk()
        {
            var response = await _client.GetAsync("/api/quality/batches?type=raw");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // --- Негативные тесты ---

        [Fact]
        public async Task Auth_Login_InvalidPassword_ReturnsFalse()
        {
            var request = new { username = "tech.ivanov", password = "wrong" };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);
            Assert.False(result.success);
            Assert.Equal("Неверный пароль", result.message.ToString());
        }

        [Fact]
        public async Task Quality_SaveTestResults_MissingBatchId_ReturnsBadRequest()
        {
            var invalidRequest = new { sampleType = "final_product", parameters = new object[] { } };
            var content = new StringContent(JsonConvert.SerializeObject(invalidRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/quality/save", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}