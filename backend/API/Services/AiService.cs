using System.Net.Http.Json;
using API.Models;

namespace API.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;

        public AiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SummarizeResponse> SummarizeAsync(SummarizeRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/summarize", request);

            if(!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("AI service failed");
            }

            var result = await response.Content.ReadFromJsonAsync<SummarizeResponse>();
            
            return result ?? new SummarizeResponse();
        }
    }
}