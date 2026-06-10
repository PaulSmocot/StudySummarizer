using Microsoft.EntityFrameworkCore;
using StudySummarizer.Domain.Entities;
using StudySummarizer.Infrastructure.Data;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Exceptions;
using StudySummarizer.Service.Interfaces;

namespace StudySummarizer.Service.Services;

public class DocumentService : IDocumentService
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".txt"];

    private readonly AppDbContext _db;

    public DocumentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DocumentResponse> UploadAsync(UploadFileInput input)
    {
        if (input.Content is null || input.SizeBytes == 0)
            throw new ValidationException("No file sent or file is empty.");

        if (input.SizeBytes > MaxFileSize)
            throw new ValidationException($"File too large. Maximum {MaxFileSize / 1024 / 1024} MB.");

        var extension = Path.GetExtension(input.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ValidationException($"Invalid type. Accepted: {string.Join(", ", AllowedExtensions)}");

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = input.FileName,
            ContentType = input.ContentType,
            SizeBytes = input.SizeBytes,
            Content = input.Content,
            Status = DocumentStatus.Uploaded,
            UploadedAt = DateTime.UtcNow
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return ToResponse(document);
    }

    public async Task<IEnumerable<DocumentResponse>> GetAllAsync()
    {
        var documents = await _db.Documents
            .Include(d => d.Summary)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        return documents.Select(ToResponse);
    }

    public async Task<DocumentResponse?> GetByIdAsync(Guid id)
    {
        var document = await _db.Documents
            .Include(d => d.Summary)
            .FirstOrDefaultAsync(d => d.Id == id);

        return document is null ? null : ToResponse(document);
    }

    public async Task<DocumentFileResult?> GetFileAsync(Guid id)
    {
        var document = await _db.Documents.FindAsync(id);
        if (document is null)
            return null;

        return new DocumentFileResult(document.Content, document.ContentType, document.FileName);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var document = await _db.Documents.FindAsync(id);
        if (document is null)
            return false;

        _db.Documents.Remove(document);
        await _db.SaveChangesAsync();
        return true;
    }

    private static DocumentResponse ToResponse(Document d) => new(
        d.Id,
        d.FileName,
        d.ContentType,
        d.SizeBytes,
        d.Status,
        d.UploadedAt,
        d.Summary is not null
    );
}
