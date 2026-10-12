using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI.Providers;

public class OpenAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIProvider> _logger;

    public OpenAIProvider(IHttpClientFactory factory, ILogger<OpenAIProvider> logger)
    {
        _httpClient = factory.CreateClient("AIClient");
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Temporary test endpoint (no API key needed)
            var url = "https://postman-echo.com/post";

            var requestBody = new
            {
                input = prompt
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return $"Processed Response: {json}";
        }
        catch (OperationCanceledException)
    when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI provider failed");
            return "⚠️ AI service temporarily unavailable. Please retry";
        }
    }
}