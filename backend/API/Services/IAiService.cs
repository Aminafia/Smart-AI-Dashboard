using API.Models;

namespace API.Services
{
    public interface IAiService
    {
        Task<SummarizeResponse> SummarizeAsync(SummarizeRequest request);
    }
}