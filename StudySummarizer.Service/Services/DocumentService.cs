using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudySummarizer.Domain.Entities;
using StudySummarizer.Infrastructure.Data;
using StudySummarizer.Service.Dtos;
using StudySummarizer.Service.Interfaces;
using ValidationException = StudySummarizer.Service.Exceptions.ValidationException;

namespace StudySummarizer.Service.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IValidator<UploadFileInput> _uploadValidator;

    public DocumentService(AppDbContext db, IValidator<UploadFileInput> uploadValidator)
    {
        _db = db;
        _uploadValidator = uploadValidator;
    }

    public async Task<DocumentResponse> UploadAsync(UploadFileInput input)
    {
        var validation = await _uploadValidator.ValidateAsync(input);
        if (!validation.IsValid)
            throw new ValidationException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));

        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = input.FileName,
            ContentType = input.ContentType,
            SizeBytes = input.SizeBytes,
            Content = input.Content,
            Status = DocumentStatus.Uploaded,
            CreatedAt = DateTime.UtcNow
        };

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return ToResponse(document);
    }

    public async Task<IEnumerable<DocumentResponse>> GetAllAsync()
    {
        var documents = await _db.Documents
            .Include(d => d.Summaries)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return documents.Select(ToResponse);
    }

    public async Task<DocumentResponse?> GetByIdAsync(Guid id)
    {
        var document = await _db.Documents
            .Include(d => d.Summaries)
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
        d.CreatedAt,
        d.Summaries.Count
    );
}
