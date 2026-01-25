namespace Core.AI.Interfaces;

public interface IAIProvider
{
    Task<string> ExecuteAsync(string prompt);
}
