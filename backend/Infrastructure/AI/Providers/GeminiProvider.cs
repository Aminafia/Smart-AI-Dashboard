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

    public async Task<string> GenerateAsync(string prompt)
    {
        try
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API Key is missing.");
            }

            var client = _httpClientFactory.CreateClient("AIClient");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

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

            var response = await client.PostAsJsonAsync(url, request);
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(
                "Gemini Status Code: {StatusCode}",
                response.StatusCode);
            _logger.LogInformation(
                "Gemini Response: {Response}",
                responseBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            var text =
                result?.Candidates
                    ?.FirstOrDefault()
                    ?.Content
                    ?.Parts
                    ?.FirstOrDefault()
                    ?.Text;

            return string.IsNullOrWhiteSpace(text)
                ? "No response generated."
                : text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini provider failed");

            return $"Gemini Error: {ex.Message}";
        }
    }
}