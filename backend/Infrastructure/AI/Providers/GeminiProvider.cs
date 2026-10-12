/*
GeminiProvider Working:
1. Receive prompt from AiService through IAIProvider
2. Read Gemini API key from configuration
3. Create AIClient using IHttpClientFactory
4. Build GeminiRequest with the prompt
5. Send HTTP request to Gemini API
6. Log response status and response body
7. Extract generated text from GeminiResponse
8. Return generated text
9. If any exception occurs, log the error and return an error message
*/

using System.Net.Http.Json;
using System.Linq;
using Application.Interfaces;
using Infrastructure.AI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.AI.Providers;

public class GeminiProvider : IAIProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiProvider> _logger;

    public GeminiProvider(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GeminiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("AIClient");

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            var request = new GeminiRequest
            {
                Contents =
                [
                    new GeminiContent
                    {
                        Parts =
                        [
                            new GeminiPart
                            {
                                Text = prompt
                            }
                        ]
                    }
                ]
            };

            using var response = await client.PostAsJsonAsync(url, request, cancellationToken);

            _logger.LogInformation(
                "Gemini returned HTTP status {StatusCode}",
                (int)response.StatusCode);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);

            var text = result?.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault()?
                .Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty response.");
            }

            return text;
        }

        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini generation failed.");
            throw;
        }
    }
}
