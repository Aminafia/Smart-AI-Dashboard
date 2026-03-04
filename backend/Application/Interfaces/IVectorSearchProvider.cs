namespace Application.Interfaces;

public interface IVectorSearchProvider
{
    Task<List<string>> SearchSimilarAsync(float[] embedding, int topK = 5);
}