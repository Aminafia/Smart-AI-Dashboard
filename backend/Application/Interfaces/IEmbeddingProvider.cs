namespace Application.Interfaces;

public interface IEmbeddingProvider
{
    Task<float[]> GenerateEmbeddingAsync(string text);
}