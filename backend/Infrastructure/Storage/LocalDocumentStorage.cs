using Application.DTOs.Documents;
using Application.Interfaces;

namespace Infrastructure.Storage;

public class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _documentsPath;

    public LocalDocumentStorage()
    {
        _documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Documents");
        Directory.CreateDirectory(_documentsPath);
    }

    public async Task<string> SaveAsync(DocumentUpload document)
    {
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(document.FileName)}";

        var fullPath = Path.Combine(_documentsPath, storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

        await document.Content.CopyToAsync(stream);

        return storedFileName;
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = Path.Combine(_documentsPath, storagePath);

        if (File.Exists(fullPath)) {
            File.Delete(fullPath); }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string storagePath)
    {
        var fullPath = Path.Combine(_documentsPath, storagePath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return Task.FromResult(stream);
    }
}