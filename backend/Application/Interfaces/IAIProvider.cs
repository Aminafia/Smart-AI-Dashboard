namespace Application.Interfaces;

public interface IAIProvider
{
    Task<string> GenerateAsync(string prompt);
}