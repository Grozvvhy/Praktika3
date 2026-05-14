using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AgroChem.TechnologistClient.Models;
using Newtonsoft.Json;

namespace AgroChem.TechnologistClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;

        public ApiService(string baseUrl)
        {
            // Игнорируем ошибки SSL сертификата (только для разработки)
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ---------- Аутентификация (без JWT) ----------
        public async Task<bool> LoginAsync(string username, string password)
        {
            var data = new { username, password };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RegisterAsync(string username, string password, string fullName, string email, string phone, string role)
        {
            var data = new { username, password, fullName, email, phone, role };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/register", content);
            return response.IsSuccessStatusCode;
        }

        // ---------- Продукция ----------
        public async Task<List<Product>> GetProductsAsync()
        {
            var response = await _client.GetAsync("/api/products");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Product>>(json);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/products", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Product>(json);
        }

        public async Task<Product> UpdateProductAsync(int id, Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/products/{id}", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Product>(json);
        }

        public async Task DeleteProductAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ---------- Рецептуры ----------
        public async Task<List<Recipe>> GetRecipesAsync()
        {
            var response = await _client.GetAsync("/api/recipes");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Recipe>>(json);
        }

        public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
        {
            var content = new StringContent(JsonConvert.SerializeObject(recipe), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/recipes", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Recipe>(json);
        }

        public async Task<Recipe> UpdateRecipeAsync(int id, Recipe recipe)
        {
            var content = new StringContent(JsonConvert.SerializeObject(recipe), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/recipes/{id}", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Recipe>(json);
        }

        public async Task DeleteRecipeAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/recipes/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ---------- Шаги техкарты ----------
        public async Task<List<ProcessStep>> GetProcessStepsAsync(int recipeId)
        {
            var response = await _client.GetAsync($"/api/recipes/{recipeId}/steps");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProcessStep>>(json);
        }

        public async Task<ProcessStep> CreateProcessStepAsync(int recipeId, ProcessStep step)
        {
            var content = new StringContent(JsonConvert.SerializeObject(step), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"/api/recipes/{recipeId}/steps", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProcessStep>(json);
        }

        public async Task<ProcessStep> UpdateProcessStepAsync(int recipeId, int stepId, ProcessStep step)
        {
            var content = new StringContent(JsonConvert.SerializeObject(step), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/recipes/{recipeId}/steps/{stepId}", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProcessStep>(json);
        }

        public async Task DeleteProcessStepAsync(int recipeId, int stepId)
        {
            var response = await _client.DeleteAsync($"/api/recipes/{recipeId}/steps/{stepId}");
            response.EnsureSuccessStatusCode();
        }

        // ---------- Заказы ----------
        public async Task<List<ProductionOrder>> GetOrdersAsync()
        {
            var response = await _client.GetAsync("/api/orders");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProductionOrder>>(json);
        }

        public async Task<ProductionOrder> CreateOrderAsync(ProductionOrder order)
        {
            var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/orders", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProductionOrder>(json);
        }

        public async Task<ProductionOrder> UpdateOrderAsync(int id, ProductionOrder order)
        {
            var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/orders/{id}", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProductionOrder>(json);
        }

        public async Task DeleteOrderAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/orders/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ---------- Партии ----------
        public async Task<List<Batch>> GetBatchesAsync()
        {
            var response = await _client.GetAsync("/api/batches");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Batch>>(json);
        }

        public async Task<Batch> CreateBatchAsync(Batch batch)
        {
            var content = new StringContent(JsonConvert.SerializeObject(batch), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/batches", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Batch>(json);
        }

        public async Task<Batch> UpdateBatchAsync(int id, Batch batch)
        {
            var content = new StringContent(JsonConvert.SerializeObject(batch), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"/api/batches/{id}", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Batch>(json);
        }

        public async Task DeleteBatchAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/batches/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Batch>> GetActiveBatchesAsync()
        {
            var response = await _client.GetAsync("/api/batches/active");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Batch>>(json);
        }

        // ---------- Отклонения ----------
        public async Task<List<DeviationEvent>> GetDeviationsAsync()
        {
            var response = await _client.GetAsync("/api/deviations");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DeviationEvent>>(json);
        }

        // ---------- Программы экструдера ----------
        public async Task<List<ExtruderProgram>> GetExtruderProgramsAsync()
        {
            var response = await _client.GetAsync("/api/extruder/programs");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ExtruderProgram>>(json);
        }

        public async Task<ExtruderProgram> SaveExtruderProgramAsync(ExtruderProgram program)
        {
            var content = new StringContent(JsonConvert.SerializeObject(program), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/extruder/programs", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ExtruderProgram>(json);
        }

        public async Task DeleteExtruderProgramAsync(int id)
        {
            var response = await _client.DeleteAsync($"/api/extruder/programs/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}