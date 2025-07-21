using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bikya.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace Bikya.Services.Services
{
    
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetBotResponse(string message)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var endpoint = "https://openrouter.ai/api/v1/chat/completions";

            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                new { role = "user", content = message }
            }
            };

            var requestJson = JsonSerializer.Serialize(requestData);
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return "حصل خطأ أثناء الاتصال بـ GPT 😓";

            using var doc = JsonDocument.Parse(responseContent);
            var botMessage = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return botMessage ?? "معنديش رد دلوقتي.";
        }
    }


}
