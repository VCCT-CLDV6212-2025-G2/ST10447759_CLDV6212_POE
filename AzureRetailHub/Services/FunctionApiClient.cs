using AzureRetailHub.Models;
using AzureRetailHub.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureRetailHub.Services
{
    public class FunctionApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public FunctionApiClient(HttpClient httpClient, IOptions<FunctionApiOptions> options)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- Account Functions ---

        public async Task<UserViewModel?> RegisterAsync(RegisterViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/register", model);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<UserViewModel>(_options);
        }

        public async Task<UserViewModel?> LoginAsync(LoginViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<UserViewModel>(_options);
        }

        // --- Product Functions ---

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Product>>("api/products", _options) ?? new List<Product>();
        }

        public async Task<Product?> GetProductByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}", _options);
        }

        public async Task<Product?> CreateProductAsync(Product product)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", product);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Product>(_options);
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{product.Id}", product);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Product>(_options);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _httpClient.DeleteAsync($"api/products/{id}");
        }

        // --- Image Upload Function (from Part 2) ---

        public async Task<string?> UploadProductImageAsync(Stream stream, string fileName)
        {
            stream.Position = 0;
            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync("api/products/image", content);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;
        }

        // --- Order Functions ---

        // DTO for the queue message
        public record OrderItemMessage(string ProductId, int Quantity, decimal Price);
        public record OrderQueueMessage(string UserId, string Status, List<OrderItemMessage> Items);

        public async Task<bool> CreateOrderAsync(OrderQueueMessage orderMessage)
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders", orderMessage);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Order>>("api/orders", _options) ?? new List<Order>();
        }

        public async Task<List<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<List<Order>>($"api/orders/user/{userId}", _options) ?? new List<Order>();
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, string status)
        {
            var payload = new { status };
            var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/status", payload);
            return response.IsSuccessStatusCode;
        }

        // --- Contracts Functions (from Part 1) ---

        public async Task<List<FileDetailDto>> GetContractsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<FileDetailDto>>("api/contracts", _options) ?? new List<FileDetailDto>();
        }

        public async Task<bool> UploadContractAsync(Stream stream, string fileName)
        {
            stream.Position = 0;
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", fileName);
            var response = await _httpClient.PostAsync("api/contracts", content);
            return response.IsSuccessStatusCode;
        }
    }
}