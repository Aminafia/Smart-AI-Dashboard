using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;

namespace Infrastructure.AI.Providers;

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;

    public OpenAIProvider(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AIClient");
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        // Temporary test endpoint (no API key needed)
        var url = "https://postman-echo.com/post";

        var requestBody = new
        {
            input = prompt
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("AI API failed");
        }

        var json = await response.Content.ReadAsStringAsync();

        return $"Processed Response: {json}";
    }
}